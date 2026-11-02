# ADR 0003: Position AgentForge as a Modular Fluent Agent Framework

- **Status:** Accepted
- **Date:** 2026-11-02
- **Supersedes:** the product label "modular agent library" in ADR 0002; the modular package and dependency decisions remain in force.
- **Decision owners:** AgentForge maintainers

## Context

The product goal is not merely to publish a collection of reusable agent libraries. Developers should be able to choose the agent architecture they need, install only its NuGet package, configure it with minimal code through a Fluent API, and execute it through a consistent contract.

Calling AgentForge a framework better communicates this guided construction experience. However, a framework label must not lead to a monolithic package, a universal builder with irrelevant options, or mandatory infrastructure that every consumer must adopt.

## Decision

AgentForge is a **modular .NET agent framework delivered as independently installable NuGet packages**.

1. The primary developer journey is: choose an agent -> install its package -> configure its module-specific Fluent API -> call Build() -> call RunAsync().
2. Each independently meaningful agent architecture or pattern has its own NuGet package.
3. A consumer can install one agent without installing unrelated agent modules.
4. Required shared abstractions and internal projects are resolved transitively by NuGet.
5. Optional capabilities such as model providers, tools, memory, retrieval, RAG, evaluation, security, and observability are separate packages unless genuinely required by an agent's core behavior.
6. Every implemented agent exposes a concise, strongly typed, module-specific builder. Shared method vocabulary is consistent where concepts overlap; architecture-specific methods remain local to the agent.
7. Build() validates configuration and creates an immutable agent. It performs no hidden remote calls or external side effects.
8. Built agents expose the common AgentHandle<TInput,TOutput> execution contract.
9. There is no mandatory AgentForge meta-package that depends on every agent. An optional convenience bundle may be considered later.
10. New agent types are added incrementally and must pass the module completion gate: behavior, builder validation, tests, sample, package README, dependency checks, and CI.

## Consequences

### Positive

- Developers can build different agent architectures with little setup code.
- The API is approachable without hiding the agent's actual decision model.
- NuGet installation remains minimal and explicit.
- New agent modules can be added without modifying unrelated modules.
- The framework can grow its catalog while preserving modularity.

### Trade-offs

- Public API consistency requires conformance tests and careful review.
- Each agent package must maintain its own documentation and examples.
- Some existing modules need builder/API migration before the developer experience is uniform.
- The framework must resist adding generic options that do not apply to every architecture.

## Implementation sequence

1. Establish this product direction and package policy.
2. Audit all implemented modules against the Fluent API contract.
3. Migrate ReAct, Plan-and-Execute, and Reflection to module-specific builders.
4. Add automated package dependency checks.
5. Continue adding agent architectures incrementally.
