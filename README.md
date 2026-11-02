# AgentForge

**This repository contains AgentForge, an open-source modular .NET library for building, running, and composing different kinds of intelligent agents.**

AgentForge treats the agent architecture as a first-class design choice. A Simple Reflex agent, a BDI agent, a ReAct agent, a planner, and a multi-agent supervisor use different decision processes. They should not be forced into one large class or one universal configuration API.

Each independently meaningful agent architecture is intended to be a separately usable module. A module owns its decision process, exposes an architecture-specific fluent API, and depends only on the capabilities it needs. Applications can use one agent directly or compose several agents through shared execution and orchestration contracts.

AgentForge targets **.NET 10 / C#** and is provider-neutral. Deterministic agents do not need an LLM. LLM-based agents can use model, tool, memory, retrieval, and other capabilities through explicit contracts and optional integrations.

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/)
[![CI](https://github.com/peymanpro/AgentForge/actions/workflows/agentforge-ci.yml/badge.svg?branch=main)](https://github.com/peymanpro/AgentForge/actions/workflows/agentforge-ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

<sub>Development status: experimental. The catalog is broader than the current implementation; each module's status is stated explicitly below.</sub>

## Table of contents

- [Why this library exists](#why-this-library-exists)
- [How to choose an agent architecture](#how-to-choose-an-agent-architecture)
- [Current agent modules](#current-agent-modules)
- [Agent architectures to build](#agent-architectures-to-build)
- [Algorithms are not agents](#algorithms-are-not-agents)
- [Shared capabilities](#shared-capabilities)
- [Installation](#installation)
- [Quick start](#quick-start)
- [How AgentForge works](#how-agentforge-works)
- [Module architecture](#module-architecture)
- [Documentation structure](#documentation-structure)
- [Limitations and non-goals](#limitations-and-non-goals)
- [Development](#development)
- [Roadmap](#roadmap)
- [License](#license)

## Why this library exists

Agent engineering includes several distinct problems that are often grouped under the same word: *agent*. Some systems are deterministic decision-makers. Some maintain an internal world model. Others plan, call tools, evaluate their own outputs, or coordinate multiple specialized agents.

A single generic agent abstraction can hide these differences. It can also make a small deterministic agent depend on model clients, tool registries, memory stores, or orchestration infrastructure that it does not need.

AgentForge exists to make the architecture itself a first-class choice:

1. **Different problems need different architectures.** A thermostat with condition-action rules does not need the same control loop as a research agent or a multi-agent supervisor.
2. **An agent should depend only on what it uses.** A deterministic agent should not pull in an LLM SDK, vector database, or tool runtime.
3. **The decision process should remain understandable.** State updates, planning, tool authorization, stopping conditions, budgets, and failures should be explicit.
4. **Agents should be independently usable and composable.** A developer can select one module, configure it through its fluent API, and combine it with other modules through shared contracts.
5. **Provider and infrastructure choices should remain replaceable.** Model access, tools, memory, retrieval, and external services are optional capabilities, not mandatory dependencies of every agent.

The goal is not to implement every named pattern as a separate package regardless of value. It is to provide a coherent set of independently useful architectures, algorithms, and capabilities, with clear boundaries between them.

## How to choose an agent architecture

Start with the decision problem—not with the model provider.

| Problem or requirement | Suitable architecture | Current status |
|---|---|---|
| Choose an action from the current input using fixed rules | Simple Reflex | Implemented, experimental |
| Use the current input and an explicit previous world state | Model-Based Reflex | Implemented, experimental |
| Let a model alternate between decisions, tool actions, and observations | ReAct | Experimental foundation |
| Generate a bounded plan and execute validated steps | Plan-and-Execute | Experimental foundation |
| Generate a candidate, evaluate it, and refine it within limits | Reflection | Experimental foundation |
| Choose actions by considering desired outcomes | Goal-Based | Planned |
| Compare possible outcomes using preferences or utility | Utility-Based | Planned |
| Maintain beliefs, desires, intentions, and commitments | BDI | Planned |
| Choose a model, tool, agent, or execution path | Router / Dispatcher | Planned |
| Delegate work to a team and integrate their results | Supervisor / Multi-Agent | Planned |

A model is a capability an agent may use; it is not, by itself, an agent architecture. Retrieval, planning algorithms, memory, and tool execution should be composed where they add value rather than bundled into every agent.

## Current agent modules

The following modules have source code in this repository. Their module READMEs describe their behavior, public surface, boundaries, and limitations.

| Module | What it does | Status and documentation |
|---|---|---|
| **Simple Reflex** | Evaluates ordered condition-action rules against the current percept; the first matching rule wins. | **Modular implementation (experimental).** [Module README](src/AgentForge.Agents.SimpleReflex/README.md) |
| **Model-Based Reflex** | Updates an explicit state from the previous state and current percept, then evaluates rules against that state. | **Modular implementation (experimental).** [Module README](src/AgentForge.Agents.ModelBasedReflex/README.md) |
| **ReAct** | Alternates model decisions, authorized tool execution, and observations until a final response or configured limit. | **Experimental foundation; module migration pending.** [Module README](src/AgentForge.Agents.ReAct/README.md) |
| **Plan-and-Execute** | Validates a generated execution plan and runs registered step handlers with bounded execution. | **Experimental foundation; module migration pending.** [Module README](src/AgentForge.Agents.Planning/README.md) |
| **Reflection** | Generates, evaluates, and iteratively refines a candidate under attempt and time limits. | **Experimental foundation; module migration pending.** [Module README](src/AgentForge.Agents.Reflection/README.md) |

“Experimental foundation” means code and tests exist, but the module has not yet completed the target Clean Architecture and fluent-API migration. It does not mean the API is stable or production-certified.

## Agent architectures to build

The broader product catalog records which architectures are accepted roadmap items and which still need research. The names below are targets or candidates, not claims of existing implementations.

### Classical decision architectures

- **Goal-Based** — choose actions according to whether they move the system toward an explicit goal.
- **Utility-Based** — compare possible outcomes using a utility or preference function.
- **Learning Agent** — improve behavior using feedback, evaluation, and exploration.
- **Rule-Based / Production-System** — apply facts, rules, conflict resolution, and inference.
- **BDI** — manage beliefs, desires, intentions, and commitment/reconsideration.
- **Reactive / Subsumption** — coordinate layered behaviors with priority and inhibition.
- **Deliberative** — build or update a world model and reason before acting.
- **Hybrid / Three-Layer** — combine reactive behavior with deliberation and planning.
- **Logic-Based** — derive conclusions and actions through explicit logical inference.

### Control architectures

- **Finite-State / Hierarchical-State** — select behavior through explicit states, transitions, and guards.
- **Behavior Tree** — compose actions and conditions through sequence, selector, and related nodes.

### LLM agent patterns

- **Planner-Replanner** — revise the remaining plan in response to observations or failed steps.
- **Reflexion** — carry verbal feedback or experience into later attempts.
- **Self-Refine** — generate, critique, and revise an output through a bounded loop.
- **Router / Dispatcher** — select an agent, tool, model, or execution route.
- **CodeAct** — express actions as code and run them only inside a constrained execution boundary.
- **Tree of Thoughts / Graph of Thoughts** — explore and evaluate multiple reasoning states.
- **LATS / RAP / ReWOO-style patterns** — investigate search-guided reasoning and separation of planning from execution.

### Specialized agents

- **Research Agent** — discover sources, retrieve evidence, track provenance, and synthesize findings.
- **Coding Agent** — inspect repositories, plan changes, edit files, run tests, and report evidence.
- **Browser / Computer-Use Agent** — observe and interact with interfaces under explicit approval boundaries.
- **Data-Analysis Agent** — inspect data, run controlled analysis, and validate conclusions.
- **SQL / Database Agent** — discover schemas, generate and validate queries, and enforce read/write policy.
- **Document-Processing Agent** — parse, extract, classify, and connect information across documents.
- **Conversational / Personal Assistant** — manage dialogue state, preferences, memory, and tasks.
- **Customer-Support Agent** — answer from approved knowledge and execute permitted support workflows.
- **Voice / Realtime Agent** — coordinate speech input/output, turn-taking, interruption, and latency.
- **Multimodal Agent** — reason over image, audio, video, and other structured inputs.

### Multi-agent architectures

- **Supervisor** and **Hierarchical Teams** — delegate tasks to workers and combine outcomes.
- **Planner-Worker** — decompose a task and assign bounded subtasks.
- **Peer Collaboration** and **Group Chat** — coordinate through message exchange and explicit turn policies.
- **Debate** and **Generator-Critic** — compare proposals through challenge and verification.
- **Blackboard** — coordinate through a shared workspace and control mechanism.
- **Contract-Net** and **Auction** — allocate work through bids and award rules.
- **Consensus** — aggregate decisions under a defined voting or agreement policy.
- **Swarm / Decentralized** — coordinate multiple limited agents through local rules or shared signals.

See the [Agent and Algorithm Catalog](docs/agent-catalog.md) for classification, status, and the full inventory. A catalog entry is not an implementation promise.

## Algorithms are not agents

AgentForge keeps reusable algorithms separate from complete agent architectures. An algorithm can support many agents and should not be presented as an agent merely because an agent uses it.

| Family | Examples |
|---|---|
| Search | BFS, DFS, Uniform-Cost Search, Greedy Best-First, A*, IDA*, Beam Search |
| Planning | STRIPS-style planning, forward/backward planning, HTN, constraint solving, scheduling, replanning |
| Game and adversarial search | Minimax, Negamax, Alpha-Beta, Expectimax, Monte Carlo Tree Search |
| Local search and optimization | Hill Climbing, Simulated Annealing, Tabu Search, Genetic Algorithms, PSO, ACO |
| Bandits and reinforcement learning | Epsilon-Greedy, UCB, Thompson Sampling, Q-Learning, SARSA, DQN, PPO, SAC |
| Multi-objective decision methods | Pareto optimization, utility aggregation, negotiation, game-theoretic strategies |

These should become reusable packages with their own contracts and tests, rather than being hidden inside one agent module.

## Shared capabilities

Agent modules can opt into capabilities when their architecture requires them.

| Capability | Responsibility |
|---|---|
| **Models and providers** | Provider-neutral model contracts and optional provider adapters |
| **Tools** | Tool definitions, schema validation, authorization, and controlled execution |
| **Memory** | Conversation, working, semantic, episodic, and other memory strategies and stores |
| **Retrieval and RAG** | Lexical/vector/hybrid retrieval, reranking, grounding, provenance, and context assembly |
| **Orchestration** | Sequential, conditional, parallel, looping, and stateful workflow composition |
| **Evaluation** | Deterministic tests, trajectory evaluation, quality/safety regression, and release evidence |
| **Security** | Trust boundaries, least privilege, tenant isolation, untrusted-content handling, and approval policies |
| **Observability** | Run events, traces, metrics, cost/latency records, correlation, and redaction |
| **Protocols and interfaces** | MCP, agent-to-agent communication, browser/computer-use, and multimodal integrations |
| **Production support** | Budgets, rate limits, retries, backpressure, caching, and lifecycle controls |

Some foundations already exist in the repository, including **AgentForge.Abstractions**, **AgentForge.Models**, **AgentForge.Tools**, **AgentForge.Retrieval**, **AgentForge.RAG**, and **AgentForge.Orchestration**. Their APIs and integration coverage remain experimental. Planned package families are tracked in the [Capability Map](docs/capability-map.md).

## Installation

**AgentForge has not been published to NuGet yet.** Package publishing is intentionally deferred while module boundaries and public APIs are being established.

For now, clone the repository and add a project reference to the module you need. For example, from an application directory next to the AgentForge checkout:

~~~bash
dotnet add YourApp.csproj reference ../AgentForge/src/AgentForge.Agents.SimpleReflex/AgentForge.Agents.SimpleReflex.csproj
~~~

A deterministic agent such as Simple Reflex does not require model-provider packages. Add other capability references only when the chosen agent needs them.

## Quick start

The examples below use application-defined input and action types. They demonstrate the current public fluent APIs for the two modular classical agents.

### Simple Reflex

A Simple Reflex agent evaluates rules in declaration order and selects the action associated with the first matching condition.

~~~csharp
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
~~~

No LLM, model provider, tool registry, database, or dependency-injection container is required. The agent proposes an action; the host applies it to the environment.

See [Simple Reflex module documentation](src/AgentForge.Agents.SimpleReflex/README.md).

### Model-Based Reflex

A Model-Based Reflex agent updates an explicit state using the previous state and current percept before evaluating its rules. The host owns the state lifecycle and persistence.

~~~csharp
var agent = ModelBasedReflexAgentBuilder
    .Create<ThermostatPercept, ThermostatState, ThermostatAction>()
    .WithStateUpdater((previous, percept) => new ThermostatState(
        PreviousTemperature: previous.CurrentTemperature,
        CurrentTemperature: percept.TemperatureCelsius,
        TargetTemperature: percept.TargetCelsius))
    .When(
        "temperature-rising-above-target",
        state => state.CurrentTemperature > state.PreviousTemperature
            && state.CurrentTemperature > state.TargetTemperature,
        ThermostatAction.Cool)
    .When(
        "temperature-falling-below-target",
        state => state.CurrentTemperature < state.PreviousTemperature
            && state.CurrentTemperature < state.TargetTemperature,
        ThermostatAction.Heat)
    .Build();

var result = await agent.RunAsync(
    new ModelBasedReflexInput<ThermostatPercept, ThermostatState>(percept, previousState),
    cancellationToken);
~~~

The agent returns the updated state with its decision. It does not retain mutable per-user state inside a shared agent instance.

See [Model-Based Reflex module documentation](src/AgentForge.Agents.ModelBasedReflex/README.md).

## How AgentForge works

A host selects an agent architecture, configures its module-specific builder, and runs the resulting handle. The agent can use optional capability modules, while the host remains responsible for application state, persistence, and side effects.

~~~text
Application / Host
       |
       | selects and configures
       v
Agent module + architecture-specific fluent builder
       |
       | Build()
       v
AgentHandle<TInput, TOutput>
       |
       | RunAsync(...)
       v
Agent use case / control loop
       |
       +------ optional model and provider adapter
       +------ optional tools and authorization
       +------ optional memory and retrieval
       +------ optional planning / search / evaluation
       |
       v
Normalized run result
~~~

The common execution surface is intentionally small. Agent-specific behavior stays in the agent module; reusable capabilities stay in their own packages.

## Module architecture

Each agent module follows Clean Architecture where it adds value:

~~~text
AgentForge.Agents.<Type>/
    Domain/              # Agent concepts, rules, and invariants
    Application/         # Use cases and the decision/control loop
    Infrastructure/      # Optional external adapters
    Public API/Builders   # Fluent API and composition root
    README.md            # Module-specific documentation
~~~

The dependency direction points inward:

- **Domain** owns the concepts and rules of the agent. It has no provider, database, transport, or other-agent dependency.
- **Application** implements the control loop and defines ports for capabilities it needs.
- **Infrastructure** implements external ports when required. It is omitted when the agent has no external I/O.
- **Public API / composition root** validates configuration, composes the module, and returns the common execution handle.

One architecture per module keeps dependency surfaces small. A Simple Reflex module does not need a model SDK; a ReAct module can opt into model and tool contracts; a Supervisor module can compose other agents through common contracts without referencing their concrete implementations.

## Documentation structure

The repository uses two complementary documentation levels:

- **This root README** explains why AgentForge exists, how to choose an architecture, which modules exist, which architectures are planned, and where to find further information.
- **Each agent module has its own README** next to its public project. That document is the source of truth for the agent's algorithm, fluent API, dependencies, state lifecycle, stopping behavior, failure semantics, security boundaries, and limitations.

The root README links every current agent module. The [Agent Catalog](docs/agent-catalog.md) tracks the complete inventory, including planned and research candidates. Cross-cutting design decisions live under [docs](docs/architecture.md).

When a new agent module is implemented, its module README and root README entry must be added or updated in the same change. Planned agents should not receive empty placeholder modules merely to create a documentation link.

## Limitations and non-goals

- AgentForge is under active development. Public APIs and module boundaries may change.
- The catalog is a product inventory, not a statement that every listed agent or algorithm is implemented.
- The existing ReAct, Planning, and Reflection implementations still need migration to the target module layering and fluent builder conventions.
- NuGet publishing and stable-version support are not part of the current phase.
- AgentForge is a library, not a hosted agent platform, managed model service, or guarantee that an application is safe or production-ready.
- Model-generated content and tool proposals are untrusted input. Applications remain responsible for authorization, data protection, provider configuration, and side-effect policies.
- An agent's proposed action is not automatically applied to the external environment; ownership of side effects is explicit.

## Development

Requirements: **.NET SDK 10.0.403** (see [global.json](global.json)).

Restore, build, and test the complete solution:

~~~bash
dotnet restore AgentForge.sln
dotnet build AgentForge.sln --configuration Release --no-restore
dotnet test AgentForge.sln --configuration Release --no-build --no-restore
~~~

Run the examples:

~~~bash
dotnet run --project samples/AgentForge.ClassicalAgents.Sample
dotnet run --project samples/AgentForge.Orchestration.Sample
~~~

The GitHub Actions CI workflow validates the solution. A module should not be marked implemented until its tests, architecture checks, sample, and CI validation pass.

## Roadmap

The roadmap prioritizes a reliable module template before expanding the catalog:

1. Complete the module layering and public fluent API migration for the existing ReAct, Planning, and Reflection foundations.
2. Add further classical architectures as independent modules, beginning with architectures that have clear and testable decision contracts.
3. Implement reusable search, planning, game, optimization, and reinforcement-learning packages separately from agent modules.
4. Add specialist agents by composing established architectures and shared capabilities.
5. Add multi-agent topologies after the shared execution, delegation, messaging, and aggregate-budget contracts are mature.
6. Expand provider, memory, retrieval, evaluation, security, observability, and protocol integrations as optional capabilities.

See the detailed [Roadmap](ROADMAP.md), [Product Vision](docs/vision.md), and [Competitive Landscape](docs/competitive-analysis.md).

## License

AgentForge is licensed under the MIT License. See [LICENSE](LICENSE).
