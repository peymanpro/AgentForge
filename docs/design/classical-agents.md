# Classical Agent Architectures

This design note defines how AgentForge will grow its deterministic, non-LLM agent families.

## 1. Why classical agents belong here

Agent engineering did not begin with language models. Classical agents provide the clearest way to study perception, state, action, goals, utility, rules, search, planning, feedback, and control loops without introducing model-provider behavior into every example.

AgentForge implements classical agents as first-class .NET modules alongside LLM-based agents. They use the same normalized run contract where appropriate, but their decision logic must remain independent from model-provider abstractions.

## 2. Shared vocabulary

- **Environment:** the system or world in which an agent observes and acts.
- **Percept:** the information currently observed from the environment.
- **Internal state:** information retained or inferred from the current and previous percepts.
- **Action:** a proposed operation that may change the environment.
- **Policy:** the rule or decision procedure that maps information to an action.
- **Goal:** a condition or desired state the agent attempts to reach.
- **Utility:** a numerical preference model used to compare possible outcomes.
- **Feedback:** an observation about the result of a previous action, used to update state or behavior.

An AgentForge agent chooses or proposes an action. The environment adapter or application remains responsible for performing external side effects and enforcing its permissions.

## 3. Current implementation boundary

The `AgentForge.Agents.Classical` project currently provides:

| Architecture | Decision input | Mechanism | State ownership |
|---|---|---|---|
| Simple-reflex | Current percept | Ordered condition-action rules; first match wins | No retained state |
| Model-based reflex | Previous state and current percept | Update internal state, then evaluate ordered rules | Explicitly passed in and returned |

These are bounded, deterministic decision steps, not environment simulators. The caller decides when to observe again and how to apply the chosen action.

Model-based state is intentionally explicit in the input/output contract rather than hidden in mutable agent fields. This avoids cross-run state leakage and lets the application decide whether to keep state in memory, persist it, or reconstruct it.

## 4. Planned classical families

The implementation sequence is deliberately dependency-aware:

1. **Goal-based agents and search:** define state-space and transition contracts; implement BFS, DFS, uniform-cost, greedy best-first, and A* as reusable search strategies; then compose them into goal-directed agents.
2. **Utility-based agents:** define candidate-action evaluation, normalized utility values, deterministic tie-breaking, and multi-objective trade-offs.
3. **Rule-based / production systems:** represent facts and working memory, match rule premises, define conflict-resolution strategies, and apply controlled rule firing.
4. **Finite-state and behavior-tree controllers:** model explicit states, guards, transitions, sequence/selector nodes, and bounded ticks.
5. **Reactive / subsumption systems:** define behavior layers, priority, inhibition, and arbitration.
6. **Adversarial decision agents:** add minimax, alpha-beta, expectimax, and Monte Carlo Tree Search over explicit game-state interfaces.
7. **BDI and hybrid architectures:** model beliefs, desires, intentions, deliberation, commitment, reconsideration, and reactive overrides.
8. **Learning agents:** separate the performance element, learning element, critic, and problem generator; connect the architecture to bandits, MDPs, and reinforcement-learning algorithms.

The catalog also records optimization, scheduling, constraint reasoning, game theory, evolutionary algorithms, and multi-agent reinforcement learning. Those algorithms should be reusable strategies or domain examples where appropriate, not automatically separate agents.

## 5. Module design rules

- Keep decision logic deterministic and testable wherever possible.
- Do not force a live LLM or vendor SDK into classical agents.
- Keep environment interaction behind explicit application-supplied contracts.
- Avoid hidden mutable state on reusable agent instances.
- Make rule ordering, tie-breaking, termination, and no-action behavior explicit.
- Preserve the distinction between an agent architecture and a reusable algorithm.
- Add a new source project only when a meaningful public boundary or dependency boundary justifies it.
- Every implementation needs tests, a runnable example, a module README, a design explanation, and documented limitations.

## 6. Learning objectives

After studying the classical modules, a developer should be able to:

- Model a problem as states, percepts, actions, goals, transitions, and costs.
- Explain the difference between a reflex policy and an internal world model.
- Implement deterministic rule selection and reason about rule ordering.
- Trace search frontiers, visited states, path costs, and heuristic behavior.
- Compare goal satisfaction with utility maximization.
- Explain how feedback, learning, and exploration differ from fixed policies.
- Decide when an agent is unnecessary and a deterministic function or workflow is the better abstraction.
