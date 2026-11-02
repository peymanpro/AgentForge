# Engineering Landscape and Positioning

**Purpose:** A decision framework for studying existing agent frameworks and avoiding vague or unsupported positioning claims.

This document is not a leaderboard and does not claim that AgentForge is superior to another project. Framework capabilities change quickly; any future factual comparison must be based on dated official documentation and reproducible experiments.

## 1. Relevant categories

AgentForge should study several adjacent categories rather than treating every project as the same kind of framework:

- **Agent application frameworks:** provide model interactions, tools, agent abstractions, and common application patterns.
- **Graph/workflow runtimes:** provide explicit state, nodes, edges, branching, persistence, checkpointing, and human interruption.
- **Model orchestration SDKs:** provide provider connectors, prompts, memory, retrieval, tools, and composition.
- **Classical AI and planning libraries:** provide symbolic reasoning, search, constraint solving, optimization, game playing, and planning.
- **Reinforcement-learning frameworks:** provide environments, policies, training loops, replay, evaluation, and simulation.
- **Multi-agent simulation platforms:** provide agent populations, message passing, scheduling, and emergent coordination.

Examples worth examining over time include Microsoft Agent Framework, Semantic Kernel, LangGraph, AutoGen, and established AI/search/RL libraries. Their scope and current status must be checked against their official sources before publishing a dated comparison.

## 2. Comparison dimensions

When studying another framework, record evidence for each dimension:

| Dimension | Questions |
|---|---|
| Agent model | Which agent architectures and control loops are implemented? |
| Classical AI | Are search, planning, symbolic reasoning, game strategies, or learning agents supported? |
| Model independence | Can applications replace the model provider without rewriting agent logic? |
| Tools | Are schemas, validation, permissions, side effects, and sandboxing explicit? |
| State and memory | What state is transient or persistent? How are retention and deletion handled? |
| Retrieval and RAG | Which lexical, vector, hybrid, graph, and reranking strategies exist? |
| Orchestration | Are workflows sequential, graph-based, parallel, durable, and resumable? |
| Multi-agent | Which delegation, messaging, negotiation, and coordination topologies exist? |
| Evaluation | Are datasets, regression tests, judges, and quality metrics available? |
| Observability | Can developers inspect correlated model, tool, retrieval, and workflow events? |
| Security | How are untrusted content, authorization, secrets, and human approval addressed? |
| Developer experience | Are APIs documented, examples runnable, and failure semantics understandable? |
| Extensibility | Can new agent algorithms and capabilities be added without modifying unrelated modules? |

A comparison should distinguish built-in behavior from integrations, examples, community packages, and features merely described on a roadmap.

## 3. AgentForge's intended focus

AgentForge is a **single .NET / C# repository** with two connected goals:

1. A modular toolkit for building and composing agent systems.
2. A source-level learning environment for understanding how the systems work.

Its intended breadth is classical and modern:
- Classical reflex, goal/utility, rule-based, reactive, deliberative, hybrid, and BDI agent architectures.
- Search, planning, game-playing, optimization, bandits, and reinforcement learning.
- LLM agent patterns such as ReAct, plan-and-execute, reflection, routing, and search-based reasoning.
- Multi-agent architectures such as supervisor, hierarchy, peer collaboration, debate, contract-net, auctions, consensus, and swarms.
- Capabilities and production engineering: models, tools, memory, retrieval, workflows, evaluation, observability, security, protocols, and deployment.

The [Agent Catalog](agent-catalog.md) distinguishes the planned landscape from implemented modules.

## 4. Current evidence-based position

At the current foundation stage:
- The root repository is .NET-only.
- Nine experimental source modules implement abstractions, model contracts, tools, ReAct, Planning, Reflection, retrieval, RAG, and bounded sequential/conditional orchestration.
- APIs are not stable.
- No NuGet package has been published, and publication is not a current objective.
- Classical-agent, reinforcement-learning, advanced multi-agent, durable workflow, and production evaluation areas are planned rather than implemented.

These limitations should remain visible in project messaging.

## 5. Positioning claims to avoid

Do not claim:
- To be the first, best, fastest, or most complete agent framework.
- To implement every agent architecture because it appears in the catalog.
- To be production-ready without operational evidence.
- To support durable or distributed execution before it is tested.
- To provide automatic factual verification merely because RAG or citations are present.
- To provide secure execution merely because a tool policy interface exists.

Prefer concrete statements about implemented algorithms, test coverage, supported capabilities, and known constraints.

## 6. Ongoing research method

For each major framework or paper:
1. Read the original design or official documentation.
2. Classify the component as an agent, algorithm, capability, protocol, workflow, or architecture.
3. Reproduce a small deterministic scenario.
4. Compare control flow, state, dependencies, limits, and failure behavior.
5. Identify what can be learned or reused without copying unnecessary abstractions.
6. Update AgentForge's catalog/design notes only when there is a clear implementation or learning benefit.

The goal is informed engineering, not feature-count competition.
