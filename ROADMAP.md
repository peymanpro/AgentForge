# AgentForge Roadmap

This roadmap describes the intended engineering sequence. It is a living plan, not a promise that every candidate will be implemented in one release.

AgentForge is a **single .NET / C# repository**. The goal is a broad, modular agent-engineering toolkit and learning codebase. The catalog records the landscape; implementation proceeds in tested, dependency-aware slices.

## Phase 0 — .NET repository foundation

- [x] Make the repository a single .NET solution rather than a multi-language monorepo.
- [x] Place the solution, source, tests, samples, and build files at the repository root.
- [x] Define precise agent-engineering terminology and module boundaries.
- [x] Maintain an extensible catalog of classical, LLM, and multi-agent approaches.
- [x] Establish deterministic test and sample conventions.
- [ ] Add a consistent module documentation template and learning exercises across all existing modules.

## Phase 1 — Strengthen the shared runtime

- [ ] Review and consolidate agent, execution-context, result, event, and failure contracts.
- [ ] Define hierarchical and aggregate budgets for nested agents and workflows.
- [ ] Add explicit run state and state-transition semantics.
- [ ] Improve dependency-injection composition without forcing DI on every consumer.
- [ ] Add first-class deterministic clocks, model fakes, and tool fakes.
- [ ] Review streaming, structured output, multimodal, and embedding capability contracts.
- [ ] Add runtime-level conformance tests across all AgentForge agent modules.
- [ ] Define structured tracing and privacy-safe observability contracts.

## Phase 2 — Classical AI agents and decision systems

- [x] Define a design and learning sequence for classical agent architectures.
- [x] Implement simple-reflex and model-based reflex foundations (experimental).
- [ ] Implement goal-based and utility-based agent examples.
- [ ] Implement rule-based / production-system agents and inference strategies.
- [ ] Implement a reusable state-space search foundation.
- [ ] Add uninformed and heuristic search strategies, including BFS, DFS, uniform-cost, greedy best-first, A*, and iterative-deepening variants.
- [ ] Add local-search and optimization strategies where they provide reusable agent behavior.
- [ ] Add adversarial/game-playing decision agents using minimax, alpha-beta, expectimax, and MCTS.
- [ ] Implement BDI and hybrid-agent prototypes after belief, desire, intention, and planning semantics are specified.
- [ ] Add bandit and reinforcement-learning agents, beginning with tabular methods before deep RL.
- [ ] Document which algorithms are reusable strategies and which classes are complete autonomous agents.

## Phase 3 — LLM agent patterns

- [x] ReAct agent (experimental).
- [x] Plan-and-execute agent (experimental).
- [x] Reflection-based refinement agent (experimental).
- [ ] Add router / tool-selection agents and explicit handoff behavior.
- [ ] Add planner-replanner and adaptive planning.
- [ ] Add Reflexion and persistent experience feedback.
- [ ] Add Self-Refine and evaluator-driven refinement variants.
- [ ] Study and prototype Tree of Thoughts, Graph of Thoughts, and search-based reasoning such as LATS/RAP.
- [ ] Add CodeAct and constrained code-execution patterns with strict sandbox boundaries.
- [ ] Add specialized research, data-analysis, SQL, coding, and browser-use agents.
- [ ] Record which named approaches are prompting techniques, algorithms, or full execution architectures.

## Phase 4 — Context, memory, and knowledge

- [x] Baseline retrieval contract and in-memory lexical implementation (experimental).
- [x] Bounded RAG context assembly with provenance (experimental).
- [ ] Conversation state and explicit context-window management.
- [ ] Short-term/buffer and summarized memory.
- [ ] Long-term semantic, episodic, entity, and procedural memory.
- [ ] Memory write policies, consolidation, decay, deletion, tenant isolation, and privacy controls.
- [ ] Dense/vector, lexical, hybrid, and metadata-filtered retrieval.
- [ ] Query rewriting, decomposition, multi-query retrieval, reranking, and relevance feedback.
- [ ] Graph, temporal, and multimodal retrieval strategies.
- [ ] Adaptive/agentic RAG, corrective retrieval, and self-evaluating retrieval pipelines.

## Phase 5 — Workflow and orchestration

- [x] Bounded sequential and conditional workflow composition (experimental).
- [ ] Parallel fan-out/fan-in and map-reduce workflows.
- [ ] Explicit loop, retry, compensation, and error-routing policies.
- [ ] Human approval, pause, and resume.
- [ ] Typed graph/state-machine workflows with cycle validation.
- [ ] Durable checkpoints and resumable execution.
- [ ] Persistence abstractions and crash-recovery semantics.
- [ ] Distributed execution and scheduling only after local semantics are proven.

## Phase 6 — Multi-agent systems

- [ ] Supervisor and delegation architectures.
- [ ] Hierarchical planner/worker teams.
- [ ] Peer-to-peer collaboration and role-based groups.
- [ ] Debate, critic-generator, and consensus strategies.
- [ ] Blackboard coordination.
- [ ] Contract-net, bidding, auction, and market-based allocation.
- [ ] Swarm and decentralized coordination.
- [ ] Multi-agent reinforcement learning experiments.
- [ ] Shared-state, message-passing, turn-taking, conflict-resolution, and termination contracts.
- [ ] Aggregate budgets, permission boundaries, and coordination observability.

## Phase 7 — Production engineering

- [ ] Model and agent evaluation suites: task success, correctness, groundedness, safety, latency, cost, and robustness.
- [ ] Offline datasets, golden cases, regression tests, and experiment tracking.
- [ ] Trace correlation across model calls, tools, retrieval, and nested agents.
- [ ] Policy/guardrail hooks, least privilege, prompt-injection defenses, and approval gates.
- [ ] Rate limiting, concurrency control, idempotency, retry policy, and circuit breaking.
- [ ] Caching, token/cost accounting, context compaction, and performance benchmarks.
- [ ] Multi-tenancy, secret handling, data retention, and audit events.
- [ ] Operational guidance for hosting, deployment, scaling, and incident diagnosis.

## Phase 8 — Protocols and specialized modalities

- [ ] MCP client integration and tool-server interoperability.
- [ ] Agent-to-agent protocol adapters where justified.
- [ ] Browser and computer-use automation behind isolated, permissioned tool boundaries.
- [ ] Voice, speech, real-time, image, audio, and other multimodal agent capabilities.
- [ ] Specialized document, research, business-process, and data agents.

## Release policy

There is **no current NuGet publication objective**. The project should first become technically coherent, well-tested, documented, and useful as a complete engineering codebase. Package boundaries and release mechanics can be reviewed later. Source modules are not considered production-ready simply because they compile.
