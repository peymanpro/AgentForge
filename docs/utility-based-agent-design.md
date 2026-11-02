# Utility-Based Agent — Design Specification

**Status:** Proposed implementation design  
**Target module:** `AgentForge.Agents.UtilityBased`  
**Target framework:** .NET 10  
**Package model:** One independently consumable NuGet package, with separate Domain and Application projects following the existing Simple Reflex module pattern.

## 1. Purpose

A Utility-Based Agent selects an action by comparing the expected desirability of available actions. Unlike a Simple Reflex Agent, which selects the first action whose rule matches, this architecture evaluates the candidate actions and chooses the one with the highest utility score.

The module owns the decision procedure. The application supplies:
- a candidate-action provider for the current percept;
- a utility function that scores each candidate in the context of that percept;
- an explicit tie policy.

The agent proposes an action. It does not perform environment side effects.

## 2. Decision model

For a percept (p), let (A(p)) be the finite ordered set of candidate actions and let (U(p,a)) be the utility assigned to action (a).

The selected action is:

[
a^* = \operatorname*{arg\,max}_{a \in A(p)} U(p,a)
]

The first implementation deliberately treats the utility function as an application-supplied deterministic function. It does not prescribe weighted sums, probability models, multi-objective optimization, or a particular way of estimating future outcomes. Those are separate strategies and can be added later without changing the agent's core control loop.

## 3. Scope and non-goals

### In scope

- Generate a candidate action set for one percept.
- Evaluate every candidate exactly once per run.
- Select the candidate with the greatest finite utility score.
- Apply an explicit and testable tie policy.
- Return the selected action, its score, and decision metadata.
- Return a valid no-action outcome when the candidate set is empty.
- Respect cancellation and the shared execution result contract.
- Keep the agent immutable after construction and safe for concurrent runs, assuming supplied delegates are safe for concurrent use.

### Out of scope for the first version

- Executing actions or mutating the environment.
- Learning or updating the utility function.
- LLM/provider integration.
- Persistent memory or state.
- Built-in weighted-sum or Pareto optimization.
- Probabilistic outcome simulation.
- Parallel candidate evaluation.
- Multi-agent negotiation or coordination.

## 4. Proposed module structure

```text
src/
  AgentForge.Agents.UtilityBased.Domain/
    AgentForge.Agents.UtilityBased.Domain.csproj
    Decisions/
      UtilityBasedDecision.cs
    Evaluation/
      UtilityEvaluation.cs
    Policies/
      UtilityTieBreakPolicy.cs

  AgentForge.Agents.UtilityBased.Application/
    AgentForge.Agents.UtilityBased.Application.csproj
    UseCases/
      UtilityBasedAgent.cs
      UtilityBasedOutput.cs

  AgentForge.Agents.UtilityBased/
    AgentForge.Agents.UtilityBased.csproj
    Builders/
      UtilityBasedAgentBuilder.cs
    README.md

tests/
  AgentForge.Agents.UtilityBased.Tests/
```

Do not create an Infrastructure project: the initial architecture has no external I/O requirement.

## 5. Public API direction

The intended consumer API is fluent and module-specific:

```csharp
var agent = UtilityBasedAgentBuilder
    .Create<ThermostatPercept, ThermostatAction>()
    .WithCandidates(percept => new[]
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
```

This is an API design target, not an existing API. The implementation must not document it as available until the builder and tests exist.

## 6. Domain rules and invariants

1. The candidate provider and utility evaluator are required.
2. The candidate sequence must not be null and must not contain null actions.
3. Candidate actions are materialized once before evaluation so one-shot or lazy enumerables cannot change during selection.
4. An empty candidate set produces a successful no-action decision, not a failed run. An ambiguous maximum under `RejectAmbiguous` also produces a completed no-action decision while preserving all evaluations.
5. Every candidate is evaluated once, in candidate order.
6. Utility scores may be negative or zero. They must be finite; NaN and positive/negative infinity are invalid evaluation results.
7. The greatest utility wins.
8. Equal maximum scores are resolved by the configured tie policy:
   - `FirstCandidate`: keep the earliest candidate;
   - `LastCandidate`: keep the latest candidate among tied maxima;
   - `RejectAmbiguous`: return a completed no-action decision with an explicit ambiguity reason; the run itself is not an execution failure.
9. Duplicate candidate values are not silently removed: candidate identity and ordering belong to the supplied candidate provider. If duplicate actions are undesirable for a use case, the provider must avoid them.
10. The agent returns a proposal only; the host decides whether and how to execute it.

## 7. Failure and execution behavior

- Invalid builder configuration fails at `Build()`.
- Invalid shared run options use the same normalized `Rejected` result convention as existing agents.
- Cancellation before or during evaluation returns `Cancelled`.
- A null candidate collection, null candidate, thrown provider/evaluator exception, or non-finite utility score returns a normalized `Failed` result without leaking exception details.
- An empty candidate list is a normal completed run with no action.
- The shared `AgentRunContext`, `AgentRunOptions`, and `AgentRunResult<T>` contracts remain unchanged.

## 8. Testing requirements

The module is not complete until tests cover:

- highest-utility candidate selection;
- negative and zero utility values;
- first-candidate and last-candidate tie resolution;
- ambiguous tie rejection;
- empty candidate set;
- candidate provider invoked once;
- each candidate evaluated exactly once and in order;
- invalid/null configuration;
- null candidate sequence and null candidate entries;
- NaN and infinite scores;
- provider and evaluator exceptions;
- cancellation before evaluation and during evaluation;
- invalid shared run options;
- concurrent runs do not share mutable decision state;
- public builder returns the common `AgentHandle<TInput,TOutput>`.

## 9. Integration checklist

When implementation is ready, update all of the following in the same feature branch:

- Add Domain, Application, public API, and test projects to `AgentForge.sln`.
- Add project references in the permitted inward dependency direction.
- Add the module README with an executable Quick Start and accurate limitations.
- Add the module to the root README contents, architecture/module table, and NuGet package table.
- Update `docs/agent-catalog.md` from Planned to Implemented (experimental) only after tests pass.
- Update `docs/agent-modules.md` only if the generic module guidance needs a real clarification; do not add duplicate module-specific detail there.
- Run restore, Release build, and the complete test suite before opening a pull request.

## 10. Acceptance criteria

The module is acceptable when it implements the decision model above, has no dependency on other concrete agent modules, exposes only the public builder/handle surface needed by consumers, passes the complete test suite, and documents only behavior that is present in code.
