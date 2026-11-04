# CSharp.AgentForge.Agents.GoalBased.Domain

Domain models for bounded goal-directed state-space search.

This package defines the typed input, transition, search status, and plan values used by the Goal-Based agent. Most applications should install `CSharp.AgentForge.Agents.GoalBased`; this domain package is a transitive implementation dependency.

## Included concepts

- `GoalBasedInput<TState, TGoal>` contains the initial state and target goal.
- `GoalBasedTransition<TState, TAction>` describes an action and the next state it proposes.
- `GoalBasedSearchStatus` distinguishes `GoalReached`, `GoalNotReachable`, and `SearchLimitReached`.
- `GoalBasedPlan<TState, TAction>` contains the proposed action sequence, state path, explored-state count, and depth.

The domain models describe a search outcome; they do not execute actions in the environment.

## Installation

For the supported Fluent API, install the public agent package:

```bash
dotnet add package CSharp.AgentForge.Agents.GoalBased
```

Install this `.Domain` package directly only when you intentionally use the low-level search contracts.

## Related packages

- [Goal-Based agent on NuGet](https://www.nuget.org/packages/CSharp.AgentForge.Agents.GoalBased/)
- [AgentForge repository](https://github.com/peymanpro/AgentForge)
