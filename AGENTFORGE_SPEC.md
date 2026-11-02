# AgentForge Product and Architecture Specification

**Status:** Draft — evolving with implementation  
**Scope:** .NET / C# library, modular agent packages, shared runtime, capabilities, and composition.

## 1. Product definition

AgentForge is a modular .NET library for building, configuring, executing, and composing different types of agents.

Its primary unit of modularity is the **agent module**. Each independently meaningful agent architecture or pattern has a dedicated module with its own public API, Domain, Application use cases, and optional Infrastructure adapters.

The library MUST make common agent construction straightforward through module-specific fluent APIs. A user SHOULD be able to install only the agent modules and capabilities they need.

AgentForge is not an educational course or a generic wrapper around an LLM. Documentation and samples exist to make the library usable, but the product is the reusable software.

## 2. Normative language

**MUST**, **MUST NOT**, **SHOULD**, **SHOULD NOT**, and **MAY** express requirement strength. Public APIs remain experimental until a separate stabilization decision.

## 3. Domain taxonomy

These categories MUST remain distinct:

- **Agent:** a component with a decision or control responsibility that maps percepts/state and goals to proposed or performed actions.
- **Agent architecture:** the structure of perception, state, decision, planning, and learning; examples include Reflex, Goal-Based, Utility-Based, BDI, Hybrid, and Learning Agent.
- **Agent pattern:** a reusable control strategy; examples include ReAct, Plan-and-Execute, Reflection, Reflexion, Router, and CodeAct.
- **Algorithm:** a concrete procedure that can be used by an agent; examples include A*, Minimax, MCTS, Q-Learning, PPO, and Genetic Algorithms.
- **Capability:** reusable functionality such as model access, tools, memory, retrieval, RAG, evaluation, or observability.
- **Tool:** a declared operation with an input/output contract, validation, authorization, side-effect profile, and execution policy.
- **Protocol / integration:** a communication boundary such as model-provider APIs, MCP, browser interfaces, or agent-to-agent protocols.
- **Workflow:** an explicit sequence or graph of operations. It can coordinate agents without itself being autonomous.
- **Multi-agent architecture:** a system-level topology coordinating multiple agents through delegation, messaging, collaboration, competition, or negotiation.
- **Runtime:** shared execution mechanics such as context, identity, budgets, cancellation, events, middleware, and state lifecycle.

A project MUST NOT be named an agent merely because it calls an LLM or appears in a workflow.

## 4. Agent module boundaries

Each independently meaningful agent type MUST have its own module/package. A module MUST expose a stable public composition boundary and MUST NOT require consumers to reference its internal Domain or Application implementations directly.

A typical module contains:

- **Domain:** entities, value objects, rules, policies, and domain decisions. It MUST NOT depend on Infrastructure, provider SDKs, or another agent module.
- **Application:** use cases and ports that implement the agent's control loop. It MAY depend on the module's Domain and shared AgentForge contracts. It MUST depend on interfaces for external capabilities rather than concrete Infrastructure classes.
- **Infrastructure:** provider, storage, transport, clock, or external-system adapters. It is optional when the agent has no infrastructure concerns. It depends inward on Application/Domain.
- **Public API / composition root:** fluent builders, validated configuration, factories, and optional dependency-injection registration. It composes Application with selected Infrastructure adapters and exposes the module's public agent handle.

Domain and Application SHOULD be separate projects when the module has enough behavior to justify compile-time dependency boundaries. An empty Infrastructure project MUST NOT be created solely to satisfy a folder convention.

## 5. Dependency direction

The required direction is inward:

```text
Application / Host
        |
        v
Agent module public API / composition root
        |
        +----> Application use cases ----> Domain
        |
        +----> Infrastructure adapters --> Application ports
        |
        +----> optional AgentForge capabilities
```

Rules:

- Domain MUST NOT reference Application, Infrastructure, provider SDKs, or other agent modules.
- Application MUST NOT reference Infrastructure implementations.
- Infrastructure MUST implement Application-defined ports or shared capability contracts.
- The public API/composition root MAY reference all layers of its own module.
- One agent module MUST NOT reference another agent module's concrete classes.
- Orchestration MUST compose through shared agent contracts and public handles.
- Optional capabilities MUST NOT become mandatory dependencies of unrelated agent modules.

## 6. Shared execution contract

Agent modules MUST integrate with a small shared execution contract. Common concerns include:

- Typed input and output.
- Run identity and parent/child correlation.
- Normalized completed, partially completed, failed, rejected, cancelled, timed-out, and budget-exhausted outcomes.
- Optional run limits and event sinks.
- Cooperative cancellation.
- Privacy-safe lifecycle events.

A configured module SHOULD expose a concise execution method such as:

```csharp
var result = await agent.RunAsync(input, cancellationToken);
```

Advanced hosts MAY supply explicit context, budgets, and event sinks.

## 7. Fluent API contract

Every agent module MUST provide a module-specific fluent builder unless its construction is already a simple immutable constructor with no meaningful configuration.

Builders MUST:

- Use discoverable method names such as `WithModel`, `WithTool`, `WithMemory`, `WithStateUpdater`, `When`, and `WithMaxSteps`, as appropriate to the agent.
- Return the same builder type from configuration methods to support chaining.
- Validate required configuration and incompatible options at `Build()`.
- Produce an immutable agent configuration.
- Avoid hidden network calls or side effects during `Build()`.
- Return a common run handle with concise and advanced execution overloads.
- Expose advanced customization without forcing it on ordinary users.

AgentForge MUST NOT create one enormous universal builder with dozens of irrelevant options. Module-specific builders MAY reuse common builder infrastructure.

## 8. Model-provider independence

Classical/deterministic agents MUST NOT depend on model abstractions unless their architecture genuinely requires them.

LLM-based agent modules SHOULD consume provider-neutral contracts. Optional capabilities MAY include tool calling, structured output, streaming, embeddings, multimodal content, token counting, and provider metadata.

Provider-specific SDKs MUST live in integration modules. Agent modules MUST NOT reference a vendor SDK directly.

## 9. Capabilities and integration packages

Capabilities SHOULD be independently installable and composable:

- Tools and authorization.
- Memory and session state.
- Retrieval and RAG.
- Planning/search/optimization algorithms.
- Model/provider adapters.
- Evaluation and safety.
- Observability and telemetry.
- MCP and agent-to-agent protocols.
- Browser/computer-use and code-execution sandboxes.
- Workflow and multi-agent orchestration.

A capability MUST define its own contracts, data boundaries, failure semantics, security implications, and optional dependencies.

## 10. State and side effects

The module MUST make state ownership explicit. A reusable agent instance SHOULD be safe for concurrent use unless explicitly documented otherwise.

Transient run state MUST be distinguishable from persistent memory. The host or configured store MUST control persistence and lifecycle.

Model-generated tool requests are proposals, not authorization decisions. External side effects MUST pass through validation, authorization, and execution policies.

## 11. Execution limits and resilience

Every iterative agent MUST document and enforce relevant limits, including steps, model requests, tool calls, tokens, time, cost, or delegation depth.

Implementations MUST document termination, cancellation, partial progress, failure, retry, and recovery behavior. Nested agents and workflows SHOULD use aggregate budgets rather than independent unlimited limits.

## 12. Testing and quality

Each module MUST include:

- Domain unit tests.
- Application use-case tests.
- Public API/builder validation tests.
- Integration tests for actual adapters when applicable.
- Deterministic fakes for models, tools, stores, and clocks where relevant.
- A runnable example demonstrating construction and execution.

Architecture tests SHOULD enforce dependency direction and prevent references to unrelated agent modules.

## 13. Product completion and release

An agent is not considered implemented because a type or package exists. Completion requires working behavior, tests, documentation, example usage, and reproducible build evidence.

NuGet publication is a separate release decision after API, dependency, compatibility, security, and support policies are reviewed.
