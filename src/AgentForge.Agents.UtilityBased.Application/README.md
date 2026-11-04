# CSharp.AgentForge.Agents.UtilityBased.Application

Application use case for Utility-Based action selection.

This package evaluates a finite candidate set for the current percept and returns the action with the highest utility. It uses the domain decision/evaluation types and the shared AgentForge execution contract.

## Execution behavior

- Obtains candidate actions from an application-supplied candidate provider.
- Evaluates each candidate with an application-supplied utility function.
- Preserves the evaluations in the output rather than returning only the winner.
- Applies the configured tie-break policy for equal maximum scores.
- Returns a no-action decision when the candidate set is empty.
- Rejects non-finite utility scores and supports cancellation.

The selected action is a proposal; the host remains responsible for performing side effects.

## Main type

`UtilityBasedAgent<TPercept, TAction>` implements `IAgent<TPercept, UtilityBasedOutput<TAction>>`.

## Installation

For ordinary use, install the public Fluent API package:

```bash
dotnet add package CSharp.AgentForge.Agents.UtilityBased
```

This `.Application` package is a lower-level implementation dependency and is normally resolved automatically.

## Related packages

- [Utility-Based agent on NuGet](https://www.nuget.org/packages/CSharp.AgentForge.Agents.UtilityBased/)
- [AgentForge repository](https://github.com/peymanpro/AgentForge)
