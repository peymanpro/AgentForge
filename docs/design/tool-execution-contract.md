# Tool Execution Contract — Design Note

**Status:** Proposal; no public API is approved  
**Initial target:** .NET / C#  
**Scope:** Tool contracts and execution semantics for the AgentForge .NET solution

## 1. Problem

Agents become useful when they can interact with application functionality: query a database, call an internal API, retrieve a document, create a ticket, or perform another operation.

This creates a critical trust boundary. A model may propose a tool call, but the model does not decide whether the application is authorized to perform that operation. Tool inputs may be malformed or unsafe, tools may have side effects, and tool outputs may contain untrusted content.

AgentForge needs a tool system that is predictable, testable, provider-independent, and safe to compose with different agent implementations.

## 2. Design goals

The tool system should:

1. Separate tool declaration from tool execution.
2. Describe a tool with a stable name, purpose, input contract, and output contract.
3. Validate model-supplied input before invoking application code.
4. Allow the application to control which tools are available in a given execution.
5. Make authorization and human approval separate from model decision-making.
6. Bound execution with cancellation, timeout, and resource policies.
7. Represent failures explicitly.
8. Keep provider-specific function schemas out of application tool implementations.
9. Support idiomatic typed APIs in each language.
10. Make tool invocation observable without logging sensitive arguments or results by default.

## 3. Trust model

A tool call passes through several distinct trust boundaries:

- **Model output:** untrusted. A tool call is only a proposal.
- **Tool declaration:** application-owned metadata and schema.
- **Authorization policy:** decides whether the current principal/context may perform the action.
- **Input validation:** checks that the proposed arguments match the declared contract.
- **Tool implementation:** application code or an external integration that may have side effects.
- **Tool result:** external or application-produced data; still untrusted when passed back into a model context.

The model must not be able to grant itself permission, expand its own tool allowlist, bypass validation, or turn a denied operation into an approved one.

## 4. Recommended logical architecture

```text
                    Agent / Workflow
                           |
                    Model tool proposal
                           |
                    Normalize tool call
                           |
                    Resolve allowed tool
                           |
                 +---------+----------+
                 |                    |
             Validate input      Authorize request
                 |                    |
                 +---------+----------+
                           |
                 Optional human approval
                           |
                    Bounded executor
                           |
                    Tool implementation
                           |
                  Normalize tool result
                           |
                  Append observation
                           |
                     Agent resumes
```

The implementation may organize these responsibilities differently, but the observable trust boundaries must remain clear.

## 5. Tool declaration

A tool declaration should describe:

- A stable, unique name within its registry.
- A concise description for the model and developer.
- An input schema or typed input contract.
- An output contract.
- Whether the operation is read-only or has side effects, where this can be declared reliably.
- Optional metadata needed for policy, documentation, or observability.

The description shown to a model is not a security policy. Authorization must be enforced independently.

### Typed implementations

Language implementations should use idiomatic type systems. In .NET, a generic interface may be appropriate:

```csharp
public interface ITool<TInput, TOutput>
{
    string Name { get; }
    string Description { get; }

    Task<TOutput> ExecuteAsync(
        TInput input,
        ToolExecutionContext context,
        CancellationToken cancellationToken = default);
}
```

This is a conceptual sketch only. It does not settle the exact public interface, schema-generation mechanism, or package placement.

The external boundary must still validate deserialized model input before it reaches a typed implementation.

### Schema representation

The project needs a language-neutral way to communicate the input shape to model adapters and conformance tests.

A JSON Schema-compatible subset is a candidate because it is widely understood by model-provider APIs and tooling. However, providers support different subsets and typed languages may have richer validation needs.

The design should distinguish:
1. The application's canonical input contract.
2. The provider-specific schema sent to a model.
3. The runtime validator used before tool execution.

The provider adapter may transform the canonical schema into a supported provider representation. That transformation must not silently remove constraints that are security- or correctness-relevant. If a provider cannot represent a required constraint, the runtime remains responsible for enforcing it.

## 6. Tool registration and availability

Tool registration should be explicit and scoped.

A tool registry or collection should:
- Reject conflicting names unless an explicit replacement mechanism exists.
- Expose only tools selected by the application for the current agent or workflow.
- Avoid implicit global mutable registries.
- Support a stable lookup by tool name.
- Allow adapters to produce provider-specific tool definitions from canonical declarations.

An agent should not automatically receive every tool registered in the application.

Availability is not authorization. A tool may be visible to a model while a particular invocation still requires a policy check or approval.

## 7. Authorization and approval

Authorization must be an application/runtime responsibility.

The policy input may include:
- The requested tool and validated arguments.
- The current execution context.
- Application-defined principal, tenant, or permission context.
- Tool risk classification and side-effect metadata.
- Remaining time or resource budget.

The public contract should not force one authorization framework. Applications may integrate their existing policy engine, role system, attribute-based access control, or custom callback.

The policy outcome should distinguish at least:
- Allowed.
- Denied.
- Requires external approval.

Approval is a separate step. It may be immediate, asynchronous, or unsupported by a specific runtime. If approval is required but no approval handler is configured, execution must fail closed.

A prompt instructing the model to "ask permission" is not a substitute for an application-enforced approval boundary.

## 8. Execution semantics

Tool execution should have an explicit context carrying only information needed by the tool and the runtime. Candidate fields include:
- Correlation or run identifier.
- Application-provided principal or scoped services.
- Remaining execution budget.
- Idempotency key, when the application can define meaningful idempotency.
- Tool-specific options.

Secrets and credentials should be provided through secure application dependencies, not generated by the model or placed in the tool description.

The executor should support cancellation and a configurable timeout. A timeout does not always guarantee that the underlying remote operation has stopped; the tool adapter must document cancellation guarantees and any possible late side effects.

### Concurrency

The initial default should be serial tool execution per agent run unless the agent/runtime explicitly opts into parallel execution.

Parallel execution introduces ordering, resource, side-effect, and partial-failure concerns. It should be introduced with explicit concurrency limits and a defined result aggregation policy.

### Retries and idempotency

The tool executor must not blindly retry arbitrary operations.

- Read-only operations may be retryable under a documented policy.
- Side-effecting operations require an idempotency strategy or explicit application approval before retries.
- A timeout after a request was sent may leave the outcome unknown.
- The executor should distinguish a known failure from an unknown completion state where possible.

Retry policy should be visible and bounded by the overall agent/workflow budget.

## 9. Tool result contract

A tool result should be normalized so that the agent can safely append it as an observation.

The result may contain:
- A success or failure status.
- Structured data or text content.
- A content type or representation hint.
- Optional metadata safe for the agent runtime.
- A stable tool-call correlation identifier.

Tool output is not automatically trusted. The agent should not interpret tool output as instructions that override system or application policy.

The design must also distinguish:
- A tool returning a business-level error as a valid result.
- An execution failure such as timeout, exception, or unavailable dependency.
- An authorization denial.
- An invalid model-generated request.

The model should receive only the information required to continue its task. Sensitive internal exception details should remain in protected diagnostics rather than being exposed as observations.

## 10. Error taxonomy

The normalized error model should distinguish at least:

- Unknown tool.
- Duplicate or invalid tool declaration.
- Invalid input or schema mismatch.
- Authorization denied.
- Approval required.
- Approval rejected or unavailable.
- Timeout.
- Cancellation.
- Tool execution failure.
- Output serialization or normalization failure.
- Resource or concurrency limit reached.
- Completion outcome unknown after an external side effect may have started.

Language-specific exception types may differ, but the semantic category should be documented and testable.

## 11. Observability and privacy

The runtime should emit structured events for meaningful transitions, such as:
- Tool call proposed.
- Tool resolved.
- Validation passed or failed.
- Authorization decision.
- Approval requested or resolved.
- Execution started.
- Execution completed or failed.
- Result returned to the agent.

Events should support correlation and duration measurement. Tool arguments, results, credentials, and personal data must not be logged by default. Applications may opt into carefully controlled diagnostic capture with redaction and access controls.

Observability must not change tool behavior or expose additional privileges.

## 12. Testing strategy

The tool system should have tests for:

### Declaration and registration
- Valid tool declaration.
- Duplicate tool names.
- Invalid or unsupported schemas.
- Registry scoping and allowlists.

### Invocation and policy
- Valid invocation.
- Invalid input rejected before application code runs.
- Unknown tool rejected.
- Authorization denied.
- Approval requested, approved, rejected, and unavailable.
- A model-generated request cannot bypass policy.

### Execution and resilience
- Success and business-level error results.
- Execution exception.
- Cancellation before and during execution.
- Timeout.
- Concurrency and resource limits.
- Retry policy and idempotency behavior.
- Unknown completion outcome for a side-effecting call.

### Integration
- Provider-native tool-call normalization.
- Tool-call round-trip through the agent runtime.
- Deterministic fake tools for agent tests.
- Sensitive values absent from default logs.

## 13. Alternatives considered

### Option A — Let the model adapter execute tools

**Rejected.** It couples model-provider code to application permissions and side effects, and makes security policies difficult to enforce consistently.

### Option B — A simple dictionary of function delegates

**Useful for prototypes, insufficient as the only public design.** It does not by itself define schemas, validation, authorization, output semantics, cancellation, or safe observability.

### Option C — Tool contracts plus a policy-controlled executor

**Recommended for prototyping.** It preserves language-native typed implementations while defining clear, testable execution boundaries.

## 14. Open design questions

1. Should a canonical tool schema be a restricted JSON Schema profile, a language-neutral schema model, or both?
2. Should the tool contract be generic in every language, or should some implementations offer a simpler dynamically typed alternative as well?
3. Where should approval handlers live: the tool executor, workflow runtime, or a separate policy/approval service?
4. How should tools declare read-only versus side-effecting behavior, and how much should be advisory versus enforced?
5. Should serial execution be mandatory for the first release, or merely the default?
6. What is the standard representation of partial results and unknown completion outcomes?
7. Which execution events belong in the shared specification versus language-specific diagnostics?

## 15. Decision status

This note recommends **declarative tool contracts with validation, application-controlled authorization, bounded execution, and normalized results**.

It is a design proposal, not an approved public API. Prototype it with:
- One deterministic in-process tool.
- One side-effecting fake tool.
- One tool with a schema validation failure.
- One tool whose execution is cancelled or times out.
- A ReAct loop using native tool calls from a fake model.

Promote the design into the shared specification only after the trust boundaries and error semantics are tested.
