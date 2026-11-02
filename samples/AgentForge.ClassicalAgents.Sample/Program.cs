using AgentForge.Agents.ModelBasedReflex.Builders;
using AgentForge.Agents.ModelBasedReflex.Domain.Decisions;
using AgentForge.Agents.SimpleReflex.Builders;

var percept = new ThermostatPercept(TemperatureCelsius: 26, TargetCelsius: 22);

var simpleReflexAgent = SimpleReflexAgentBuilder
    .Create<ThermostatPercept, ThermostatAction>()
    .When(
        "temperature-too-high",
        current => current.TemperatureCelsius > current.TargetCelsius + 1,
        ThermostatAction.Cool)
    .When(
        "temperature-too-low",
        current => current.TemperatureCelsius < current.TargetCelsius - 1,
        ThermostatAction.Heat)
    .Build();

var simpleResult = await simpleReflexAgent.RunAsync(percept);
Console.WriteLine($"Simple Reflex: {Describe(simpleResult.Output?.Decision.HasAction, simpleResult.Output?.Decision.Action)}");

var modelBasedReflexAgent = ModelBasedReflexAgentBuilder
    .Create<ThermostatPercept, ThermostatState, ThermostatAction>()
    .WithStateUpdater((previous, current) => new ThermostatState(
        PreviousTemperature: previous.CurrentTemperature,
        CurrentTemperature: current.TemperatureCelsius,
        TargetTemperature: current.TargetCelsius))
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

var modelBasedResult = await modelBasedReflexAgent.RunAsync(
    new ModelBasedReflexInput<ThermostatPercept, ThermostatState>(
        percept,
        new ThermostatState(
            PreviousTemperature: 24,
            CurrentTemperature: 24,
            TargetTemperature: 22)));

Console.WriteLine($"Model-Based Reflex: {Describe(modelBasedResult.Output?.Decision.HasAction, modelBasedResult.Output?.Decision.Action)}");
Console.WriteLine($"Updated state: {modelBasedResult.Output?.CurrentState}");

static string Describe<TAction>(bool? hasAction, TAction? action)
{
    return hasAction == true ? action?.ToString() ?? "action" : "no action";
}

internal sealed record ThermostatPercept(int TemperatureCelsius, int TargetCelsius);

internal sealed record ThermostatState(
    int PreviousTemperature,
    int CurrentTemperature,
    int TargetTemperature);

internal enum ThermostatAction
{
    Heat,
    Cool
}
