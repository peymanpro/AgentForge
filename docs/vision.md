# Vision

AgentForge aims to become a comprehensive, modular .NET agent-engineering toolkit and a practical learning environment.

The central idea is simple: an agent should be understood as an engineered decision system, not as a prompt wrapped around an LLM. Its behavior emerges from a model of the environment, state, policy, memory, planning, tools, evaluation, execution limits, and feedback.

## Two goals, one codebase

### 1. A useful engineering library

AgentForge should provide replaceable, composable implementations of classical and modern agent systems. Developers should be able to select the agent architecture and capabilities that match their problem without adopting an opaque, all-in-one framework.

### 2. A deep learning environment

The source code should also teach the discipline. Each implemented module should connect:
- The problem it solves.
- The theory and terminology behind it.
- The algorithm or architecture.
- The execution lifecycle.
- The trade-offs and failure modes.
- The tests and deterministic examples.
- The path toward more advanced variants.

Learning should progress from deterministic classical systems to model-driven agents, then to memory, orchestration, multi-agent coordination, and production operations.

## What AgentForge covers

- Classical agent models and symbolic decision-making.
- Search, planning, game-playing, optimization, and reinforcement learning.
- LLM-based agent patterns and tool use.
- State, memory, retrieval, and RAG.
- Workflows and orchestration.
- Multi-agent architectures and coordination.
- Evaluation, observability, security, reliability, and deployment.
- Specialized coding, research, data, browser, voice, and multimodal agents.

See [Agent Catalog](agent-catalog.md) for the classified inventory and [Learning Path](learning-path.md) for the study sequence.

## Principles

1. **Mechanisms before labels.** A pattern must be implemented and explained, not merely named.
2. **Classical and modern AI belong together.** LLMs are an important tool, not the definition of an agent.
3. **Precise taxonomy.** Algorithms, architectures, capabilities, and protocols are not automatically agents.
4. **Modularity with discipline.** Boundaries should make systems easier to understand, test, reuse, and extend.
5. **Provider independence.** The model provider should be replaceable without rewriting the agent algorithm.
6. **Safety is part of the design.** Tool authority, data trust, side effects, and limits are first-class concerns.
7. **Learning through evidence.** Tests, traces, examples, diagrams, and design records are part of each implementation.
8. **Breadth without fake completeness.** The catalog may be broad; implementation status must remain honest.

## What success looks like

A developer should be able to study an idea, trace it to its implementation, run it, inspect its tests, understand its limitations, and compose it into a larger system. Over time, AgentForge should become a coherent place to learn and practice agent engineering across the full lifecycle—from decision theory to production operations.

AgentForge can support a substantial path toward becoming an agent engineer. It cannot replace foundational study, independent experimentation, mathematical understanding, or production experience.
