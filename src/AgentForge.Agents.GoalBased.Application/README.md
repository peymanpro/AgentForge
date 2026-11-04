# CSharp.AgentForge.Agents.GoalBased.Application

Application use case for bounded Goal-Based search.

The application layer performs breadth-first search over transitions supplied by the host and returns a shortest goal-reaching action sequence when one is found within the configured limits.

## Execution behavior

- Receives the initial state and target goal.
- Uses application-supplied successor and goal-predicate functions.
- Uses breadth-first search to find a shortest action sequence in the supplied unweighted state graph.
- Tracks visited states and supports a custom state comparer.
- Bounds exploration with maximum depth and maximum explored-state settings.
- Returns explicit outcomes for goal reached, goal unreachable, or a search limit reached.
- Supports cancellation through the shared AgentForge execution contract.

The result contains proposed actions. The host decides whether and how to execute them.

## Main types

- `GoalBasedAgent<TState, TGoal, TAction>` performs the search.
- `GoalBasedAgentOptions` configures `MaxDepth` and `MaxExploredStates`.
- `GoalBasedOutput<TState, TAction>` carries the status, optional plan, and explanation.

## Installation

For ordinary use, install the public Fluent API package:

```bash
dotnet add package CSharp.AgentForge.Agents.GoalBased
```

This `.Application` package is a lower-level implementation dependency and is normally resolved automatically.

## Related packages

- [Goal-Based agent on NuGet](https://www.nuget.org/packages/CSharp.AgentForge.Agents.GoalBased/)
- [AgentForge repository](https://github.com/peymanpro/AgentForge)
