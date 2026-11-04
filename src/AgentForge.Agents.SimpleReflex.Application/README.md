# CSharp.AgentForge.Agents.SimpleReflex.Application

Application use case for one Simple Reflex decision cycle.

This package implements the Simple Reflex execution flow over the domain rules and the shared AgentForge execution contract. Most applications should install `CSharp.AgentForge.Agents.SimpleReflex`, which provides the supported Fluent builder and depends on this package transitively.

## Execution behavior

- Evaluates the configured condition-action rules in registration order.
- Selects the action produced by the first matching rule.
- Returns an explicit no-action decision when no rule matches.
- Validates run options and returns normalized AgentForge execution outcomes.
- Observes cancellation during rule evaluation.

The selected action is a proposal. The application layer does not perform environment side effects or persist external state.

## Main type

`SimpleReflexAgent<TPercept, TAction>` implements `IAgent<TPercept, SimpleReflexOutput<TAction>>` and accepts an ordered collection of domain rules.

## Installation

For ordinary use, install the public Fluent API package:

```bash
dotnet add package CSharp.AgentForge.Agents.SimpleReflex
```

This `.Application` package is a lower-level implementation dependency and is normally resolved automatically.

## Related packages

- [Simple Reflex agent on NuGet](https://www.nuget.org/packages/CSharp.AgentForge.Agents.SimpleReflex/)
- [AgentForge repository](https://github.com/peymanpro/AgentForge)
