# AgentForge Product Vision

AgentForge is a **modular .NET agent framework** that lets developers create, configure, and run the agent architecture they need through concise, strongly typed Fluent APIs.

The primary user experience is intentionally simple:

1. Choose the agent architecture or pattern that fits the problem.
2. Install only that agent's NuGet package.
3. Configure it with a discoverable Fluent API.
4. Call `Build()` to validate and create the agent.
5. Execute it through a consistent `RunAsync()` contract.
6. Add other agent modules or optional capabilities only when the application needs them.

A developer should not need to understand the internal Domain/Application wiring to use an agent. At the same time, the public API must not hide important decisions, resource limits, state ownership, or security boundaries.

## Product promise

- **Low-code construction:** a short, readable happy path for every implemented agent.
- **Choice:** many agent architectures, each selectable independently.
- **Modularity:** installing one agent must not pull in unrelated agents.
- **Consistency:** common concepts use familiar names and a shared execution contract.
- **Specificity:** each builder exposes only the configuration meaningful to its architecture.
- **Extensibility:** new agents and capabilities can be added without changing unrelated modules.
- **Transparency:** Build-time validation, explicit dependencies, bounded execution, and documented failure behavior.

## Product model

AgentForge is a framework delivered as composable packages. The product has four related but distinct package families:

- **Agent modules:** independently installable architectures and patterns such as Simple Reflex, Goal-Based, Utility-Based, Learning, ReAct, and Reflection.
- **Algorithms:** reusable search, planning, game-playing, optimization, and learning procedures that can power multiple agents.
- **Capabilities:** optional model, tool, memory, retrieval, RAG, evaluation, security, and observability integrations.
- **Composition:** workflow and multi-agent modules that combine agents through shared contracts.

An algorithm or capability is not automatically a standalone agent. Package boundaries must represent useful, independently consumable contracts.

## Package and dependency principles

- Each independently meaningful agent architecture/pattern receives its own NuGet package.
- A module depends on shared abstractions, not on unrelated concrete agent modules.
- Shared abstractions are restored transitively; consumers should not need to manually install internal implementation packages.
- Optional capabilities are separate packages and must not become dependencies of agents that do not need them.
- No mandatory umbrella package may pull every agent into an application.
- A future optional convenience bundle may exist, but it must remain opt-in and must not replace per-agent installation.

## Fluent API principles

- Each agent has a module-specific, strongly typed builder.
- Shared vocabulary is consistent where concepts genuinely overlap: `Create`, `WithName`, `WithDescription`, `WithTimeout`, `WithMaxSteps`, and `Build`.
- Architecture-specific methods remain specific: for example, `When` for reflex rules, `WithGoalPredicate` for goal search, and `WithEpsilonGreedy` for the classic learning agent.
- Do not create a universal builder filled with irrelevant switches.
- Validate required configuration and incompatible options in `Build()`.
- Building an agent must not silently call remote services or perform external side effects.
- A built agent exposes a consistent `AgentHandle<TInput,TOutput>` and `RunAsync()` API.
- Advanced execution options remain available without burdening the ordinary happy path.

## What AgentForge is not

- Not a single generic LLM wrapper.
- Not a mandatory all-in-one package.
- Not a monolithic agent class with dozens of strategy flags.
- Not a workflow engine disguised as an autonomous agent.
- Not an educational course or a collection of sample-only implementations.
- Not a claim that every architecture in the catalog is already implemented.

## Long-term scope

The catalog tracks classical agent architectures, symbolic and search-based decision systems, LLM patterns, specialist agents, multi-agent topologies, and supporting capabilities. New modules should be added incrementally, each with a usable Fluent API, tests, package documentation, examples, and verified CI.

See the [Agent Catalog](agent-catalog.md), [Fluent API Design](fluent-api.md), and [Roadmap](../ROADMAP.md).
