# Goal-Based Agent Design

## Purpose and boundary

The Goal-Based module turns an explicit initial state and goal into a sequence of proposed actions. It owns the search process, not the environment or action side effects.

This is deliberately different from the neighboring modules:

- **Simple Reflex** maps the current percept to the first matching rule.
- **Utility-Based** scores available actions and selects the highest-scoring one.
- **Goal-Based** searches state transitions to find an action sequence satisfying a goal.
- **Plan-and-Execute** validates a supplied/generated plan and invokes registered step handlers.

## Initial algorithm

The first implementation uses breadth-first search (BFS) over an application-supplied state-transition graph.

Inputs:
- Initial state.
- Goal value.
- Successor provider: state -> ordered transitions (action, next state).
- Goal predicate: (state, goal) -> bool.

The search:
1. Checks whether the initial state already satisfies the goal.
2. Enqueues the initial state and records it in a visited set.
3. Dequeues states in FIFO order.
4. Evaluates successors in provider order and ignores previously visited states.
5. Returns as soon as a newly discovered state satisfies the goal.
6. Stops when the reachable state space is exhausted or a configured resource limit is reached.

Because BFS explores by increasing depth, the first returned plan has the fewest transitions when all transitions have equal cost.

## Bounded execution

Two independent limits protect the search:
- Maximum depth (default 32).
- Maximum explored states (default 10,000).

Outcomes distinguish:
- `GoalReached`: a plan was found.
- `GoalNotReachable`: the reachable state space was exhausted.
- `SearchLimitReached`: a configured bound prevented a complete reachability conclusion.

Cancellation is cooperative and checked during traversal and successor enumeration. Provider and predicate exceptions are normalized into a failed agent run; internal exception details are not returned.

## State identity and determinism

The caller may provide an `IEqualityComparer<TState>`. The default comparer is used otherwise. The successor provider's enumeration order determines tie ordering among equal-depth plans. Applications should provide stable, deterministic transitions when reproducibility matters.

## Non-goals of the first implementation

- No action execution, authorization, persistence, or environment mutation.
- No LLM-based planning.
- No heuristic/A* search or weighted transition costs.
- No probabilistic transitions, learning, multi-agent coordination, or replanning.
- No optimality guarantee for non-uniform action costs.

Weighted-cost search can be considered separately if a concrete use case justifies the additional API and invariants.
