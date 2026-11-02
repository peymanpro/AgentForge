# Workflow and Orchestration — Implementation Decision

**Status:** Implemented in the .NET MVP; public API remains experimental  
**Initial target:** .NET / C#  
**Implementation:** agentforge.net/src/AgentForge.Orchestration

## 1. Purpose

AgentForge provides optional workflow composition for independently implemented agents and application operations. The orchestrator coordinates execution but does not own the reasoning, model prompts, tool loops, memory strategy, or private algorithm of any step.

The MVP deliberately implements bounded sequential and conditional composition. It is not a distributed workflow engine.

## 2. Public API

The builder begins with AgentWorkflow.Create<TInput>(workflowId). AddStep registers an application operation. AddAgentStep adapts any IAgent<TInput,TOutput>. AddCondition registers an application-owned predicate. WorkflowStepCondition.WhenTrue and WhenFalse select conditional branches.

A step receives the workflow input and only the prior outputs explicitly named in its inputReferences argument.

## 3. Data flow and conditions

- Every step explicitly declares the IDs of prior step outputs it may access.
- WorkflowStepExecutionContext.GetOutput<T> rejects undeclared references and incompatible types.
- Conditions are registered by the application and declare their own prior-step references.
- A condition can be synchronous or asynchronous and is evaluated once per workflow run.
- Conditions are cached and reused by every step that references them.
- A step whose condition does not match is genuinely skipped before its own branch-only inputs are resolved.
- A downstream step that requires an output from a skipped branch is blocked because the required output does not exist.
- Conditional routing is application code. A model cannot provide executable branch code.

Outputs are held in an in-memory, per-run dictionary. The dictionary exposed to one step is read-only and includes only explicitly declared outputs. Output objects themselves are passed by reference; applications should favor immutable records and avoid mutating them after use.

## 4. Failure behavior

The default policy is stop-on-first-non-success:
- Condition false: Skipped; execution continues.
- Condition exception or unavailable condition input: current step fails or blocks and later steps are blocked.
- Missing output mapping: current step is blocked and later steps are blocked.
- Failed or rejected step: the workflow stops and preserves completed outputs.
- Partially completed nested agent: the workflow returns a partial result and blocks later steps.
- Cancellation and timeout: the active step receives the terminal state and later steps are blocked.
- Step-budget exhaustion: the next eligible step is marked BudgetExhausted and later steps are blocked.

No automatic retry or implicit error-handling branch is performed.

## 5. Budgets and child runs

WorkflowOptions defines default maximum executed steps, overall timeout, and per-step timeout. Per-run AgentRunOptions.MaxSteps and Timeout override the corresponding workflow defaults.

Nested agent steps receive child AgentRunContext values whose parent is the workflow run. The remaining per-step timeout is passed to the child. Model-request and tool-invocation limits are forwarded to each nested agent; these are per nested step, not a shared aggregate counter across the workflow.

Cancellation is cooperative. A timeout does not roll back external side effects already performed by a step that ignores cancellation.

## 6. Observability

The runtime emits non-sensitive events for workflow start, step start/completion/failure/skip, condition evaluation, and terminal outcomes. Step inputs and outputs are not included in default event attributes. Nested agent events use child run IDs linked to the workflow run.

## 7. Runtime limits

The MVP intentionally does not implement parallel execution, cycles, loops, automatic retries, dynamic graph mutation, durable checkpointing, pause/resume, process restart recovery, distributed scheduling, or transactional rollback. A workflow run is in-memory and sequential.

## 8. Validation evidence

The implementation includes deterministic tests for sequential data mapping, output isolation, conditional routing, branch skipping, unavailable branch outputs, failure handling, condition failure, step budgets, timeout, cancellation, nested-agent context, event privacy, and invalid workflow definitions.

A runnable example is provided at agentforge.net/samples/AgentForge.Orchestration.Sample.
