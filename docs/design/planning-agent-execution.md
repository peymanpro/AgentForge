# Planning / Plan-and-Execute Agent — Design Note

**Status:** Proposal; implementation contract not yet approved  
**Initial target:** .NET / C#  
**Cross-language scope:** Shared observable behavior; idiomatic implementations

## 1. Purpose

The Planning / Plan-and-Execute agent is intended for tasks that benefit from being decomposed into explicit steps before execution.

Unlike a simple prompt that asks a model to "think step by step", this component must produce a structured plan, validate it, execute its steps through known capabilities, track progress, and return a result that accounts for completed and failed work.

The plan is an execution artifact, not merely explanatory text.

## 2. Scope

The MVP planning agent should:
- Accept a task and bounded execution options.
- Generate a structured plan through a supplied planning strategy.
- Validate the plan before executing it.
- Execute approved steps through explicit step handlers or composed agents.
- Track step states and results.
- Enforce plan-size, step, time, and resource budgets.
- Report partial completion and failure clearly.
- Support cancellation.
- Keep plan generation separate from step execution.

The MVP should begin with ordered, sequential steps. Dependency graphs, dynamic parallelism, long-running persistence, and complex replanning are later extensions.

## 3. Conceptual lifecycle

```text
             User task + constraints
                       |
                       v
                  Plan strategy
                       |
                       v
               Structured plan
                       |
                       v
                 Plan validator
                       |
             +---------+---------+
             |                   |
           Invalid              Valid
             |                   |
             v                   v
          Fail run          Initialize step state
                                 |
                                 v
                           Execute next step
                                 |
                          +------+------+
                          |             |
                       Success        Failure
                          |             |
                          v             v
                    Record result   Apply failure policy
                          |             |
                          +------+------+
                                 |
                         More steps left?
                                 |
                           +-----+-----+
                           |           |
                          Yes          No
                           |           |
                           v           v
                      Next step   Aggregate outcome
                                       |
                                       v
                                    Complete
```

## 4. Plan representation

The initial plan representation should be structured and language-neutral in meaning.

A plan should contain:
- A short overall objective.
- An ordered collection of steps.
- A stable step identifier.
- A concise step objective.
- Optional input references to prior step outputs.
- An expected outcome or completion criterion where useful.
- Optional step metadata that is safe and meaningful for orchestration.

The plan should not contain arbitrary executable code, raw shell commands, or unrestricted instructions that bypass the tool and authorization system.

The first implementation should not require a general dependency graph. The order of steps is the execution order. More expressive dependency structures can be added after the sequential contract is tested.

## 5. Plan generation

Plan generation should be a replaceable strategy. It may use:
- A model with structured-output capability.
- A model tool/function-call capability.
- A separately validated prompt-and-parse strategy.

The strategy receives the task, application instructions, permitted capabilities, and planning constraints. It returns a normalized plan candidate.

A plan generator must not execute steps, invoke tools, or grant permissions. Its output is untrusted and must pass validation.

A model-generated plan should be constrained by:
- Maximum number of steps.
- Maximum field sizes and nesting depth.
- Allowed references or step types.
- Application-provided task constraints.
- The set of capabilities available to the plan executor.

## 6. Plan validation

Validation occurs before execution.

The validator should check:
- The plan contains at least one step unless an explicit no-op plan is supported.
- Step identifiers are unique and valid.
- The step count does not exceed the configured limit.
- Required fields are present and within length/size bounds.
- Referenced prior outputs exist and refer only to permitted earlier steps.
- Step types or handlers are recognized.
- The plan does not contain unsupported execution instructions.
- Any declared constraints are internally consistent.

A plan that fails validation must not partially execute. The result should identify the validation category without leaking sensitive task content.

## 7. Step execution model

The plan executor should not understand the internals of every agent or tool.

A step should be dispatched through a public execution contract. Candidate implementations include:
- A typed step handler registered for a known step type.
- An existing agent invoked with a step objective and the permitted context.
- A deterministic application-defined operation.

For the initial implementation, composing an existing ReAct agent as a step executor is one useful option. It should remain optional rather than becoming a hidden dependency of the planning agent.

Step handlers receive only the context and capabilities assigned to them. They must not gain the entire application tool registry automatically.

Each step should have an outcome such as:
- Pending.
- Running.
- Completed.
- Failed.
- Blocked.
- Skipped.
- Cancelled.

The public model may represent these states using idiomatic enums or tagged unions in each language.

## 8. Failure policy

The initial agent should support an explicit failure policy, at minimum:

- **Stop on failure:** stop before executing later steps and return a partial plan result.
- **Continue independent steps:** only if the planner or application can prove that remaining steps do not depend on the failed step.

The second policy should be deferred unless dependencies are represented and validated. The safe MVP default is stop on failure.

A failed step must not be silently marked complete. Any partial output should be preserved as a partial result and clearly labeled.

## 9. Replanning

Dynamic replanning is attractive but introduces additional decisions:
- Whether a failed step should be retried or replaced.
- Whether completed steps may be revised.
- How new plans are validated against original constraints.
- How plan versions and step identity are tracked.
- How replan attempts consume model and execution budgets.
- How side effects from prior steps are handled.

For the first implementation:
- No automatic replan should occur by default.
- A configurable bounded replan strategy may be considered after the initial executor is validated.
- Replanning must never bypass normal tool authorization or repeat side-effecting operations without an explicit idempotency policy.

## 10. Budgets and cancellation

Planning needs explicit limits for:
- Plan-generation attempts.
- Maximum steps in a plan.
- Executed steps.
- Overall elapsed time.
- Optional model usage/cost.
- Optional per-step timeout.

A single overall execution context should propagate cancellation to the planner, step handlers, composed agents, and tools.

A budget exhausted during execution should return an explicit partial outcome. It should not be reported as a successful completed plan.

## 11. Result semantics

The final result should preserve:
- The validated plan.
- Per-step status and safe diagnostic information.
- Outputs from completed steps.
- The first failure or blocking reason where applicable.
- Whether the plan completed, partially completed, failed validation, was cancelled, or exhausted a budget.
- Overall usage and timing when available.

The final response may include a concise summary, but the structured execution result is the source of truth for callers.

## 12. State ownership

The planning agent owns the lifecycle of one plan execution:
- Task and planning constraints.
- Validated plan.
- Step state and execution order.
- Step outputs and references.
- Budgets and counters.
- Terminal status.

Persistent storage, cross-run memory, distributed execution, and durable recovery are separate capabilities and not part of the first planning implementation.

## 13. Observability

The implementation should emit structured events such as:
- Plan generation started/completed.
- Plan validation passed/failed.
- Step started.
- Step completed/failed/blocked/skipped.
- Failure policy applied.
- Budget exhausted.
- Plan completed/partially completed/cancelled/failed.

Events should identify plan and step correlation without storing the full private task, prompt, or output by default.

## 14. Testing strategy

Use deterministic fake planning strategies and fake step handlers.

Minimum tests:

1. A valid plan with one step executes and completes.
2. A multi-step sequential plan executes in order.
3. Invalid empty plan is rejected before any step runs.
4. Duplicate step identifiers are rejected.
5. Unknown step handler is rejected before execution or marked according to a documented validation rule.
6. Maximum plan step count is enforced.
7. Invalid output reference is rejected.
8. Completed step output is passed to a later step through an explicit reference.
9. Failure under stop-on-failure produces a partial result and stops later steps.
10. Cancellation during plan generation is propagated.
11. Cancellation during a step is propagated.
12. Overall and per-step time budgets are enforced.
13. Plan generation does not itself execute tools.
14. ReAct composition uses only the explicitly provided tool scope.
15. No automatic replan occurs unless the configured strategy explicitly enables it.
16. Events reflect the exact observable step transitions.

## 15. API sketch

A possible use shape is:

```csharp
var planner = PlanAndExecuteAgent.Create()
    .WithPlanningStrategy(planningStrategy)
    .WithStepExecutor(stepExecutor)
    .WithMaxSteps(12)
    .WithFailurePolicy(PlanFailurePolicy.StopOnFailure)
    .Build();

var result = await planner.RunAsync(
    "Research the requested topic and summarize the evidence.",
    cancellationToken);
```

This is illustrative only. Names, builder methods, result types, default policies, and package boundaries are not approved.

## 16. Alternatives considered

### Ask the model to produce a numbered list and execute it as prose

**Rejected.** Prose does not provide stable step identity, dependency references, validation, status tracking, or deterministic execution semantics.

### Let the planning model directly call any tool

**Rejected as the core planning design.** It collapses planning and execution, bypasses the plan validation boundary, and makes partial progress difficult to reason about.

### Generate a structured plan, validate, and execute through explicit handlers

**Recommended for prototyping.** This keeps planning, policy, and execution responsibilities separate and allows the plan to be tested independently of model quality.

## 17. Open design questions

1. What is the minimum plan-step schema required for the first use cases?
2. Should a step be a generic objective executed by an agent, or a discriminated operation with an explicit step type?
3. How are outputs referenced safely without passing arbitrary serialized application state?
4. Should the ContinueIndependentSteps policy be deferred entirely until a dependency graph exists?
5. Which evaluation or verification mechanism determines that a step is complete?
6. Should the first version include a single bounded replan option, or defer replanning completely?
7. How should plan and step budgets compose with nested agent and tool budgets?

## 18. Decision status

This note recommends a **structured, validated, sequential Plan-and-Execute lifecycle with explicit step handlers and stop-on-failure as the safe default**.

It is a proposal, not an approved API or implementation claim. Validate it with a deterministic planner and handlers before promoting it into the shared specification.
