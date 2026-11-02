using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Classical;

var perceptRules = new[]
{
    new ReflexRule<ThermostatPercept, ThermostatAction>(
        "temperature-too-high",
        percept => percept.TemperatureCelsius > percept.TargetCelsius + 1,
        _ => ThermostatAction.Cool),
    new ReflexRule<ThermostatPercept, ThermostatAction>(
        "temperature-too-low",
        percept => percept.TemperatureCelsius < percept.TargetCelsius - 1,
        _ => ThermostatAction.Heat)
};

var simpleReflex = new SimpleReflexAgent<ThermostatPercept, ThermostatAction>(perceptRules);
var percept = new ThermostatPercept(TemperatureCelsius: 26, TargetCelsius: 22);
var simpleResult = await simpleReflex.RunAsync(
    percept,
    AgentRunContext.Create(),
    new AgentRunOptions());

Console.WriteLine($"Simple reflex: {Describe(simpleResult.Output?.Decision)}");

var modelRules = new[]
{
    new ReflexRule<ThermostatState, ThermostatAction>(
        "temperature-rising",
        state => state.CurrentTemperature > state.PreviousTemperature
            && state.CurrentTemperature > state.TargetTemperature,
        _ => ThermostatAction.Cool),
    new ReflexRule<ThermostatState, ThermostatAction>(
        "temperature-falling",
        state => state.CurrentTemperature < state.PreviousTemperature
            && state.CurrentTemperature < state.TargetTemperature,
        _ => ThermostatAction.Heat)
};

var modelBased = new ModelBasedReflexAgent<ThermostatPercept, ThermostatState, ThermostatAction>(
    stateUpdater: (previous, current) => new ThermostatState(
        PreviousTemperature: previous.CurrentTemperature,
        CurrentTemperature: current.TemperatureCelsius,
        TargetTemperature: current.TargetCelsius),
    rules: modelRules);

var modelResult = await modelBased.RunAsync(
    new ModelBasedReflexInput<ThermostatPercept, ThermostatState>(
        percept,
        new ThermostatState(PreviousTemperature: 24, CurrentTemperature: 24, TargetTemperature: 22)),
    AgentRunContext.Create(),
    new AgentRunOptions());

Console.WriteLine($"Model-based reflex: {Describe(modelResult.Output?.Decision)}");
Console.WriteLine($"Updated internal state: {modelResult.Output?.CurrentState}");

static string Describe(ClassicalDecision<ThermostatAction>? decision)
{
    if (decision is null || !decision.HasAction)
    {
        return "no action";
    }

    return decision.Action.ToString();
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
