# CSharp.AgentForge.Agents.Learning.Application

Application use cases for context-free and contextual learning decisions.

This package implements the action-selection steps over the immutable state and policy contracts in the Learning domain package.

## Use cases

- `SelectLearningAction<TAction>` builds per-action reward estimates from `LearningState<TAction>` and delegates selection to an `ILearningPolicy<TAction>`. The public Learning module supplies the epsilon-greedy policy through its Fluent API.
- `SelectContextualLearningAction<TAction>` implements disjoint LinUCB scoring over the supplied context and per-action sufficient statistics. Its output includes estimated reward, exploration bonus, and upper-confidence score.

The application layer selects an action; the host evaluates the outcome, applies reward feedback to the immutable state, and persists the resulting snapshot.

## Installation

For ordinary use, install the public Fluent API package:

```bash
dotnet add package CSharp.AgentForge.Agents.Learning
```

This `.Application` package is a lower-level implementation dependency and is normally resolved automatically.

## Related packages

- [Learning agent on NuGet](https://www.nuget.org/packages/CSharp.AgentForge.Agents.Learning/)
- [AgentForge repository](https://github.com/peymanpro/AgentForge)
