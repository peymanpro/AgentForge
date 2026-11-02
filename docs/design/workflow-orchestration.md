# Workflow and Orchestration — Design Note

**Status:** Proposal; implementation contract not yet approved  
**Initial target:** .NET / C#  
**Cross-language scope:** Shared observable behavior; idiomatic implementations

## 1. Purpose

AgentForge needs a way to compose independently implemented agents and capabilities into an application workflow.

An orchestrator coordinates execution. It should not contain the private reasoning logic of ReAct, Planning, Reflection, or any future agent. Agents should remain independently usable and testable; workflows should connect them through explicit inputs, outputs, and execution policies.

The MVP requires bounded sequential and conditional composition. It does not require a distributed workflow engine.

## 2. Design goals

The first orchestration layer should:

1. Compose agents and other typed operations through public contracts.
2. Support sequential execution.
3. Support explicit conditional branching.
4. Pass outputs through declared inputs rather than hidden shared mutable state.
5. Propagate cancellation and overall execution budgets.
6. Define error and partial-result behavior.
7. Expose execution events and step results.
8. Keep agent internals opaque to the orchestrator.
9. Avoid requiring a durable database, message broker, or distributed scheduler.
10. Remain implementable idiomatically in .NET, TypeScript, and Python.

## 3. Conceptual architecture

```text
                    Application
                        |
                 Workflow definition
                        |
                   Orchestrator
                        |
            +-----------+-----------+
            |           |           |
         Step A      Condition    Step B
            |           |           |
          Agent       Route        Agent
            |                       |
        Step result             Step result
            |                       |
            +-----------+-----------+
                        |
                  Workflow result
```

The orchestrator knows that a step has an input contract, an execution operation, and an output result. It does not know the internal model prompts, tool loop, memory strategy, or implementation classes of the step.

## 4. Workflow and step model

A workflow should contain:
- A stable workflow identifier.
- An explicitly ordered set of steps or a small directed graph.
- A declared input contract.
- A declared result contract.
- Execution options and limits.
- Failure and cancellation policies.

A step should contain:
- A stable identifier.
- A clear step type or operation reference.
- A declared input mapping.
- A declared output contract.
- Optional per-step limits.
- Optional condition or routing metadata where applicable.

The public API may be fluent, declarative, or builder-based in each language. The behavior should remain clear even when the syntax differs.

## 5. Initial composition patterns

### Sequential

Execute steps in declared order. A later step receives only the outputs explicitly mapped from prior steps and the permitted workflow context.

The runtime records each step outcome. A failed step follows the workflow's configured failure policy.

### Conditional routing

Evaluate a condition using an explicit workflow context or a typed result from an earlier step. Route to one of the declared branches.

Conditions should be application-defined or use a controlled expression model. Do not execute arbitrary model-generated code as a branch predicate.

The selected branch must be observable. A branch that is not selected should have an explicit skipped/not-selected state rather than appearing to have run.

### Agent composition

Any agent that implements the public execution contract may be used as a workflow step. This includes ReAct, Planning, and Reflection.

The orchestrator must not require a step to derive from one universal agent base class if a small execution contract is sufficient.

An agent step receives only the input, context, services, and capabilities intentionally assigned to it.

## 6. Data passing and context

Step-to-step data flow should be explicit.

Prefer declared output references and typed mappings over a mutable global dictionary shared by every step.

The workflow context may carry:
- Run and correlation identifiers.
- Cancellation and remaining budget.
- Application-provided principal or tenant context.
- Scoped services or dependencies.
- Safe metadata needed by the step.

The context should not automatically contain:
- Every application service.
- All registered tools.
- Credentials or secrets.
- Unrelated user data.
- The entire output history when a step requires only selected values.

The design should keep data ownership clear and allow step outputs to be inspected or redacted according to application policy.

## 7. Result model

A workflow result should preserve:
- Workflow identifier and terminal status.
- Per-step outcomes.
- Selected conditional branches.
- Outputs from completed steps.
- The first failure or blocking reason.
- Partial results when the workflow does not complete.
- Timing and usage metadata where available.

Candidate step outcomes include:
- Pending.
- Running.
- Completed.
- Failed.
- Skipped.
- Blocked.
- Cancelled.

Candidate workflow terminal statuses include:
- Completed.
- Partially completed.
- Failed.
- Cancelled.
- Timed out.
- Budget exhausted.

The exact status vocabulary should be shared only where it represents common observable behavior.

## 8. Failure policy

The MVP should support a conservative default:

- Stop execution after the first unhandled step failure.
- Preserve completed step results.
- Mark later steps as skipped or blocked.
- Return an explicit partial/failure result.

A workflow may define a local error-handling branch for expected business failures, but this must be explicit in the workflow definition.

The first implementation should not include implicit retries. Retries can duplicate side effects and should be owned by a documented step or application policy that understands idempotency.

## 9. Cancellation, budgets, and timeouts

The orchestrator should enforce an overall run budget and pass remaining limits to each step.

Candidate limits:
- Overall elapsed time.
- Maximum number of executed steps.
- Per-step timeout.
- Optional model/token/cost budget when reliable measurements are available.
- Optional limit on conditional route transitions.

Cancellation must propagate to the active step and prevent later steps from starting.

The runtime should define whether a step that ignores cancellation may continue externally. A workflow timeout cannot guarantee rollback of a side effect already performed by an external service.

## 10. State and persistence

The MVP workflow state is in-memory and scoped to one run.

It includes:
- Current step and branch.
- Completed step outcomes.
- Declared outputs needed by later steps.
- Run budget and terminal status.

Durable checkpointing, pause/resume across process restarts, distributed scheduling, external queues, and transactional rollback are deferred.

The absence of durable persistence must be documented. The MVP must not claim that an interrupted in-memory workflow can be resumed after process termination.

## 11. Security and trust boundaries

Workflow composition must preserve the security boundaries of its steps.

- A workflow definition does not automatically grant tools or permissions.
- Each agent/tool invocation remains subject to its own authorization and validation policy.
- Conditional routing must not be controlled by unvalidated model output without an explicit parser and policy.
- Step outputs are data, not trusted instructions.
- Secrets must be passed only through secure scoped dependencies, not serialized into general workflow state or diagnostic events.
- The orchestrator must not expose data from one step to another unless the workflow explicitly maps it.

## 12. Observability

The initial runtime should emit structured events such as:
- Workflow started.
- Step scheduled/started.
- Condition evaluated.
- Branch selected.
- Step completed/failed/skipped/blocked.
- Cancellation requested.
- Budget exhausted.
- Workflow completed/partially completed/failed.

Events should carry workflow and step correlation IDs, status, and safe duration metadata. Sensitive step inputs and outputs must not be logged by default.

## 13. Testing strategy

Use deterministic fake steps and conditions.

Minimum scenarios:

1. A two-step sequential workflow passes a declared output from the first step to the second.
2. A step cannot access an output that was not mapped to it.
3. A conditional workflow selects the expected branch.
4. A non-selected branch is marked as skipped.
5. An invalid or unknown route is rejected clearly.
6. A step failure stops later steps under the default policy.
7. Completed outputs remain available in the partial workflow result.
8. A configured failure-handling branch executes only for its declared failure condition.
9. Cancellation is propagated to the active step.
10. No later step starts after cancellation.
11. Overall timeout is enforced.
12. The maximum executed-step limit is enforced.
13. Nested agent execution preserves its own tool authorization policy.
14. A workflow does not silently retry a side-effecting step.
15. Events represent the actual execution sequence.
16. Sensitive inputs and outputs are absent from default logs.

## 14. API sketch

A possible composition shape is:

```csharp
var workflow = AgentWorkflow.Create()
    .AddStep("research", researchAgent)
    .AddStep("review", reflectionAgent)
    .AddCondition("needs_more_work", reviewResult => reviewResult.NeedsRevision)
    .AddStep("revise", revisionAgent)
    .Build();

var result = await workflow.RunAsync(
    "Prepare the requested research summary.",
    cancellationToken);
```

This is illustrative only. The example is not a proposed final syntax: route semantics, input mappings, step types, typed outputs, defaults, and package boundaries need a concrete prototype.

The public API should make the actual dependency graph explicit rather than relying on execution order hidden in method calls.

## 15. Alternatives considered

### A universal base class for every agent, tool, and workflow step

**Not selected as the default.** It risks coupling unrelated components and hiding type-specific inputs and outputs. A small execution contract plus adapters may be sufficient.

### A full general-purpose graph runtime in the MVP

**Deferred.** Durable state, graph mutation, parallel execution, retries, interrupts, and resumption substantially expand the scope. The MVP needs bounded sequential and conditional composition first.

### Directly invoke agents from application code without a workflow abstraction

**Still valid for simple applications.** AgentForge should not force a workflow engine on developers who only need one agent. Orchestration is an optional compositional capability.

### Bounded sequential and conditional workflow composition

**Recommended for the MVP.** It delivers useful composition while keeping semantics, state, and failure behavior understandable.

## 16. Open design questions

1. Should the first workflow graph be represented as a linear builder with explicit conditional blocks, or as a small directed graph definition?
2. How should strongly typed step outputs be mapped across heterogeneous agent types?
3. Should the execution contract be based on a generic typed result or a normalized result envelope?
4. How should a step expose safe scoped services and tools without turning workflow context into a service locator?
5. Which failure conditions should be routable as business outcomes, and which should always terminate the workflow?
6. How should nested agent budgets consume the overall workflow budget?
7. Which workflow events belong in the shared cross-language conformance suite?

## 17. Decision status

This note recommends **optional, bounded workflow composition through explicit step contracts, data mappings, sequential execution, and conditional routing**.

It is a proposal, not an approved API or implementation claim. Validate it with a two-step sequential workflow and a conditional branch using deterministic fake agents before promoting its semantics into the shared specification.
