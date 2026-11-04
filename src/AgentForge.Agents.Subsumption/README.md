# AgentForge.Agents.Subsumption

A standalone layered reactive agent based on the Subsumption Architecture. Independent behaviors are registered as layers with priorities. The highest-priority active layer selects the action and suppresses lower-priority layers for that decision cycle.

## Installation

After publication to NuGet.org:

```bash
dotnet add package CSharp.AgentForge.Agents.Subsumption
```

## Quick Start

```csharp
using AgentForge.Agents.Subsumption.Builders;
using AgentForge.Agents.Subsumption.Models;

var agent = SubsumptionAgentBuilder.Create<RobotPercept, RobotAction>()
    .WithLayer(
        "emergency-stop",
        priority: 100,
        percept => percept.ObstacleDistanceMeters < 0.5,
        _ => RobotAction.Stop)
    .WithLayer(
        "obstacle-avoidance",
        priority: 50,
        percept => percept.ObstacleDistanceMeters < 2,
        _ => RobotAction.TurnAway)
    .WithLayer(
        "wander",
        priority: 10,
        _ => true,
        _ => RobotAction.MoveForward)
    .Build();

var result = await agent.RunAsync(
    new SubsumptionAgentInput<RobotPercept>(new RobotPercept(0.3)),
    cancellationToken);

Console.WriteLine(result.Output?.Action);

public sealed record RobotPercept(double ObstacleDistanceMeters);

public enum RobotAction
{
    Stop,
    TurnAway,
    MoveForward
}
```

## Arbitration semantics

- Layers are evaluated in descending priority order.
- Ties preserve registration order.
- The first active layer supplies the action; lower layers are not evaluated.
- No active layer returns a successful no-action result.
- The module evaluates one decision cycle; the host supplies the next percept on the next run.
- State, sensors, scheduling, motor control, and physical side effects remain host responsibilities.

## How it differs from Simple Reflex

Simple Reflex is an ordered list of condition-action rules within one rule set. Subsumption explicitly models multiple behavior layers with priorities and suppression: a safety layer can override navigation, and navigation can override a default behavior without sharing one flat rule list.

## Fluent configuration

- `WithLayer(name, priority, isActive, actionFactory)` registers a behavior layer.
- `WithTimeProvider(provider)` configures run timestamps.
- `Build()` validates the layer catalog and returns the common `AgentHandle`.

The layer predicates and action factories should be deterministic, bounded, and free of external side effects.