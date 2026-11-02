# AgentForge

**A modular .NET agent-engineering library and learning laboratory.**

AgentForge is a .NET 10 / C# codebase for building, inspecting, composing, testing, and operating intelligent agents. It brings classical AI agents and algorithms together with modern LLM-based agents, reusable agent capabilities, orchestration patterns, and production engineering practices.

This repository is intentionally **.NET-only**. It is not a multi-language monorepo. The solution, source projects, tests, samples, documentation, and CI all belong to one coherent C# codebase.

AgentForge has two connected goals:

1. **Build a useful engineering toolkit:** real, modular, provider-independent building blocks that application developers can compose.
2. **Teach agent engineering through implementation:** every important concept should be explained through its problem, algorithm, execution lifecycle, trade-offs, tests, and runnable examples.

The ambition is broad, but implementation claims remain evidence-based. A catalog entry is not the same as an implemented feature.

> **Project status:** Active engineering foundation. Ten .NET source modules currently cover abstractions, model contracts, tools, ReAct, Planning, Reflection, retrieval, RAG, and bounded workflow orchestration. Their APIs are experimental. No NuGet package has been published, and NuGet publication is not a current objective.

## Why AgentForge?

An agent system is much more than a prompt connected to an LLM. Real agent engineering involves decision-making, control loops, state, memory, planning, search, tools, permissions, retrieval, coordination, evaluation, observability, reliability, and deployment.

AgentForge makes those mechanics explicit:

- **Inspectable algorithms:** the decision loop belongs in understandable source code, not hidden behind a label.
- **Modular boundaries:** agents, capabilities, protocols, orchestration, and infrastructure are distinct.
- **Bring your own model:** agent logic does not depend on a particular model vendor.
- **Classical and modern AI:** symbolic, search-based, goal-driven, utility-based, BDI, learning, reinforcement-learning, LLM, and multi-agent approaches have a place in one coherent architecture.
- **Safe execution:** model output is untrusted; tool requests are validated and authorized before execution.
- **Learning by engineering:** each implemented concept is accompanied by tests, explanations, examples, and trade-off notes.
- **Extensible by design:** new agent families and capabilities can be added without turning the core into a collection of special cases.

## Repository layout

The .NET solution lives at the repository root:

```text
AgentForge/
├── AgentForge.sln
├── Directory.Build.props
├── global.json
├── src/
│   ├── AgentForge.Abstractions/
│   ├── AgentForge.Models/
│   ├── AgentForge.Tools/
│   ├── AgentForge.Agents.Classical/
│   ├── AgentForge.Agents.ReAct/
│   ├── AgentForge.Agents.Planning/
│   ├── AgentForge.Agents.Reflection/
│   ├── AgentForge.Retrieval/
│   ├── AgentForge.RAG/
│   └── AgentForge.Orchestration/
├── tests/
│   └── one focused test project per implemented module
├── samples/
│   └── runnable examples
├── docs/
│   ├── agent-catalog.md
│   ├── capability-map.md
│   ├── learning-path.md
│   ├── architecture.md
│   ├── concepts.md
│   ├── design/
│   └── decisions/
└── .github/workflows/
```

Only implemented modules are created as source projects. Planned modules are tracked in the catalog and roadmap until their contracts and behavior are ready.

## Current implementation

| Module | Responsibility | Status |
|---|---|---|
| `AgentForge.Abstractions` | Agent execution contracts, context, results, failures, events | Experimental implementation |
| `AgentForge.Models` | Provider-neutral chat model contracts and optional tool-calling capability | Experimental implementation |
| `AgentForge.Tools` | Tool definitions, registry, validation, authorization, execution | Experimental implementation |
| `AgentForge.Agents.Classical` | Deterministic simple-reflex and model-based reflex architectures | Experimental implementation |
| `AgentForge.Agents.ReAct` | Bounded reasoning/action/observation loop | Experimental implementation |
| `AgentForge.Agents.Planning` | Structured plan validation and bounded plan execution | Experimental implementation |
| `AgentForge.Agents.Reflection` | Evaluation-guided iterative refinement | Experimental implementation |
| `AgentForge.Retrieval` | Scoped retrieval and authorization-aware access | Experimental implementation; lexical baseline |
| `AgentForge.RAG` | Bounded context assembly with source provenance | Experimental implementation |
| `AgentForge.Orchestration` | Sequential and conditional workflow composition | Experimental implementation |

See [the agent catalog](docs/agent-catalog.md) for implemented, planned, and research-level agent families. See [the capability map](docs/capability-map.md) for the wider engineering surface.

## Agent families in scope

AgentForge is intended to cover more than LLM tool-calling agents.

- **Classical agent models:** simple reflex, model-based reflex, goal-based, utility-based, learning agents, rule-based systems, reactive/subsumption, deliberative, hybrid, and BDI.
- **Classical decision algorithms:** state-space search, heuristic search, adversarial search, constraint solving, planning, optimization, game-theoretic strategies, bandits, and reinforcement learning.
- **LLM agent patterns:** ReAct, plan-and-execute, replanning, reflection, Reflexion, self-refine, tree/graph search reasoning, tool-use, and code-oriented agents.
- **Multi-agent architectures:** supervisor, hierarchical delegation, planner-worker, peer collaboration, debate, critic-generator, blackboard, contract-net, auction/market, consensus, and swarm-style systems.
- **Specialized agents:** research, coding, browser/computer use, data analysis, SQL/database, document processing, conversational, voice, and multimodal agents.

These names do not all represent the same kind of component. The catalog explicitly classifies agents, algorithms, patterns, capabilities, protocols, and architectures instead of calling everything an agent.

## Learn through the codebase

The [learning path](docs/learning-path.md) is designed to take a developer from agent fundamentals toward advanced agent engineering. It links concepts to the relevant modules, implementation stages, exercises, and evidence of understanding.

A module is considered complete only when it has:
- A precise problem statement and correct classification.
- A documented algorithm and execution lifecycle.
- Explicit public contracts and dependencies.
- Bounded execution, cancellation, and failure semantics where relevant.
- Deterministic tests for success, edge cases, failure, and limits.
- A runnable sample and design/trade-off documentation.

AgentForge can provide a substantial practical learning path, but mastery also requires independent study of mathematics, machine learning, model behavior, distributed systems, security, and real deployments.

## Build and test

Requirements: .NET SDK 10.0.403 (see `global.json`).

From the repository root:

```bash
dotnet restore AgentForge.sln
dotnet build AgentForge.sln --configuration Release --no-restore
dotnet test AgentForge.sln --configuration Release --no-build --no-restore
dotnet run --project samples/AgentForge.ClassicalAgents.Sample
dotnet run --project samples/AgentForge.Orchestration.Sample
```

The sample is deterministic and does not require a model API key.

## Design principles

1. A component must solve a documented problem; names alone do not count as implementations.
2. Agents, algorithms, patterns, capabilities, protocols, architectures, and workflows must be classified correctly.
3. Agent implementations depend on explicit contracts, not on unrelated implementation details.
4. Model-generated actions are proposals, never authorization decisions.
5. Limits, cancellation, side effects, and failure behavior must be visible.
6. Tests use deterministic fakes wherever practical.
7. Modules are split when the boundary provides real reuse, independent tests, dependency isolation, or conceptual clarity—not simply to increase project count.
8. Planned features remain planned until working behavior and evidence exist.
9. No NuGet package is published until the project is intentionally ready for a reviewed release. Publication is outside the current phase.

## Documentation

- [Vision](docs/vision.md)
- [Concepts and terminology](docs/concepts.md)
- [Agent catalog](docs/agent-catalog.md)
- [Capability map](docs/capability-map.md)
- [Learning path](docs/learning-path.md)
- [Architecture](docs/architecture.md)
- [Implementation scope and status](docs/mvp-scope.md)
- [Design notes](docs/design/README.md)
- [Classical agent architecture design](docs/design/classical-agents.md)
- [AgentForge specification](AGENTFORGE_SPEC.md)
- [Roadmap](ROADMAP.md)
- [Contributing](CONTRIBUTING.md)
- [Security policy](SECURITY.md)

## License

MIT. See [LICENSE](LICENSE).
