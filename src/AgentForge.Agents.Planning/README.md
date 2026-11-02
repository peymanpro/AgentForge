# AgentForge.Agents.Planning

A structured Plan-and-Execute agent that validates a generated plan before running its registered step handlers.

## Quick Start

Register an application-specific `IPlanningStrategy` and only the `IPlanStepHandler` implementations that the agent is allowed to execute.

```csharp
using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Planning;
using AgentForge.Agents.Planning.Execution;
using AgentForge.Agents.Planning.Planning;
using Microsoft.Extensions.DependencyInjection;

var handlers = new PlanStepHandlerRegistry(
    services.GetServices<IPlanStepHandler>());

var agent = new AgentHandle<string, PlanAndExecuteOutput>(
    new PlanAndExecuteAgent(
        services.GetRequiredService<IPlanningStrategy>(),
        handlers,
        new PlanAndExecuteAgentOptions(
            maxPlanSteps: 8,
            timeout: TimeSpan.FromSeconds(60))));

var result = await agent.RunAsync(
    "Find the customer account and calculate the renewal date.",
    cancellationToken);

Console.WriteLine($"Completed steps: {result.Output?.CompletedStepCount}");
```

The planner and step handlers are application-owned dependencies. Register only handlers permitted for this workload.

## Behavior

- The complete plan is validated before any handler executes.
- Step types must exist in the immutable handler registry.
- Steps execute sequentially and receive only explicitly referenced prior outputs.
- The first failed step stops the run; later steps are marked blocked.
- Automatic replanning, retries, parallel execution, and durable state are not included.
