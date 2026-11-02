# AgentForge.Agents.SimpleReflex

A standalone, provider-independent Simple Reflex agent module.

## Fluent API

```csharp
var agent = SimpleReflexAgentBuilder
    .Create<ThermostatPercept, ThermostatAction>()
    .When(
        "temperature-too-high",
        percept => percept.TemperatureCelsius > percept.TargetCelsius + 1,
        ThermostatAction.Cool)
    .When(
        "temperature-too-low",
        percept => percept.TemperatureCelsius < percept.TargetCelsius - 1,
        ThermostatAction.Heat)
    .Build();

var result = await agent.RunAsync(percept, cancellationToken);
```

Rules are evaluated in declaration order. The first match wins. No match produces a successful result with `Decision.HasAction == false`.

## Module boundaries

- **Domain:** condition-action rules and decision/output types.
- **Application:** one bounded decision use case implementing the shared agent execution contract.
- **Infrastructure:** intentionally absent. A simple-reflex decision has no external I/O dependency; adding an empty infrastructure project would add no value.
- **Public API / composition root:** the fluent builder validates configuration and composes the application use case.

The module depends only on AgentForge's execution abstractions. It does not require an LLM, provider SDK, tool registry, database, or dependency-injection container.

## Behavior and limits

- Actions are proposed, not executed. The host remains responsible for applying side effects to its environment.
- Rule ordering is explicit and deterministic.
- Duplicate rule names are rejected.
- Cancellation and run options use the shared execution contract.
- Exceptions from rule delegates are converted to a normalized internal failure; exception contents are not exposed.
