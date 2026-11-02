# AgentForge.Agents.ModelBasedReflex

A deterministic agent that updates an explicit internal state from the previous state and current percept before selecting an action.

## Quick Start

Add a project reference:

\`\`\`bash
dotnet add path/to/YourApp.csproj reference src/AgentForge.Agents.ModelBasedReflex/AgentForge.Agents.ModelBasedReflex.csproj
\`\`\`

Define the percept, state, and action types. Supply the state transition and ordered rules:

\`\`\`csharp
using AgentForge.Agents.ModelBasedReflex.Builders;
using AgentForge.Agents.ModelBasedReflex.Domain.Decisions;

var agent = ModelBasedReflexAgentBuilder
    .Create<ThermostatPercept, ThermostatState, ThermostatAction>()
    .WithStateUpdater((previous, percept) => new ThermostatState(
        PreviousTemperature: previous.CurrentTemperature,
        CurrentTemperature: percept.TemperatureCelsius,
        TargetTemperature: percept.TargetCelsius))
    .When(
        "rising-above-target",
        state => state.CurrentTemperature > state.PreviousTemperature
            && state.CurrentTemperature > state.TargetTemperature,
        ThermostatAction.Cool)
    .When(
        "falling-below-target",
        state => state.CurrentTemperature < state.PreviousTemperature
            && state.CurrentTemperature < state.TargetTemperature,
        ThermostatAction.Heat)
    .Build();

var result = await agent.RunAsync(
    new ModelBasedReflexInput<ThermostatPercept, ThermostatState>(
        new ThermostatPercept(29, 22),
        new ThermostatState(27, 28, 22)),
    cancellationToken);

if (result.Output is { } output)
{
    Console.WriteLine(output.CurrentState);
    Console.WriteLine(output.Decision.Action);
}

public sealed record ThermostatPercept(double TemperatureCelsius, double TargetCelsius);

public sealed record ThermostatState(
    double PreviousTemperature,
    double CurrentTemperature,
    double TargetTemperature);

public enum ThermostatAction
{
    Cool,
    Heat
}
\`\`\`

## Behavior

- The state updater runs before rule evaluation.
- Rules run in declaration order; the first match wins.
- The host owns state persistence and supplies the previous state to each run.
- No matching rule returns the updated state with a no-action decision.
- No model, provider, or database is required.
