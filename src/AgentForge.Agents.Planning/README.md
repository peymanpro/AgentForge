# AgentForge.Agents.Planning

A structured Plan-and-Execute agent that validates a generated plan before running its registered step handlers.

## Installation

Install this package from NuGet.org:

```bash
dotnet add package CSharp.AgentForge.Agents.Planning
```

## Quick Start

Register an application-specific `IPlanningStrategy` and only the `IPlanStepHandler` implementations that the agent is allowed to execute.

```csharp
using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Planning;
using AgentForge.Agents.Planning.Builders;
using AgentForge.Agents.Planning.Execution;
using AgentForge.Agents.Planning.Planning;
using Microsoft.Extensions.DependencyInjection;

var agent = PlanAndExecuteAgentBuilder.Create()
    .WithPlanner(services.GetRequiredService<IPlanningStrategy>())
    .WithStepHandlers(services.GetServices<IPlanStepHandler>())
    .WithMaxPlanSteps(8)
    .WithTimeout(TimeSpan.FromSeconds(60))
    .Build();

var result = await agent.RunAsync(
    "Find the customer account and calculate the renewal date.",
    cancellationToken);

Console.WriteLine($"Completed steps: {result.Output?.CompletedStepCount}");
```

The planner and step handlers are application-owned dependencies. Register only handlers permitted for this workload. The builder creates an immutable handler registry and validates that a planning strategy is supplied.

## Behavior

- The complete plan is validated before any handler executes.
- Step types must exist in the immutable handler registry.
- Steps execute sequentially and receive only explicitly referenced prior outputs.
- The first failed step stops the run; later steps are marked blocked.
- Automatic replanning, retries, parallel execution, and durable state are not included.
