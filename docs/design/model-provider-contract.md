# Model Provider Contract — Design Note

**Status:** Proposal; no public API is approved  
**Initial target:** .NET / C#  
**Cross-language scope:** Shared semantics for all AgentForge implementations

## 1. Problem

AgentForge agents need to ask language models to generate responses, select actions, evaluate outputs, and sometimes return structured or streamed results.

Model providers do not expose identical capabilities. Some support native tool calling, constrained structured output, embeddings, streaming, or multimodal inputs; others do not, or expose them through different APIs.

A model abstraction that assumes every provider supports every feature becomes difficult or misleading. An abstraction that exposes only plain text may be too weak for real agent execution.

The design must preserve provider replaceability without erasing meaningful provider capabilities.

## 2. Design goals

The model integration layer should:

1. Let an application supply its own provider adapter.
2. Give agents a small, stable baseline contract.
3. Represent optional capabilities explicitly.
4. Normalize common request, response, usage, and error semantics.
5. Keep tool execution outside the model adapter.
6. Propagate cancellation.
7. Avoid leaking vendor SDK types into core agent contracts.
8. Remain practical to implement with idiomatic .NET abstractions and provider SDKs.

## 3. Non-goals

The first contract does not need to:

- Recreate every vendor API or provider-specific option.
- Guarantee identical tokenization, model quality, or output behavior across providers.
- Make every provider support tool calling, embeddings, streaming, or multimodal content.
- Own conversation memory or durable agent state.
- Execute tools requested by a model.
- Hide all provider-specific behavior behind a lowest-common-denominator interface.

Provider adapters may expose advanced provider-specific extensions outside the minimal shared contract, provided the core remains independent of those SDKs.

## 4. Recommended conceptual separation

Use one baseline chat-generation contract and separate optional capability contracts.

```text
                 Agent / Workflow
                        |
              Model capability selection
                        |
          +-------------+------------------+
          |             |                  |
      Chat model    Tool calling     Structured output
          |          capability        capability
          |             |                  |
          +-------------+------------------+
                        |
                Provider adapter
                        |
             Provider SDK / HTTP API
```

Streaming and embeddings are additional optional capabilities. An embedding model need not implement the chat model contract.

This is a conceptual model, not a final interface hierarchy.

## 5. Baseline chat contract

The baseline operation should accept a normalized model request and return a normalized model response.

The request should be able to represent:
- Ordered messages with explicit roles.
- Text content in the initial version.
- Generation options that have provider-neutral meaning.
- An optional model identifier or deployment selector when the adapter supports it.
- Cancellation through the language's standard cancellation mechanism.

The response should be able to represent:
- Assistant message content.
- A normalized finish reason.
- Token or usage information when available.
- Provider/model metadata when available and safe to expose.
- A normalized error outcome when the call fails.

The base contract should not assume that the adapter owns the conversation thread. The agent or workflow should own execution state and pass the necessary messages for each model call.

A possible .NET-shaped sketch is:

```csharp
public interface IChatModel
{
    Task<ModelResponse> GenerateAsync(
        ModelRequest request,
        CancellationToken cancellationToken = default);
}
```

This sketch is illustrative. It does not establish final names, members, nullability, serialization, or package placement.

## 6. Optional capability contracts

An implementation should expose optional capabilities through discoverable contracts rather than requiring every provider adapter to implement every feature.

Candidate capabilities:

| Capability | Possible contract | Purpose |
|---|---|---|
| Native tool calling | `IToolCallingModel` | Submit tool definitions and receive normalized tool-call requests |
| Structured output | `IStructuredOutputModel` | Request schema-constrained or otherwise guaranteed structured output |
| Streaming | `IStreamingChatModel` | Receive response chunks or normalized stream events |
| Embeddings | `IEmbeddingModel` | Convert text or supported content into embedding vectors |
| Multimodal input/output | Separate content capability contracts | Handle image, audio, video, or other content parts |

The names are placeholders. The design must decide whether a capability extends the baseline chat interface or is represented by a separate interface that accepts the same request types.

### Capability discovery

Capabilities should be discoverable from the adapter's actual implemented contracts or an equivalent explicit capability descriptor.

Do not rely on a loose boolean such as `SupportsTools = true` unless the callable contract that implements the feature is also available.

An agent must handle missing capabilities explicitly. It may:
- Select a different algorithm or strategy.
- Use a supported fallback where one is defined and safe.
- Return a clear unsupported-capability error.

It must not silently pretend that a provider supports a feature.

## 7. Tool calling must not equal tool execution

The model adapter may normalize a request to call a tool. It must not directly execute the application's tool.

The intended boundary is:

```text
Agent
  |
  | tool definitions / permitted tools
  v
Model adapter
  |
  | normalized tool-call request
  v
Agent runtime
  |
  | validate input and check application policy
  v
Tool executor
  |
  | tool result
  v
Agent runtime
  |
  | send observation back to the model
  v
Model adapter
```

A model-generated tool call is a proposal, not an authorization decision. Tool permissions, validation, side effects, and execution policy belong to the application/runtime boundary.

### Native tool calling and fallback strategies

Native tool calling is one possible capability, not a universal assumption.

The agent layer may support multiple decision strategies, such as:
- Provider-native tool calling.
- Structured-output-based action selection, when the provider supports it.
- A separately tested prompt-and-parse strategy, when appropriate.

These strategies must be explicit and independently tested. A fallback must not weaken validation or authorization.

## 8. Request and response semantics

The detailed types remain open, but the following principles should guide their design.

### Messages

The normalized conversation model should distinguish at least:
- System or instruction messages.
- User messages.
- Assistant messages.
- Tool-result messages.

The final contract may need developer messages, multiple content parts, tool-call identifiers, and provider-specific metadata. It should add these intentionally rather than mirroring one vendor's wire format.

### Generation options

Only options with a sufficiently clear cross-provider meaning should be part of the common request. Examples may include maximum output tokens, temperature, and stop sequences, subject to provider support.

Provider-specific options should use an extension mechanism that does not introduce vendor SDK dependencies into core packages.

The contract must document when an option is unsupported, ignored, or approximated by an adapter. Silent semantic changes should be avoided.

### Responses and finish reasons

The response should distinguish at least normal completion, tool-call handoff, token or budget limit, and provider interruption where these states can be observed.

The adapter should not fabricate usage or metadata that the provider did not return. Optional fields should remain absent or explicitly unknown.

## 9. Errors, cancellation, and retries

The model layer should normalize useful error categories without discarding diagnostic context:

- Authentication or authorization failure.
- Invalid request or unsupported parameter.
- Rate limit or quota exhaustion.
- Provider/network failure.
- Timeout.
- Unsupported model capability.
- Provider-side content or safety refusal, where exposed.

Sensitive provider payloads, credentials, and raw prompts must not be included in exception messages or logs by default.

Cancellation should propagate through the adapter. The runtime should own overall run budgets and timeouts.

Retry behavior should be explicit. A retry policy should consider idempotency, rate limits, provider guidance, and the agent's total budget. Hidden, unbounded retries are not acceptable.

## 10. State and responsibility boundaries

The model adapter should be responsible for communicating with the model service and normalizing results.

The agent runtime should be responsible for:
- Conversation and execution state.
- Agent lifecycle and control loop.
- Tool selection policy and tool execution.
- Step, time, and resource budgets.
- Cancellation across the complete run.
- Execution events and traces.

Memory stores, retrieval, and workflow persistence should remain separate capabilities. They should not become implicit responsibilities of a chat model interface.

## 11. Testing strategy

A model adapter should have:
- Contract tests for request/response normalization.
- Tests for optional capabilities it claims to implement.
- Tests for unsupported capabilities.
- Error mapping tests.
- Cancellation tests.
- Tests that verify vendor SDK types do not leak into core public contracts.

Agent tests should normally use deterministic fake model implementations. This makes the execution algorithm testable without live provider calls, credentials, network access, or nondeterministic model output.

Where behavior is shared by AgentForge .NET modules, contract tests should verify observable semantics rather than internal object layout.

## 12. Alternatives considered

### Option A — One large universal model interface

A single interface requires chat generation, streaming, tool calling, structured output, embeddings, and multimodal support.

**Rejected as the default.** Providers do not support all capabilities, and mandatory members would encourage fake implementations, exceptions, or lowest-common-denominator semantics.

### Option B — Plain text completion only

A single prompt string and text response are the entire abstraction.

**Not sufficient for the intended MVP.** It loses role structure, tool-call handoff, finish reasons, usage, and the semantics needed by interactive agents. It could exist as a minimal adapter for limited use cases, not as the only model contract.

### Option C — Baseline chat contract plus optional capabilities

A compact baseline is supplemented by typed or otherwise explicit optional capabilities.

**Recommended for prototyping.** This is the best current balance between a usable agent foundation and truthful representation of provider differences. The exact interfaces should be validated through ReAct and Planning prototypes before acceptance.

## 13. Prototype questions

Before adopting a stable contract, prototype the following:

1. A text-only model adapter that implements only the baseline contract.
2. A native tool-calling adapter.
3. An adapter with structured output but no native tool calling.
4. A streaming adapter.
5. An embedding-only adapter.
6. A deterministic fake adapter used by ReAct, Planning, and Reflection tests.

The design is acceptable only if agents can use the capabilities they require without forcing unrelated providers to implement unsupported features.

## 14. Decision status

This note recommends **baseline chat generation plus explicit optional capabilities**, but does not approve a public API.

The next design step is a small .NET prototype around ReAct and the tool-call boundary. The resulting contracts should be checked against the idioms and requirements of the .NET ecosystem before being promoted into the AgentForge specification.
