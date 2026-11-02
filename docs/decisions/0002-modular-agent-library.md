# ADR 0002: Make AgentForge a Modular Agent Library

- **Status:** Accepted
- **Date:** 2026-11-02
- **Supersedes:** the educational-codebase framing in earlier project documents
- **Decision owners:** AgentForge maintainers

## Context

AgentForge is intended to help application developers build and compose many different kinds of agents. The earlier repository direction emphasized learning and a broad catalog. That description does not express the product requirement: the repository must deliver reusable agent modules with simple public APIs.

The existing `AgentForge.Agents.Classical` package combined multiple independent architectures, while other agent projects held their decision logic and supporting code in single assemblies. That makes module boundaries and dependency direction difficult to enforce.

## Decision

AgentForge is a **modular .NET agent library**, not an educational course or learning laboratory.

1. Each independently meaningful agent architecture/pattern receives its own module/package.
2. Each module follows Clean Architecture where the use case justifies the layers:
   - Domain.
   - Application.
   - Infrastructure adapters when external I/O is required.
   - Public API/composition root with a module-specific fluent builder.
3. Domain must not depend on Application, Infrastructure, provider SDKs, or other agent modules.
4. Application depends on Domain and shared contracts; external capabilities are expressed through ports.
5. Infrastructure implements ports and depends inward.
6. The public module composes its own layers and exposes a concise `Build()` / `RunAsync()` experience.
7. Module-specific fluent APIs are preferred over a universal builder with unrelated options.
8. All built agents expose a common run handle and normalized execution result.
9. Model, tool, memory, retrieval, evaluation, observability, and protocol capabilities are optional dependencies.
10. Algorithms such as A*, Minimax, and Q-Learning are implemented as reusable algorithm modules and are not mislabeled as standalone agents.
11. Agent modules must not depend on one another's concrete implementation types. Orchestration composes through shared contracts.
12. NuGet publication is a later release decision; the current priority is module correctness, usability, and API coherence.

## Consequences

### Positive

- Applications can install only the agent architecture and capabilities they need.
- Simple deterministic agents remain usable without LLM providers or tool runtimes.
- Agent internals have enforceable dependency direction.
- Fluent builders provide a consistent, approachable construction experience.
- Multi-agent composition does not require concrete cross-module dependencies.
- New agent types can be added without modifying unrelated agents.

### Trade-offs

- More projects and package boundaries are required than in a single-assembly design.
- Shared abstractions must remain small to avoid a new monolith.
- Existing ReAct, Planning, Reflection, and other modules need an incremental Clean Architecture/API migration.
- Not every module needs a non-empty Infrastructure layer; creating empty projects would be harmful.
- A large catalog does not justify generating dozens of empty modules. Each implementation must be delivered with behavior and tests.

## First implementation slice

The combined Classical module is being split into:
- `AgentForge.Agents.SimpleReflex`
- `AgentForge.Agents.ModelBasedReflex`

Each has its own Domain and Application projects plus a public module project exposing a fluent builder. Neither module requires Infrastructure because the host supplies percepts/state and applies proposed actions.
