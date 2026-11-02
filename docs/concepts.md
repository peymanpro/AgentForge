# Concepts and Terminology

Agent engineering has a broad vocabulary. AgentForge separates the concepts below so that code, modules, and documentation communicate what a component actually does.

## Agent

An agent perceives or receives relevant state, selects actions according to a policy or decision process, and acts toward a goal. It has a defined control loop or decision responsibility.

An agent may be:
- Entirely deterministic and rule-based.
- Search- or planning-based.
- Utility- or goal-driven.
- Learning-based, including reinforcement learning.
- LLM-based.
- One participant in a multi-agent system.

An LLM call alone is not necessarily an agent.

## Agent model or architecture

An architecture describes the structure of an agent: how it represents state, chooses actions, plans, reacts, or learns.

Examples:
- Simple-reflex and model-based reflex architectures.
- Goal-based and utility-based agents.
- Learning-agent architecture.
- Reactive, deliberative, and hybrid architectures.
- Subsumption architecture.
- Belief-Desire-Intention (BDI).
- Rule-based and blackboard architectures.

## Algorithm

An algorithm is a concrete procedure. It may be used by an agent but is not necessarily an agent on its own.

Examples:
- BFS, DFS, uniform-cost search, A*, and beam search.
- Minimax, alpha-beta pruning, expectimax, and Monte Carlo Tree Search.
- Hill climbing, simulated annealing, genetic algorithms, particle swarm optimization, and ant colony optimization.
- Q-learning, SARSA, policy gradients, PPO, and actor-critic methods.

AgentForge may implement these as reusable strategies and then compose them into complete agents.

## Agent pattern

A pattern describes a reusable control or reasoning strategy.

Examples include:
- ReAct: reason, act, observe, and continue or finish.
- Plan-and-execute: construct a plan, validate it, and execute bounded steps.
- Reflection: evaluate a candidate and refine it using feedback.
- Reflexion: use verbal feedback or experience as part of subsequent attempts.
- Router: select a specialized path, tool, or agent.
- Planner-replanner: revise a plan when execution changes the known state.

A named paper technique is not automatically a complete software architecture. AgentForge documents its actual scope and mechanics.

## Capability

A capability is reusable functionality available to an agent or workflow.

Examples:
- Tool execution.
- Conversation context and memory.
- Retrieval and RAG.
- Structured output.
- Streaming.
- Embeddings.
- Evaluation.
- Human approval.
- Observability and tracing.

A retrieval pipeline or memory store is not automatically an agent.

## Tool

A tool is a declared operation that an agent can request through a defined contract. Tools have schemas, validation, authorization, side effects, timeouts, and execution outcomes.

A model-requested tool call is untrusted input. The runtime/application decides whether the call is valid and permitted.

## Workflow

A workflow is an explicit composition of steps, often a sequence or graph. It may call agents, tools, or ordinary functions. It need not be autonomous.

Examples: sequential execution, conditional branching, parallel fan-out/fan-in, map-reduce, event-driven routing, and durable state graphs.

## Multi-agent architecture

A multi-agent architecture coordinates multiple distinct agents. It defines communication, roles, delegation, shared or private state, conflict resolution, and termination.

Examples: supervisor, hierarchical teams, peer collaboration, debate, critic-generator, contract-net, auction-based allocation, blackboard, and swarm systems.

## Protocol and integration

A protocol defines how systems communicate. A provider API, MCP, or agent-to-agent protocol is an integration boundary, not an agent algorithm.

## Runtime and infrastructure

Runtime infrastructure handles common execution concerns such as identity, context, cancellation, budgets, lifecycle, state, events, and errors. It should support agent implementations without owning their decision logic.

## Why the distinctions matter

A correct taxonomy leads to better APIs and learning. For example, A* belongs as a search strategy; a goal-based navigation agent may use A*. RAG is a capability/pipeline; a research agent may use RAG. MCP is a protocol; an agent may use tools exposed through MCP. A workflow may orchestrate agents without itself becoming an agent.

See [Agent Catalog](agent-catalog.md) for the detailed inventory.
