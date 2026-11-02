# AgentForge.Agents.SimpleReflex

A deterministic agent that selects an action from the current percept using ordered condition-action rules.

## Quick Start

Add a project reference:

```bash
dotnet add path/to/YourApp.csproj reference src/AgentForge.Agents.SimpleReflex/AgentForge.Agents.SimpleReflex.csproj
```

Define the percept and action types, then build and run the agent:

```csharp
using AgentForge.Agents.SimpleReflex.Builders;

var agent = SimpleReflexAgentBuilder
    .Create<ThermostatPercept, ThermostatAction>()
    .When(
        "too-hot",
        percept => percept.TemperatureCelsius > percept.TargetCelsius + 1,
        ThermostatAction.Cool)
    .When(
        "too-cold",
        percept => percept.TemperatureCelsius < percept.TargetCelsius - 1,
        ThermostatAction.Heat)
    .Build();

var result = await agent.RunAsync(
    new ThermostatPercept(29, 22),
    cancellationToken);

var output = result.Output;
if (output?.Decision.HasAction == true)
{
    Console.WriteLine(output.Decision.Action);
}

public sealed record ThermostatPercept(double TemperatureCelsius, double TargetCelsius);

public enum ThermostatAction
{
    Cool,
    Heat
}
```

## Behavior

- Rules run in declaration order; the first match wins.
- No matching rule returns a successful no-action decision.
- The agent proposes an action; the host performs the side effect.
- No model, provider, tool registry, or database is required.
