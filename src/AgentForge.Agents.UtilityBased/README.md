# AgentForge.Agents.UtilityBased

A standalone Utility-Based agent module that evaluates available candidate actions and proposes the action with the greatest utility score.

## Installation

Install this package from NuGet.org:

```bash
dotnet add package CSharp.AgentForge.Agents.UtilityBased
```

## Quick Start

The candidate provider defines the actions available for the current percept. The utility evaluator assigns a score to each candidate. The agent evaluates every candidate once and selects the highest-scoring action.

```csharp
using AgentForge.Agents.UtilityBased.Builders;
using AgentForge.Agents.UtilityBased.Domain.Policies;

var agent = UtilityBasedAgentBuilder
    .Create<ThermostatPercept, ThermostatAction>()
    .WithCandidates(_ => new[]
    {
        ThermostatAction.Cool,
        ThermostatAction.Heat,
        ThermostatAction.Maintain
    })
    .WithUtility((percept, action) => action switch
    {
        ThermostatAction.Cool =>
            percept.TemperatureCelsius - percept.TargetCelsius,
        ThermostatAction.Heat =>
            percept.TargetCelsius - percept.TemperatureCelsius,
        ThermostatAction.Maintain =>
            -Math.Abs(percept.TemperatureCelsius - percept.TargetCelsius),
        _ => throw new ArgumentOutOfRangeException(nameof(action))
    })
    .WithTieBreakPolicy(UtilityTieBreakPolicy.FirstCandidate)
    .Build();

var result = await agent.RunAsync(
    new ThermostatPercept(29, 22),
    cancellationToken);

if (result.Output?.Decision.HasAction == true)
{
    Console.WriteLine(result.Output.Decision.Action);
    Console.WriteLine(result.Output.Decision.Utility);
}

public sealed record ThermostatPercept(double TemperatureCelsius, double TargetCelsius);

public enum ThermostatAction
{
    Cool,
    Heat,
    Maintain
}
```

## Decision model

For a percept (p), let (A(p)) be the finite candidate set and (U(p,a)) the utility assigned to candidate (a). The agent selects:

```text
arg max U(p, a), for a in A(p)
```

The utility function is supplied by the application. The module does not impose a weighted-sum formula, probability model, or optimization technique.

## Tie-breaking

The default policy is `FirstCandidate`. Configure another policy when the domain requires different behavior:

- `FirstCandidate`: keep the earliest candidate among equal maximum scores.
- `LastCandidate`: keep the latest candidate among equal maximum scores.
- `RejectAmbiguous`: return a successful no-action decision when multiple candidates share the maximum score.

Candidate order is preserved. Duplicate action values are not silently removed.

## Behavior and guarantees

- Every candidate is evaluated exactly once, in the order supplied.
- Negative and zero utility scores are valid.
- Utility scores must be finite; NaN and positive or negative infinity fail the run.
- An empty candidate set returns a completed no-action result.
- The output includes the selected action, its utility, and an immutable list of all evaluations.
- The agent proposes an action; the host remains responsible for executing side effects.
- Cancellation is checked before evaluation and between candidate evaluations. A synchronous user-supplied delegate cannot be forcibly interrupted while it is running.
- Candidate and utility delegates are application-owned. They should be deterministic and safe for concurrent calls if the configured agent is shared across concurrent runs.
- No model provider, tool registry, database, or infrastructure adapter is required.

## Architecture

- **Domain:** decision, utility evaluation, and tie-breaking policy.
- **Application:** candidate evaluation and action-selection use case.
- **Public API:** fluent builder and common execution handle.
- **Infrastructure:** not required by this module.

## Limitations

The initial implementation performs a deterministic, sequential, single-step utility comparison. It does not learn utility functions, simulate probabilistic outcomes, execute actions, perform parallel evaluation, or implement multi-objective/Pareto optimization. These concerns can be composed as separate strategies or capabilities later.