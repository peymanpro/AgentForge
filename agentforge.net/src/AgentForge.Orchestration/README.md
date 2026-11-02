# AgentForge.Orchestration

This is the workflow orchestration library for AgentForge .NET.

It composes independent agents and application operations into a bounded sequential workflow with explicit output mappings and application-owned conditional routing.

## Problem it solves

Applications often need to run more than one agent: research, review, revise, and publish, for example. Calling each agent manually leaves data flow, cancellation, budgets, branch decisions, and partial failures scattered through application code. The orchestrator centralizes those workflow concerns without taking ownership of each agent's internal algorithm.

## Quick start

    var workflow = AgentWorkflow.Create<ResearchTask>("research-review")
        .AddAgentStep("research", researchAgent, context => context.WorkflowInput)
        .AddAgentStep(
            "review",
            reviewAgent,
            context => context.GetOutput<ResearchDraft>("research"),
            ["research"])
        .AddCondition(
            "needs_revision",
            context => context.GetOutput<ReviewResult>("review").NeedsRevision,
            ["review"])
        .AddAgentStep(
            "revise",
            revisionAgent,
            context => new RevisionRequest(
                context.GetOutput<ResearchDraft>("research"),
                context.GetOutput<ReviewResult>("review")),
            ["research", "review"],
            WorkflowStepCondition.WhenTrue("needs_revision"))
        .Build();

    var result = await workflow.RunAsync(
        request,
        AgentRunContext.Create(),
        new AgentRunOptions());

The example uses application-defined agent implementations. AgentForge does not select a model provider or hide the implementation behind a universal agent base class.

## Execution model

1. Steps execute in declaration order.
2. Each step declares the exact IDs of prior outputs it may read.
3. Conditions are application-owned delegates and can inspect only their declared prior outputs.
4. A condition is evaluated once per run and its result is reused by every step that references it.
5. A step whose condition does not match is recorded as Skipped.
6. A step that depends on an output that was not produced is recorded as Blocked.
7. By default, a failed, rejected, timed-out, or partially completed step stops later execution.
8. Completed outputs are retained in the workflow result.

Conditional expressions are ordinary application code. Model output is never compiled or executed as a branch predicate.

## Limits and cancellation

- WorkflowOptions defines finite default workflow, per-step, and executed-step limits.
- AgentRunOptions.Timeout and AgentRunOptions.MaxSteps override the corresponding workflow defaults.
- Cancellation is propagated to the active operation or nested agent.
- Nested agents receive child run contexts linked to the workflow run.
- Model-request and tool-invocation limits are forwarded to each nested agent step. They are per nested step, not an aggregate workflow-wide counter.

A timeout cannot roll back an external side effect already performed by an operation that ignores cancellation.

## Results and observability

WorkflowRunOutput preserves ordered step results, condition decisions, child run IDs, and completed outputs. Step states include Completed, PartiallyCompleted, Failed, Skipped, Blocked, Cancelled, TimedOut, BudgetExhausted, and Rejected.

The runtime emits structured lifecycle events and does not log step inputs or outputs by default.

## Deliberate MVP boundaries

This library is an in-memory sequential and conditional orchestrator. It does not implement parallel execution, cycles, loops, automatic retries, dynamic graph mutation, durable checkpoints, process restart recovery, distributed scheduling, or transactional rollback.

Step outputs are application objects passed by reference. Prefer immutable output records and avoid mutating an output after publishing it to the workflow.

## Tests and sample

- Tests: tests/AgentForge.Orchestration.Tests
- Runnable deterministic sample: samples/AgentForge.Orchestration.Sample

From the agentforge.net directory, run:

    dotnet run --project samples/AgentForge.Orchestration.Sample
