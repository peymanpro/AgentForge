# AgentForge.Agents.Planning

An independently implemented structured Plan-and-Execute agent for AgentForge .NET.

## Algorithm

1. IPlanningStrategy generates an ExecutionPlan candidate from the task and registered step types.
2. PlanValidator checks the full plan before any step handler runs.
3. Validation enforces bounded plan size, valid and unique identifiers, registered handler types, required objectives, and backward-only references.
4. PlanStepHandlerRegistry resolves each declared step type to an application-supplied IPlanStepHandler.
5. Steps execute sequentially. Each handler receives only the prior outputs explicitly referenced by its PlanStep.
6. The first failed step stops execution; completed outputs are preserved and later steps are marked Blocked.

## Safety and limits

Generated plan content is untrusted. It cannot invent a handler: step types must be present in the immutable registry. References cannot point forward or to the current step.

The run is bounded by maximum plan size, executed-step limits, and an overall timeout. Cancellation is propagated. No automatic replan, retry, arbitrary command execution, parallel execution, or durable state is included.

## Extension points

The planner and step handlers are supplied by the application. A handler may wrap an agent, a deterministic operation, or a domain service while retaining its own authorization policy.
