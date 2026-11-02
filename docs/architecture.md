# AgentForge System Architecture

AgentForge uses modular Clean Architecture. The system has shared execution/capability contracts and independently composed agent modules. The public module package is the only surface an application should need for ordinary use.

## 1. Architectural overview

```text
Application / Host
    |
    +---- AgentForge.Agents.SimpleReflex
    +---- AgentForge.Agents.ReAct
    +---- AgentForge.Agents.BDI
    +---- AgentForge.Agents.Research
    |
    +---- AgentForge.Orchestration
    +---- AgentForge.MultiAgent.Supervisor
    |
    +---- optional capabilities
          Models / Providers / Tools / Memory / Retrieval / RAG
          Evaluation / Security / Observability / Protocols
```

Agent modules are independently installable and do not call one another's concrete implementation classes. Composition happens through shared agent contracts and public handles.

## 2. One agent type per module

Examples:

- `AgentForge.Agents.SimpleReflex`
- `AgentForge.Agents.ModelBasedReflex`
- `AgentForge.Agents.GoalBased`
- `AgentForge.Agents.UtilityBased`
- `AgentForge.Agents.BDI`
- `AgentForge.Agents.ReAct`
- `AgentForge.Agents.PlanAndExecute`
- `AgentForge.Agents.Reflection`
- `AgentForge.Agents.Supervisor` or architecture-specific multi-agent modules.

The module name expresses the architecture or pattern implemented. Search algorithms and capabilities receive separate package families because they can be reused by many agents.

## 3. Clean Architecture inside each module

A module normally follows this shape:

```text
AgentForge.Agents.<Type>/
  Domain/
  Application/
  Infrastructure/       # Optional; only when external adapters are needed
  Public API / Builders # Composition root
```

Where compile-time isolation is useful, Domain and Application live in separate projects. The public module project references those layers and exposes the fluent builder. Infrastructure is a separate project only when the module needs a provider, persistence, transport, or other external adapter.

### Dependency direction

```text
Domain <----- Application <----- Infrastructure
   ^                ^                    |
   |                |                    |
   +------ Public API / Composition Root+
```

- **Domain** owns entities, value objects, policies, and domain decisions. It has no provider, runtime, Infrastructure, or cross-agent dependency.
- **Application** owns the agent's use cases and control loop. It depends on its Domain and defines ports for external capabilities.
- **Infrastructure** implements those ports. It points inward and does not own agent decision logic.
- **Public API / composition root** exposes fluent builders, validated options, optional DI registration, and the configured run handle.

An Infrastructure project is not mandatory. Simple Reflex and Model-Based Reflex need no provider or storage adapter: their host supplies percepts/state and applies proposed actions. Adding an empty Infrastructure project would weaken rather than improve the architecture.

## 4. Shared contracts and capabilities

The shared layer must stay small:

| Module | Responsibility |
|---|---|
| `AgentForge.Abstractions` | Generic agent contract, normalized run outcomes, run context, limits, events, concise AgentHandle |
| `AgentForge.Runtime` | Optional common runtime services: budgets, lifecycle, middleware, context/state infrastructure |
| `AgentForge.Models` | Provider-neutral model contracts and normalized chat types |
| `AgentForge.Tools` | Tool schemas, registry, validation, authorization, execution |
| `AgentForge.Memory.*` | Memory contracts and store adapters |
| `AgentForge.Retrieval.*` | Retrieval contracts and lexical/vector/hybrid adapters |
| `AgentForge.RAG` | Retrieval-context-generation composition and provenance |
| `AgentForge.Search.*` | Search algorithms and reusable search strategies |
| `AgentForge.Planning.*` | Planning models, algorithms, and planners |
| `AgentForge.Optimization.*` | Optimization and metaheuristic strategies |
| `AgentForge.ReinforcementLearning.*` | Environments, policies, algorithms, training and evaluation support |
| `AgentForge.Orchestration` | Workflow composition |
| `AgentForge.MultiAgent.*` | Multi-agent coordination architectures |
| `AgentForge.Evaluation` | Quality, safety, and regression evaluation |
| `AgentForge.Security` | Shared trust-boundary and policy mechanisms |
| `AgentForge.Observability` | Events, traces, metrics, correlation, redaction |
| `AgentForge.Providers.*` | Provider adapters |
| `AgentForge.Protocols.*` | MCP, A2A, and other protocol integrations |

These are target package boundaries. Planned names do not imply that packages already exist.

## 5. Fluent API and composition

Each agent module owns its fluent builder. Shared code can standardize option validation, naming, metadata, and the execution handle, but agent-specific methods remain local.

```csharp
var agent = ReActAgentBuilder
    .Create()
    .WithModel(model)
    .WithTools(tools => tools.Add(orderLookup))
    .WithAuthorization(policy)
    .WithMaxModelTurns(8)
    .Build();

var result = await agent.RunAsync("Find order 123.", cancellationToken);
```

The example shows the intended API direction. The current ReAct module is still being migrated to this builder contract.

A user can compose independent modules with a workflow or multi-agent architecture. The orchestrator depends on `IAgent<TInput,TOutput>` or the public handle, never on a concrete ReAct/BDI/Research implementation.

## 6. Provider independence

LLM-based agents depend on provider-neutral contracts. Provider adapters sit outside the agent's Domain and Application layers.

The design should support adapters built on `Microsoft.Extensions.AI` and optional provider-specific integrations. No provider package is mandatory for deterministic agents.

A provider adapter implements model capabilities; an agent module decides when those capabilities are required. A model-generated tool call remains only a proposal until validated and authorized by the tool boundary.

## 7. State and concurrency

- Run context is transient and scoped to one execution.
- Persistent memory and agent-session state are separate capabilities.
- Agent instances should be immutable after Build and safe for concurrent runs unless the module explicitly documents session-local state.
- Stateful agents should receive a session/state handle or explicit state input, not hide mutable per-user state in a singleton.
- The host controls persistence, tenant scope, and state lifetime.

## 8. Testing strategy

Each module should have:

- **Domain tests:** invariants, rules, decisions, and state transitions.
- **Application tests:** use-case behavior, cancellation, budgets, failures, and partial outcomes.
- **Architecture tests:** reference direction and forbidden cross-module dependencies.
- **Infrastructure tests:** adapters and external boundaries, only when present.
- **Public API tests:** builder validation and composition.
- **Scenario tests:** reproducible tasks with deterministic fakes.

A full solution build and test run is required before a module can be marked implemented.

## 9. Current migration state

The Simple Reflex and Model-Based Reflex modules are the first examples of separate Domain/Application/public projects and fluent builders. The previous combined `AgentForge.Agents.Classical` module is being retired.

ReAct, Planning, and Reflection already have separate top-level agent projects, but their internal layer boundaries and fluent public APIs still need migration. Their current experimental implementations are not yet examples of the final module template.
