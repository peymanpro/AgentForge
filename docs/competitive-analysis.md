# Competitive Landscape and Product Positioning

**Research snapshot:** 2026-11-02  
**Purpose:** use current public documentation to inform AgentForge's architecture. This is not a feature leaderboard or a claim that AgentForge is superior.

## 1. Microsoft Agent Framework

Microsoft Agent Framework (MAF) is now the most relevant .NET reference implementation to study. Microsoft's April 2026 announcement states that MAF reached 1.0 GA on April 2, 2026 and positions it as the successor that brings together Semantic Kernel and AutoGen. Its public surface spans agents, workflows, agent harnesses, integrations, middleware, context providers, tool/MCP integrations, and hosting. <https://devblogs.microsoft.com/agent-framework/microsoft-agent-framework-at-build-2026-announce/> <https://learn.microsoft.com/agent-framework/>

The current documentation describes several useful patterns:

- Agent loops that combine model interactions, conversations, and tools.
- Middleware around agent behavior, context management, and model clients.
- Workflow composition and agent participation in workflows.
- Sequential, concurrent, handoff, group-chat, and Magentic multi-agent orchestrations.
- Human-in-the-loop, checkpoints/resumption, observability, and hosting capabilities.

Official references:
- [Agent Framework overview](https://learn.microsoft.com/en-us/agent-framework/overview/)
- [Agent pipeline architecture](https://learn.microsoft.com/en-us/agent-framework/agents/agent-pipeline)
- [Workflow orchestrations](https://learn.microsoft.com/en-us/agent-framework/workflows/orchestrations/)
- [Workflow capabilities](https://learn.microsoft.com/en-us/agent-framework/workflows/)

## 2. What AgentForge should learn from it

MAF demonstrates that production agents need more than a prompt and tool list. Its pipeline separation, optional context providers, middleware, sessions, explicit workflows, and observability are useful design references.

AgentForge should adopt the underlying engineering ideas where appropriate:
- A clear agent execution pipeline.
- Explicit context/session ownership.
- Interception points for middleware and policy.
- Workflows as a distinct abstraction from autonomous agents.
- Streaming and checkpointing as optional capabilities rather than hidden behavior.
- Provider integrations behind contracts.

AgentForge should not duplicate the whole MAF API or make MAF a mandatory dependency.

## 3. AgentForge's intended emphasis

AgentForge's product focus is different in three ways:

1. **Agent-architecture modules as the primary unit.** Simple Reflex, Model-Based Reflex, Goal-Based, Utility-Based, BDI, ReAct, Plan-and-Execute, Router, and multi-agent topologies are independently selectable modules, not a single general-purpose LLM agent class.
2. **Clean Architecture inside every module.** Domain rules, application control loops, external adapters, and public composition APIs have clear dependency direction.
3. **A consistent fluent construction experience.** Each module exposes only the configuration relevant to its architecture and returns a common execution handle.

The catalog deliberately also includes classical search, planning, optimization, learning agents, and specialist agents. Search algorithms such as A* and MCTS remain reusable algorithm packages rather than being mislabeled as agents.

## 4. Positioning comparison

| Dimension | Microsoft Agent Framework (documented scope) | AgentForge target |
|---|---|---|
| General LLM agent loop | Strong built-in focus | Separate ReAct, tool-use, planning, and other agent modules |
| Multi-agent orchestration | Sequential, concurrent, handoff, group chat, Magentic | Dedicated modules for each coordination topology |
| Middleware/context/session | First-class documented pipeline concepts | Shared optional runtime capabilities |
| Classical symbolic architectures | Not the central product focus in its main agent API | Core catalog and independently selectable agent modules |
| Search/optimization/RL | Integrations and specialized labs may exist | Explicit algorithm and agent module families |
| Clean Architecture per agent type | Not the central packaging unit in public docs | Required internal design convention |
| Fluent API | Builder/pipeline APIs are present in selected areas | A consistent module-specific fluent builder for every agent type |
| Provider integration | Broad provider and hosting ecosystem | Provider-neutral core with optional provider adapters |
| Mandatory dependency | Framework's own abstraction/runtime | Only the module and capabilities selected by the application |

This is a comparison of stated product emphasis, not a measured quality ranking. Actual API coverage should be verified against the current SDK version before implementation decisions.

## 5. Other references

Semantic Kernel remains relevant for concepts such as plugins, memory, filters, and prior orchestration patterns, but Microsoft positions Agent Framework as the convergence path for agent development. The Semantic Kernel documentation is useful for migration and historical design context: <https://learn.microsoft.com/en-us/semantic-kernel/>

Other frameworks worth studying for specific ideas include LangGraph for stateful graph execution, AutoGen for multi-agent interaction history, and established search/RL libraries for deterministic algorithm implementations. Any direct feature comparison must be dated and grounded in the upstream docs.

## 6. Design conclusion

AgentForge should not compete by having a larger list of methods in a generic agent builder. Its advantage should be the quality of its module boundaries and the developer experience of choosing exactly the agent architecture needed.

The product will be successful when a developer can:
- Choose an agent module by architecture.
- Add only the necessary provider/capability packages.
- Configure it with a short fluent chain.
- Run it through a consistent API.
- Compose it with other modules without tight coupling.
- Inspect clear domain, application, and infrastructure boundaries when customization is needed.
