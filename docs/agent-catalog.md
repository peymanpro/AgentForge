# Agent and Algorithm Catalog

This catalog is the product inventory for AgentForge. It is intentionally broader than the current implementation and records classical architectures, algorithms, modern LLM agent patterns, specialist agents, and multi-agent coordination systems.

Each independently meaningful agent type is intended to have its own module/package. Algorithms, capabilities, and protocols remain distinct reusable packages rather than being mislabeled as agents.

**Status meanings**
- **Implemented (experimental):** working source code and tests exist in this repository; the API is not yet stable.
- **Planned:** accepted as a candidate for the engineering roadmap, but no complete implementation exists yet.
- **Research:** worth studying or prototyping; inclusion and boundaries still need evaluation.
- **Capability / architecture / algorithm:** the item is deliberately not classified as a standalone agent.

Do not interpret a catalog entry as an implementation claim. See [Concepts](concepts.md) for the classification rules.

## 1. Classical agent models and architectures

| Approach | Correct classification | Main idea | Status |
|---|---|---|---|
| Simple-reflex agent | Agent architecture | Select an action from the current percept using ordered condition-action rules | Implemented (experimental) — [module README](../src/AgentForge.Agents.SimpleReflex/README.md) |
| Model-based reflex agent | Agent architecture | Update an explicit internal state from prior state and current percept, then apply condition-action rules | Implemented (experimental) — [module README](../src/AgentForge.Agents.ModelBasedReflex/README.md) |
| Goal-based agent | Agent architecture | Select actions by considering goal achievement | Planned |
| Utility-based agent | Agent architecture | Compare outcomes using a utility/preference function | Planned |
| Learning agent | Agent architecture | Improve performance using feedback; commonly separates performance, learning, critic, and exploration/problem-generation responsibilities | Planned |
| Rule-based / production-system agent | Agent architecture | Apply facts, rules, conflict resolution, and inference | Planned |
| Reactive / subsumption agent | Agent architecture | Layered behaviors with priority and suppression/inhibition | Planned |
| Deliberative agent | Agent architecture | Build an explicit world model and reason or plan before acting | Planned |
| Hybrid / three-layer agent | Agent architecture | Combine reactive control with deliberation and planning | Planned |
| BDI agent | Agent architecture | Maintain beliefs, desires, intentions, and commitment/reconsideration rules | Planned |
| Blackboard system | Coordination architecture | Specialists coordinate through a shared workspace and control mechanism | Research |
| Behavior-tree agent | Control architecture | Compose decisions from sequence, selector, condition, and action nodes | Planned |
| Finite-state / hierarchical-state agent | Control architecture | Choose actions from explicit state transitions and guards | Planned |
| Logic-based agent | Agent architecture | Represent knowledge and derive actions through logical inference | Research |

## 2. Classical search, planning, and decision algorithms

These are reusable strategies that may power a complete agent. They are not automatically agents by themselves.

### Uninformed and heuristic search

- Breadth-first search (BFS), depth-first search (DFS), depth-limited search, iterative deepening, bidirectional search.
- Uniform-cost search.
- Greedy best-first search.
- A*, iterative-deepening A* (IDA*), beam search, weighted A*.
- State-space representation, successor generation, goal tests, path costs, heuristics, admissibility, consistency, duplicate detection, and frontier policies.

### Planning and constraint reasoning

- Classical state-space planning and STRIPS-style operators.
- Forward and backward planning.
- Partial-order planning.
- Hierarchical Task Network (HTN) planning.
- Constraint satisfaction: variables, domains, constraints, backtracking, constraint propagation, and heuristics.
- Scheduling, resource allocation, and temporal planning.
- Replanning and plan repair after new observations.

### Local search and optimization

- Hill climbing and stochastic/local variants.
- Simulated annealing.
- Tabu search.
- Genetic and evolutionary algorithms.
- Particle Swarm Optimization (PSO).
- Ant Colony Optimization (ACO).
- Artificial Bee Colony and other swarm/metaheuristic variants.
- Multi-objective optimization and Pareto trade-offs.

### Adversarial and game-playing decision methods

- Minimax and negamax.
- Alpha-beta pruning.
- Expectimax and chance-node reasoning.
- Monte Carlo Tree Search (MCTS).
- Rollouts, UCT, evaluation functions, transposition tables, and game-state modeling.
- Game-theoretic policies, payoff matrices, equilibrium concepts, and negotiation strategies.

## 3. Bandits and reinforcement-learning agents

| Family | Methods and concepts | Status |
|---|---|---|
| Multi-armed bandits | Epsilon-greedy, optimistic initialization, UCB, Thompson sampling, contextual bandits | Planned |
| Dynamic programming | Markov Decision Processes, Bellman equations, policy iteration, value iteration | Planned |
| Monte Carlo learning | Return estimation, on-policy and off-policy prediction/control | Planned |
| Temporal-difference learning | TD(0), n-step TD, SARSA, Expected SARSA, Q-learning | Planned |
| Deep value-based RL | DQN, target networks, replay buffers, Double DQN, Dueling DQN, prioritized replay | Planned |
| Policy-gradient methods | REINFORCE and variance reduction | Planned |
| Actor-critic methods | A2C/A3C, PPO, DDPG, TD3, SAC | Planned |
| Model-based RL | Learned dynamics, planning with a model, model-predictive control | Research |
| Hierarchical RL | Options, subgoals, temporal abstraction, hierarchical policies | Research |
| Multi-agent RL | Independent learners, MADDPG, QMIX, value decomposition, centralized training/decentralized execution | Research |

Implementation should begin with small tabular environments and deterministic simulations before deep RL. This keeps the mathematics, state transitions, reward design, exploration, and evaluation understandable.

## 4. LLM-based single-agent patterns

| Approach | Main mechanism | Classification | Status |
|---|---|---|---|
| Basic tool-using agent | Model proposes a tool call; runtime validates, authorizes, executes, and returns an observation | Agent pattern | Planned expansion |
| ReAct | Interleave reasoning, action, observation, and stopping | Agent pattern | Implemented (experimental) — [module README](../src/AgentForge.Agents.ReAct/README.md) |
| Plan-and-execute | Generate and validate a plan, then execute bounded steps | Agent pattern | Implemented (experimental) — [module README](../src/AgentForge.Agents.Planning/README.md) |
| Planner-replanner | Revise the remaining plan after new observations or failed steps | Agent pattern | Planned |
| Reflection | Evaluate a candidate and use feedback to refine it | Agent pattern | Implemented (experimental) — [module README](../src/AgentForge.Agents.Reflection/README.md) |
| Reflexion | Store verbal feedback/experience and use it in later attempts | Agent pattern | Planned |
| Self-Refine | Iteratively generate, critique, and revise output | Refinement pattern | Planned |
| Router / dispatcher | Classify a task and select a tool, agent, model, or execution path | Agent pattern when it owns the routing decision | Planned |
| Tree of Thoughts | Explore and evaluate multiple reasoning branches | Search-based reasoning pattern | Research |
| Graph of Thoughts | Explore and combine a graph of intermediate reasoning states | Search-based reasoning pattern | Research |
| Language Agent Tree Search (LATS) | Combine language-model proposals with tree search and value/feedback signals | Agent architecture | Research |
| Reasoning via Planning (RAP) | Use planning/search concepts to guide model reasoning | Agent architecture | Research |
| ReWOO-style execution | Separate planning from tool execution and observation collection | Agent pattern | Research |
| CodeAct | Express actions as code and execute through a constrained runtime | Agent pattern; requires a sandbox | Planned |
| Self-consistency | Sample multiple reasoning paths and aggregate answers | Decoding/evaluation technique, not a complete agent | Research |
| Prompt chaining | Connect explicit model calls in a controlled sequence | Workflow pattern, not automatically an agent | Planned capability |
| Toolformer-style use | Learn or select tool invocation behavior from training/signals | Training/use technique | Research |

## 5. Specialized LLM agents

These are task-oriented compositions built from the foundational patterns and capabilities. They should not become independent frameworks unless they require genuinely distinct contracts.

- Research agent: question decomposition, source discovery, retrieval, evidence tracking, synthesis, citation checking, and report generation.
- Coding agent: repository inspection, issue decomposition, patch proposal, tests, tool permissions, review, and bounded iteration.
- Browser/computer-use agent: navigation, observation, interaction, verification, and approval boundaries.
- Data-analysis agent: data inspection, hypothesis generation, statistical/SQL/Python tool execution in a controlled environment, and result validation.
- SQL/database agent: schema discovery, query generation, read/write policy, query validation, execution, and result explanation.
- Document-processing agent: parsing, extraction, classification, cross-document linking, and structured output.
- Conversational/personal assistant: dialogue state, user preferences, memory, task planning, and handoff.
- Customer-support/workflow agent: policy-grounded answers, case lookup, action proposals, escalation, and human approval.
- Voice/realtime agent: speech input/output, turn management, interruption, latency, and multimodal context.
- Vision/multimodal agent: images, audio, video, structured visual inputs, and cross-modal tool use.

## 6. Multi-agent architectures

These are system-level coordination patterns. Individual participants may be ordinary agents; the architecture defines how they coordinate.

| Architecture | Coordination mechanism | Status |
|---|---|---|
| Supervisor | A coordinator delegates tasks to worker agents and integrates results | Planned |
| Hierarchical team | Managers delegate recursively across levels | Planned |
| Planner-worker | A planner decomposes work; workers execute bounded subtasks | Planned |
| Peer-to-peer collaboration | Agents exchange messages and negotiate without one permanent supervisor | Planned |
| Round-robin/group chat | Agents take turns under an explicit turn-selection policy | Planned |
| Debate / adversarial collaboration | Agents propose and challenge alternatives under a termination policy | Planned |
| Generator-critic / proposer-verifier | Separate proposal and verification roles | Planned |
| Blackboard | Agents read/write a shared workspace coordinated by control rules | Research |
| Contract-net | A manager announces work; agents bid; a contract is awarded and monitored | Research |
| Auction / market-based allocation | Work or resources are allocated through bids and utility | Research |
| Consensus | Agents combine beliefs or decisions under a stated aggregation rule | Research |
| Swarm / decentralized | Many limited agents coordinate through local rules or shared signals | Research |
| Competitive multi-agent systems | Agents optimize conflicting or adversarial objectives | Research |

A multi-agent implementation must specify communication, role authority, shared/private state, delegation, conflict resolution, fairness, termination, and aggregate budgets.

## 7. Capabilities and infrastructure that support agents

These are not agents by themselves, but they are essential to agent engineering:

- Model adapters and optional capabilities: tool calling, structured output, streaming, embeddings, vision, audio, token accounting.
- Prompt templates, versioning, parameterization, and prompt-injection-aware boundaries.
- Execution context, state machines, run identity, cancellation, timeout, budgets, and lifecycle events.
- Tool registry, schemas, validation, authorization, idempotency, side effects, concurrency, and sandboxing.
- Memory: buffer, summary, semantic, episodic, entity, procedural, working memory, consolidation, decay, retention, and deletion.
- Retrieval: lexical, vector/dense, hybrid, metadata filters, reranking, query transformation, graph and temporal retrieval.
- RAG: ingestion, chunking, indexing, retrieval, grounding, provenance, context assembly, and answer verification.
- Orchestration: sequential, conditional, parallel, loops, map-reduce, state graphs, retries, compensation, durable checkpoints, and human approval.
- Evaluation: deterministic tests, golden datasets, model-as-judge, task success, groundedness, safety, regression, and benchmark design.
- Observability: structured logs, traces, metrics, correlation, cost/latency, privacy, and redaction.
- Security: threat modeling, least privilege, tenant isolation, secrets, prompt injection, untrusted content, guardrails, and human approval.
- Protocols: provider APIs, MCP, agent-to-agent communication, browser/computer interfaces, and event transport.
- Production engineering: caching, rate limits, backpressure, retries, circuit breakers, deployment, scaling, and incident response.

See [Capability Map](capability-map.md) for the wider taxonomy and [Roadmap](../ROADMAP.md) for implementation sequencing.

## 8. Catalog governance

A new entry should include its classification, core references, problem, algorithm/architecture, dependencies, risks, and proposed tests. Similar techniques should be grouped when their implementations share one meaningful boundary. The catalog should remain comprehensive without forcing every named paper technique into an independent package.
