# AgentForge Roadmap

AgentForge is a modular .NET agent framework delivered as independently installable NuGet packages. Its primary developer experience is selecting one agent, configuring it through a concise Fluent API, building it, and running it. This roadmap tracks API consistency, independent modules, reusable algorithms, and optional capabilities.

A catalog entry is not an implementation claim. A module is marked implemented only after its public API, behavior, tests, example, and dependency direction are verified.

## Phase 0 — Product architecture and reference modules

- [x] Establish AgentForge as a .NET-only library.
- [x] Define one independently selectable module per agent architecture/pattern.
- [x] Define Clean Architecture boundaries and dependency direction.
- [x] Define module-specific fluent API conventions and a common run handle.
- [x] Split Simple Reflex into its own Domain, Application, and public module projects.
- [x] Split Model-Based Reflex into its own Domain, Application, and public module projects.
- [x] Add architecture tests for Domain/Application/Public API dependency direction in the first two agent modules.
- [ ] Expand architecture tests to every agent module as it migrates.
- [ ] Add a consistent module template and build validation.
- [ ] Complete the Fluent API contract and package-independence checks across all implemented agents.
- [ ] Add package dependency tests proving that installing one agent does not pull unrelated agent modules.
- [ ] Add a shared builder/handle conformance test suite without introducing a universal mega-builder.

## Phase 1 — Shared runtime and public API foundation

This phase prioritizes the developer experience: every implemented agent should have a consistent, concise construction and execution path. Avoid expanding the framework with unrelated infrastructure before the module APIs are coherent.

- [x] Add a concise AgentHandle for simple and advanced execution overloads.
- [ ] Review and consolidate run context, run options, failures, partial results, and event contracts.
- [ ] Introduce hierarchical/aggregate budgets for nested agents and workflows.
- [ ] Add standard metadata, naming, and immutable built configuration.
- [ ] Add middleware/behavior pipeline contracts without forcing them on every agent.
- [ ] Define explicit session and persistent-state abstractions.
- [ ] Add runtime contract tests across all agent modules.
- [ ] Add architecture tests that fail on forbidden project references.

## Phase 2 — Migrate existing LLM agent modules

- [ ] Refactor ReAct into Domain, Application, optional Infrastructure adapters, and a public fluent composition root.
- [ ] Add a module-specific ReAct builder with a concise Build()/RunAsync() happy path.
- [ ] Add a fluent ReAct builder for model, tool registry, authorization, limits, memory, and observability.
- [ ] Refactor Plan-and-Execute into clean module layers and a fluent builder; validate package independence.
- [ ] Refactor Reflection into clean module layers and a fluent builder; validate package independence.
- [ ] Add Planner-Replanner as its own module.
- [ ] Add Router/Dispatcher and tool-selection agent module.
- [ ] Add Reflexion and persistent experience feedback.
- [ ] Add Self-Refine and evaluator-guided refinement.
- [ ] Add Tree of Thoughts and Graph of Thoughts as separate modules where their search/control contracts are justified.
- [ ] Add CodeAct with a strict sandbox boundary.

## Phase 3 — Complete classical agent architectures

- [ ] Goal-Based agent.
- [ ] Utility-Based agent.
- [ ] Rule-Based / Production-System agent.
- [ ] Learning-Agent architecture (performance element, learning element, critic, problem generator).
- [ ] Reactive / Subsumption agent.
- [ ] Deliberative agent.
- [ ] Hybrid / Three-Layer agent.
- [ ] BDI agent (Beliefs, Desires, Intentions, commitment, reconsideration).
- [ ] Finite-State agent/controller.
- [ ] Behavior-Tree agent.
- [ ] Logic-Based agent.
- [ ] Define environment/simulation adapters for deterministic agent scenarios.

## Phase 4 — Reusable decision algorithms and planning modules

These are algorithm/capability packages, not automatically complete agents.

- [ ] State-space contracts and successor/goal/cost models.
- [ ] BFS, DFS, depth-limited, iterative-deepening, bidirectional search.
- [ ] Uniform-cost, greedy best-first, A*, IDA*, beam and weighted A*.
- [ ] STRIPS-style classical planning, forward/backward search, partial-order planning, HTN.
- [ ] Constraint satisfaction, propagation, and backtracking.
- [ ] Minimax, negamax, alpha-beta, expectimax, MCTS/UCT.
- [ ] Hill climbing, simulated annealing, tabu search.
- [ ] Genetic/evolutionary algorithms, PSO, ACO, and multi-objective optimization.
- [ ] Game-theoretic strategy and negotiation modules.

## Phase 5 — Reinforcement learning

- [ ] MDP and Bellman foundations.
- [ ] Multi-armed bandits: epsilon-greedy, UCB, Thompson sampling, contextual bandits.
- [ ] Monte Carlo methods and temporal-difference learning.
- [ ] SARSA, Expected SARSA, Q-learning.
- [ ] DQN and extensions.
- [ ] REINFORCE, actor-critic, PPO, DDPG, TD3, SAC.
- [ ] Model-based and hierarchical RL candidates.
- [ ] Multi-agent reinforcement learning.
- [ ] Deterministic environment interfaces, training loops, checkpoints, and evaluation datasets.

## Phase 6 — Specialist agents

- [ ] Research and evidence-synthesis agent.
- [ ] Coding and repository agent.
- [ ] Browser-use and computer-use agents.
- [ ] Data-analysis and SQL/database agents.
- [ ] Document-processing and extraction agents.
- [ ] Customer-support and business-process agents.
- [ ] Conversational/personal assistant agent.
- [ ] Voice/realtime agent.
- [ ] Vision and multimodal agent.

Specialized modules should compose the foundational agents and capabilities rather than duplicate them.

## Phase 7 — Multi-agent architectures

Each independently meaningful coordination topology receives a distinct module or clearly separated public module boundary.

- [ ] Supervisor/delegation.
- [ ] Hierarchical teams.
- [ ] Planner-worker.
- [ ] Peer-to-peer collaboration.
- [ ] Group Chat / round-robin.
- [ ] Debate/adversarial collaboration.
- [ ] Critic-generator / proposer-verifier.
- [ ] Blackboard.
- [ ] Contract-Net.
- [ ] Auction/market-based allocation.
- [ ] Consensus.
- [ ] Swarm/decentralized coordination.
- [ ] Competitive multi-agent systems.

All coordination modules need messaging, role authority, state scope, conflict handling, termination, fairness, and aggregate budgets.

## Phase 8 — Capabilities and integrations

- [ ] Microsoft.Extensions.AI adapter.
- [ ] Additional provider adapters (OpenAI, Azure OpenAI, Anthropic, Ollama, and others where supported).
- [ ] Streaming and structured-output capabilities.
- [ ] Tool schema, validation, authorization, approval, idempotency, and safe execution.
- [ ] Working, conversation, summary, semantic, episodic, entity, and procedural memory.
- [ ] Lexical, vector, hybrid, graph, and temporal retrieval.
- [ ] Reranking, query rewriting, decomposition, and contextual compression.
- [ ] RAG ingestion, provenance, grounding, and answer verification.
- [ ] MCP client/server integration and A2A adapters.
- [ ] Browser/computer-use and sandboxed code-execution infrastructure.

## Phase 9 — Production capabilities

- [ ] Evaluation datasets, scenario harnesses, regression, and model-as-judge validation.
- [ ] Task success, correctness, groundedness, safety, latency, and cost metrics.
- [ ] OpenTelemetry traces and privacy-safe logs.
- [ ] Prompt-injection threat modeling and untrusted-input handling.
- [ ] Rate limits, concurrency, backpressure, retries, circuit breakers, caching, and idempotency.
- [ ] Session persistence, checkpointing, resumption, and crash recovery.
- [ ] Multi-tenancy, retention/deletion, auditing, and secret handling.
- [ ] Hosting and deployment guidance.

## Module completion gate

Every implemented agent module MUST have:
1. Domain and Application boundaries that follow the dependency rules.
2. Infrastructure adapters only when required.
3. A module-specific fluent builder and common execution handle.
4. Build-time configuration validation.
5. Tests for success, no-op, failure, cancellation, limits, and relevant concurrency/state behavior.
6. A runnable example and concise module README.
7. Passing full-solution CI.

## Release policy

No NuGet package has been published. Package release is a later decision after the module graph, APIs, compatibility policy, dependencies, security, documentation, and support model are stable.
