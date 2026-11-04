# CSharp.AgentForge.Agents.SimpleReflex.Domain

Domain primitives for the Simple Reflex agent.

This package contains the framework-independent rules and decision values used by the Simple Reflex application layer. Most applications should install the public `CSharp.AgentForge.Agents.SimpleReflex` package instead; NuGet brings this domain package in transitively.

## Included concepts

- `SimpleReflexRule<TPercept, TAction>` describes a named condition and action factory.
- `SimpleReflexDecision<TAction>` represents either a selected action or an explicit no-action outcome, with an explanation.
- `SimpleReflexOutput<TAction>` carries the decision and the name of the matching rule, when present.

Rules are evaluated by the application layer in their configured order. The domain types do not execute environment side effects.

## Installation

For the supported Fluent API, install the public agent package:

```bash
dotnet add package CSharp.AgentForge.Agents.SimpleReflex
```

Install this `.Domain` package directly only when you intentionally compose or integrate the lower-level domain contracts.

## Related packages

- [Simple Reflex agent on NuGet](https://www.nuget.org/packages/CSharp.AgentForge.Agents.SimpleReflex/)
- [AgentForge repository](https://github.com/peymanpro/AgentForge)
