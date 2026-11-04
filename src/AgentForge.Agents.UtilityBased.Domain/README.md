# CSharp.AgentForge.Agents.UtilityBased.Domain

Domain decision and evaluation types for the Utility-Based agent.

This package contains the values that describe candidate utility scores, selected actions, and tie-breaking behavior. Most applications should install `CSharp.AgentForge.Agents.UtilityBased`; NuGet resolves this domain package automatically.

## Included concepts

- `UtilityEvaluation<TAction>` records a candidate action and its finite utility score.
- `UtilityBasedDecision<TAction>` represents the selected action, its utility, or an explicit no-action outcome.
- `UtilityTieBreakPolicy` defines how equal maximum scores are handled: `FirstCandidate`, `LastCandidate`, or `RejectAmbiguous`.

The domain types are independent of model providers and do not execute the selected action.

## Installation

For the supported Fluent API, install the public agent package:

```bash
dotnet add package CSharp.AgentForge.Agents.UtilityBased
```

Install this `.Domain` package directly only when you intentionally compose lower-level decision contracts.

## Related packages

- [Utility-Based agent on NuGet](https://www.nuget.org/packages/CSharp.AgentForge.Agents.UtilityBased/)
- [AgentForge repository](https://github.com/peymanpro/AgentForge)
