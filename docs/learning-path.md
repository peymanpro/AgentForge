# Agent Engineering Learning Path

This path uses AgentForge as a practical curriculum. It moves from simple deterministic decision systems to LLM agents, memory, orchestration, multi-agent coordination, and production engineering.

Each phase should be studied through four steps: **understand the problem → study the mechanism → inspect the implementation/tests → modify or implement a working variant.**

## Before starting: foundations

Useful prerequisites:
- C# fundamentals, async/await, interfaces, generics, dependency injection, and testing.
- Data structures, graph traversal, complexity, probability, and basic linear algebra.
- Basic machine-learning vocabulary: training, inference, features, objective, loss, evaluation, and generalization.
- HTTP, serialization, concurrency, cancellation, and basic distributed-systems concepts.

These subjects can be studied alongside AgentForge; they are not all prerequisites for the first modules.

## Phase 1 — Understand an agent

**Questions:** What distinguishes an agent from a function, chatbot, workflow, or model call? What are environment, percept, state, action, goal, policy, utility, and feedback?

Study:
- [Concepts](concepts.md)
- [Agent Catalog](agent-catalog.md), sections 1–3
- [AgentForge specification](../AGENTFORGE_SPEC.md)

Practice:
- Trace an agent run from input through decision to action and outcome.
- Draw its state machine.
- Identify what information is trusted and what is not.

Completion evidence: explain why a workflow, retrieval function, model, and tool are not automatically agents.

## Phase 2 — Runtime and contracts

Study `src/AgentForge.Abstractions`:
- Agent input/output and execution context.
- Run identity and parent/child relationship.
- Results, terminal statuses, and failure categories.
- Cancellation, options, events, and resource limits.

Study `src/AgentForge.Models`:
- Message roles and normalized model requests/responses.
- Finish reasons and tool-call proposals.
- Baseline model interface versus optional capabilities.

Practice:
- Build a deterministic fake model.
- Add a new optional model capability without making every model implement it.
- Test timeout, cancellation, unsupported capability, and provider failure.

Completion evidence: add an agent implementation that uses the shared runtime without referencing a specific vendor SDK.

## Phase 3 — Tools and trust boundaries

Study `src/AgentForge.Tools`:
- Tool contracts, schemas, registry, validation, authorization, execution.
- Model proposal versus application permission.
- Side effects, least privilege, idempotency, and safe errors.

Practice:
- Implement a read-only tool and a side-effecting tool.
- Reject malformed input and unauthorized calls before execution.
- Add an approval gate for a consequential operation.

Completion evidence: prove with tests that invalid and unauthorized calls never invoke the underlying operation.

## Phase 4 — Classical agents and algorithms

Start with the implemented `src/AgentForge.Agents.Classical` module and its [design note](design/classical-agents.md), then read the classical sections of the [Agent Catalog](agent-catalog.md).

The first two architectures are implemented experimentally: simple-reflex evaluates the current percept; model-based reflex updates an explicitly supplied internal state before evaluating rules. The thermostat sample shows the same scenario through both approaches.

Recommended order:
1. Simple-reflex agent.
2. Model-based reflex agent.
3. Goal-based and utility-based agents.
4. State-space search: BFS, DFS, uniform-cost, greedy best-first, A*.
5. Rule-based inference and conflict resolution.
6. Behavior trees, finite-state machines, and subsumption.
7. BDI and hybrid architectures.
8. Minimax/alpha-beta and MCTS for adversarial tasks.
9. Bandits, MDPs, Q-learning, SARSA, and then deep RL methods.

Practice:
- Build a grid-world or maze simulator.
- Compare search strategies on the same state space.
- Make a utility-based agent choose among actions with competing outcomes.
- Build a small game-playing agent and inspect its search tree.
- Add a tabular Q-learning agent and plot learning curves.

Completion evidence: explain how state representation, reward/utility, search strategy, and environment design affect behavior.

## Phase 5 — LLM agent patterns

Start with the implemented modules:
- `src/AgentForge.Agents.ReAct`
- `src/AgentForge.Agents.Planning`
- `src/AgentForge.Agents.Reflection`

Study:
- ReAct: decision/action/observation loop.
- Plan-and-execute: plan representation, validation, step execution.
- Reflection: evaluator, feedback, refinement, stopping.
- Tool selection, routing, replanning, Reflexion, Self-Refine.
- Tree/Graph of Thoughts, LATS, RAP, and CodeAct as research or later implementations.

Practice:
- Change the decision strategy while keeping the execution boundary unchanged.
- Make a failed step trigger a documented replanning policy.
- Compare a plain retry with evaluator-guided refinement.
- Use bounded model/tool calls and preserve partial outcomes.

Completion evidence: explain the algorithmic difference between ReAct, plan-and-execute, reflection, and retry.

## Phase 6 — Retrieval, memory, and RAG

Start with:
- `src/AgentForge.Retrieval`
- `src/AgentForge.RAG`

Study:
- Lexical versus dense/vector versus hybrid retrieval.
- Embeddings, chunking, metadata, filters, authorization, and reranking.
- Query transformations and retrieval evaluation.
- Working, conversation, episodic, semantic, entity, and procedural memory.
- Context-window management and compaction.
- Provenance, grounding, and untrusted retrieved text.

Practice:
- Compare lexical and vector retrieval on a fixed query/document set.
- Measure recall@k, precision@k, and ranking quality.
- Add reranking and inspect when it helps or hurts.
- Implement a memory policy with explicit read/write/delete behavior.
- Build a RAG case with conflicting or insufficient evidence.

Completion evidence: distinguish retrieval, memory, RAG, and an autonomous retrieval agent.

## Phase 7 — Orchestration and stateful workflows

Start with `src/AgentForge.Orchestration`.

Study:
- Sequential and conditional composition.
- Explicit dataflow and typed step outputs.
- Failure propagation, skipped/blocked steps, partial results, and cancellation.
- Parallel fan-out/fan-in, cycles, loops, retries, compensation.
- Checkpoints, persistence, pause/resume, human approval, and durable execution.

Practice:
- Compose two different agents and a deterministic condition.
- Add failure routing and approval.
- Compare in-memory workflow state with persisted checkpoints.

Completion evidence: explain why an orchestrator is not necessarily an agent and define its failure and recovery semantics.

## Phase 8 — Multi-agent systems

Read the multi-agent section of the [Agent Catalog](agent-catalog.md).

Study:
- Supervisor and hierarchical delegation.
- Planner-worker and peer-to-peer systems.
- Debate, critic-generator, consensus, blackboard, contract-net, auctions, and swarms.
- Message passing, shared/private state, role authority, conflict resolution, termination, and aggregate budgets.

Practice:
- Start with two agents and one explicit coordinator.
- Add a worker failure and measure how it affects the global task.
- Prevent delegation loops and unbounded agent spawning.
- Compare centralized and decentralized coordination.

Completion evidence: describe when multiple agents add value over one well-designed agent or workflow.

## Phase 9 — Evaluation, safety, and production

Study the [Capability Map](capability-map.md), especially evaluation, security, observability, and operations.

Study:
- Golden datasets, regression, model-as-judge, and judge calibration.
- Task success, correctness, groundedness, safety, latency, and cost.
- Tracing, correlated model/tool/retrieval/workflow events.
- Prompt injection, least privilege, sandboxing, approval, tenant boundaries, secrets, retention, and audit.
- Rate limits, concurrency, caching, retries, idempotency, circuit breakers, and recovery.

Practice:
- Build an evaluation dataset before changing an agent prompt or algorithm.
- Add traces with redaction by default.
- Run prompt-injection and unauthorized-tool scenarios.
- Benchmark latency, throughput, token usage, and cost.

Completion evidence: diagnose a failed run from trace/test evidence and explain its security and operational limits.

## Phase 10 — Specialized agents and research

Build task-specific systems by composing the foundations:
- Research and evidence-synthesis agents.
- Coding and repository agents.
- Browser/computer-use agents.
- SQL and data-analysis agents.
- Document-processing agents.
- Voice, realtime, vision, and multimodal agents.
- Domain-specific planning, scheduling, or decision agents.

For each, start with a threat model, tool boundary, deterministic scenario, and evaluation set—not just a prompt.

## Study routine for each module

1. Read its module README and design note.
2. Inspect the public contracts before implementation details.
3. Trace one successful run through the source.
4. Trace one failure, cancellation, and budget-exhaustion path.
5. Run and modify the tests.
6. Run the sample.
7. Write a short note: problem, algorithm, assumptions, trade-offs, limitations.
8. Implement a small variation and test it.

AgentForge is a substantial practical curriculum, not a certification or a substitute for mathematics, ML theory, independent research, and real-world operating experience.
