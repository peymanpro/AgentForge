# CSharp.AgentForge.Agents.ModelBasedReflex.Application

Application use case for a stateful Model-Based Reflex decision cycle.

The application layer updates an explicit state from the previous state and current percept, then evaluates the configured rules against the updated state. The host supplies the prior state for each run and decides how to persist the returned state.

## Execution behavior

- Applies the configured state updater before evaluating rules.
- Evaluates rules in registration order; the first match selects the action.
- Returns the updated state even when no rule matches.
- Uses the shared AgentForge execution result and cancellation contracts.
- Does not retain mutable state between runs or execute environment side effects.

## Main type

`ModelBasedReflexAgent<TPercept, TState, TAction>` implements the shared `IAgent` contract and accepts a state updater plus an ordered rule collection.

## Installation

For ordinary use, install the public Fluent API package:

```bash
dotnet add package CSharp.AgentForge.Agents.ModelBasedReflex
```

This `.Application` package is a lower-level implementation dependency and is normally resolved automatically.

## Related packages

- [Model-Based Reflex agent on NuGet](https://www.nuget.org/packages/CSharp.AgentForge.Agents.ModelBasedReflex/)
- [AgentForge repository](https://github.com/peymanpro/AgentForge)
