# AgentForge.Agents.ModelBasedReflex

A standalone, provider-independent Model-Based Reflex agent module.

## Fluent API

```csharp
var agent = ModelBasedReflexAgentBuilder
    .Create<ThermostatPercept, ThermostatState, ThermostatAction>()
    .WithStateUpdater((previous, percept) => new ThermostatState(
        PreviousTemperature: previous.CurrentTemperature,
        CurrentTemperature: percept.TemperatureCelsius,
        TargetTemperature: percept.TargetCelsius))
    .When(
        "temperature-rising-above-target",
        state => state.CurrentTemperature > state.PreviousTemperature
            && state.CurrentTemperature > state.TargetTemperature,
        ThermostatAction.Cool)
    .When(
        "temperature-falling-below-target",
        state => state.CurrentTemperature < state.PreviousTemperature
            && state.CurrentTemperature < state.TargetTemperature,
        ThermostatAction.Heat)
    .Build();

var result = await agent.RunAsync(
    new ModelBasedReflexInput<ThermostatPercept, ThermostatState>(percept, previousState),
    cancellationToken);
```

The state update and decision are explicit. The agent instance does not retain mutable state between runs; the host controls state persistence and lifetime.

## Module boundaries

- **Domain:** model-based input, rule, decision, and output types.
- **Application:** state-update and decision use case implementing the shared agent contract.
- **Infrastructure:** intentionally absent; the host supplies the state updater and owns state persistence.
- **Public API / composition root:** the fluent builder validates configuration and composes the application use case.

The module has no dependency on an LLM, provider SDK, tools, database, or dependency-injection container.

## Behavior and limits

- The state updater runs before rule evaluation.
- Rules are evaluated in declaration order; the first match wins.
- No match is a successful no-action result that still returns the updated state.
- The selected action is a proposal. The host applies it to the environment.
- Cancellation and run options use the shared execution contract.
