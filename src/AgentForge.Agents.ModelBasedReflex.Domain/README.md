# CSharp.AgentForge.Agents.ModelBasedReflex.Domain

Domain primitives for the Model-Based Reflex agent.

This package defines the input, rule, decision, and output values used by the Model-Based Reflex execution flow. Most applications should install `CSharp.AgentForge.Agents.ModelBasedReflex`; this domain package is then resolved as a transitive dependency.

## Included concepts

- `ModelBasedReflexInput<TPercept, TState>` combines the latest percept with the previous state supplied by the host.
- `ModelBasedReflexRule<TState, TAction>` defines an ordered condition-action rule over the updated state.
- `ModelBasedReflexDecision<TAction>` represents a selected action or an explicit no-action result.
- `ModelBasedReflexOutput<TState, TAction>` carries the new state, decision, and matching rule information.

The state is supplied to each run and returned to the host; the domain package does not own persistence.

## Installation

For the supported Fluent API, install the public agent package:

```bash
dotnet add package CSharp.AgentForge.Agents.ModelBasedReflex
```

Install this `.Domain` package directly only when you intentionally work with the lower-level domain contracts.

## Related packages

- [Model-Based Reflex agent on NuGet](https://www.nuget.org/packages/CSharp.AgentForge.Agents.ModelBasedReflex/)
- [AgentForge repository](https://github.com/peymanpro/AgentForge)
