# CSharp.AgentForge.Agents.Learning.Domain

Domain state, input, statistics, and policy contracts for feedback-driven learning agents.

This package contains immutable learning snapshots and typed values shared by the Learning application layer. Most applications should install `CSharp.AgentForge.Agents.Learning`; this domain package is resolved transitively.

## Included concepts

- `LearningAgentInput<TAction>` and `LearningAgentOutput<TAction>` model a context-free action-selection decision.
- `LearningState<TAction>` stores immutable per-action reward statistics and returns a new snapshot from `Observe`.
- `ActionStatistics` and `ActionEstimate<TAction>` represent observations and estimated rewards.
- `ContextualLearningAgentInput<TAction>` and `ContextualLearningAgentOutput<TAction>` carry candidate actions, a fixed-dimensional context, and score diagnostics.
- `ContextualLearningState<TAction>` stores immutable sufficient statistics for disjoint linear contextual bandits.
- `ILearningPolicy<TAction>` defines the selection policy contract used by the context-free use case.

The host supplies feedback/rewards and owns persistence of the returned state snapshots.

## Installation

For the supported Fluent API, install the public agent package:

```bash
dotnet add package CSharp.AgentForge.Agents.Learning
```

Install this `.Domain` package directly only when you intentionally compose learning policies or state contracts.

## Related packages

- [Learning agent on NuGet](https://www.nuget.org/packages/CSharp.AgentForge.Agents.Learning/)
- [AgentForge repository](https://github.com/peymanpro/AgentForge)
