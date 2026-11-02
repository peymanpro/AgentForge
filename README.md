# AgentForge

**A modular .NET library for building, composing, and operating different kinds of intelligent agents.**

AgentForge is a .NET 10 / C# library. Its purpose is to make agent construction easier without hiding how an agent works or forcing every problem into one generic LLM loop.

Every agent architecture is provided as an independently usable module. Each module owns its domain rules and application use cases, exposes a small fluent API, and depends only on the capabilities it needs. Applications can use one agent directly or compose different agents into larger workflows and multi-agent systems.

AgentForge is **not tied to a model provider**. Classical and deterministic agents do not require an LLM. LLM-based agents accept provider-neutral model contracts and optional capabilities such as tools, memory, retrieval, structured output, and streaming.

## Product principles

- **One agent type, one module.** Simple Reflex, Model-Based Reflex, ReAct, BDI, Goal-Based, Utility-Based, Supervisor, Debate, and other architectures are independently selectable.
- **Clean Architecture inside each module.** Domain and Application are separated; Infrastructure contains adapters only when the module needs them; the public module is the composition root.
- **Fluent by default.** Common configuration reads like a declaration. The built agent has a concise `RunAsync` entry point.
- **Small dependency surface.** A rule-based agent should not pull in model SDKs, tool execution, vector databases, or orchestration.
- **Composition over inheritance.** Agents implement the shared execution contract and can be composed without depending on one another's concrete types.
- **Explicit behavior.** State ownership, budgets, cancellation, tool permissions, failure outcomes, and side effects are visible.
- **No false completeness.** The catalog distinguishes working implementations from planned and research modules.

## Example: Simple Reflex agent

```csharp
var agent = SimpleReflexAgentBuilder
    .Create<ThermostatPercept, ThermostatAction>()
    .When(
        "temperature-too-high",
        percept => percept.TemperatureCelsius > percept.TargetCelsius + 1,
        ThermostatAction.Cool)
    .When(
        "temperature-too-low",
        percept => percept.TemperatureCelsius < percept.TargetCelsius - 1,
        ThermostatAction.Heat)
    .Build();

var result = await agent.RunAsync(percept, cancellationToken);
```

No model, provider, tool registry, database, or DI container is required.

## Example: Model-Based Reflex agent

```csharp
var agent = ModelBasedReflexAgentBuilder
    .Create<Percept, WorldState, Action>()
    .WithStateUpdater((previous, percept) => UpdateWorldState(previous, percept))
    .When("rule-one", state => MatchesRuleOne(state), Action.One)
    .When("rule-two", state => MatchesRuleTwo(state), Action.Two)
    .Build();

var result = await agent.RunAsync(
    new ModelBasedReflexInput<Percept, WorldState>(percept, previousState),
    cancellationToken);
```

The host owns the state lifecycle. The agent returns the updated state rather than keeping mutable state hidden inside a shared instance.

## Agent modules

The long-term catalog covers several distinct families:

| Family | Examples |
|---|---|
| Classical architectures | Simple Reflex, Model-Based Reflex, Goal-Based, Utility-Based, Learning, Rule-Based, BDI, Hybrid |
| Control architectures | Finite-State, Behavior Tree, Subsumption, Deliberative, Reactive |
| LLM agent patterns | ReAct, Plan-and-Execute, Replanner, Router, Reflection, Reflexion, Self-Refine, CodeAct |
| Search and reasoning | BFS, DFS, A*, Minimax, Alpha-Beta, MCTS, Tree of Thoughts, Graph of Thoughts |
| Learning and optimization | Bandits, Q-Learning, SARSA, PPO, Genetic Algorithms, PSO, ACO |
| Multi-agent systems | Supervisor, Planner-Worker, Hierarchical, Peer Collaboration, Group Chat, Debate, Consensus, Contract-Net, Swarm |
| Specialized agents | Research, Coding, Browser, Computer Use, Data Analysis, SQL, Document, Customer Support, Voice, Multimodal |

Algorithms, capabilities, and protocols are not automatically agents. They are separate modules that agent modules can compose.

See [Agent Catalog](docs/agent-catalog.md) for classification and implementation status, [Module Architecture](docs/agent-modules.md) for module boundaries, and [Fluent API](docs/fluent-api.md) for API conventions.

## Shared capabilities

Agents can opt into shared modules when required:

- Model contracts and provider adapters.
- Tools, validation, authorization, and controlled execution.
- Conversation state and memory.
- Retrieval, reranking, and RAG.
- Planning, search, optimization, and reinforcement learning.
- Workflow and multi-agent orchestration.
- Evaluation, safety, observability, and cost/resource accounting.
- MCP, agent-to-agent protocols, browser/computer-use, and multimodal integration.

These capabilities are optional dependencies. An agent module should not require capabilities it does not use.

## Current implementation status

The first two classical modules have been split into independent Clean Architecture modules with fluent builders:

- `AgentForge.Agents.SimpleReflex` — experimental implementation.
- `AgentForge.Agents.ModelBasedReflex` — experimental implementation.

ReAct, Planning, Reflection, Tools, Models, Retrieval, RAG, and Orchestration exist as experimental foundations. Their internal layer boundaries and public fluent APIs are part of the ongoing module migration. Other architectures remain in the catalog as planned or research candidates.

No NuGet package has been published. Public APIs remain experimental while the module boundaries and common execution contract are being established.

## Build and test

Requirements: .NET SDK 10.0.403 (see `global.json`).

```bash
dotnet restore AgentForge.sln
dotnet build AgentForge.sln --configuration Release --no-restore
dotnet test AgentForge.sln --configuration Release --no-build --no-restore
dotnet run --project samples/AgentForge.ClassicalAgents.Sample
dotnet run --project samples/AgentForge.Orchestration.Sample
```

## Architecture and engineering contracts

- [Product vision](docs/vision.md)
- [Module architecture](docs/agent-modules.md)
- [Fluent API conventions](docs/fluent-api.md)
- [System architecture](docs/architecture.md)
- [Agent and algorithm catalog](docs/agent-catalog.md)
- [Capability map](docs/capability-map.md)
- [Competitive landscape](docs/competitive-analysis.md)
- [AgentForge specification](AGENTFORGE_SPEC.md)
- [Roadmap](ROADMAP.md)
- [Design notes](docs/design/README.md)
- [Contributing](CONTRIBUTING.md)
- [Security policy](SECURITY.md)

## License

MIT. See [LICENSE](LICENSE).
