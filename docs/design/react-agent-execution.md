# ReAct Agent — Execution Design Note

**Status:** Proposal; implementation contract not yet approved  
**Initial target:** .NET / C#  
**Cross-language scope:** Shared observable behavior; idiomatic implementations

## 1. Purpose

This document defines the proposed execution behavior of the first ReAct agent in AgentForge.

ReAct combines reasoning-oriented model interaction with actions and observations. In an application, the important behavior is an explicit control loop: obtain a model decision, validate and execute an allowed action when requested, return the observation to the model, and continue until a defined stopping condition is reached.

The objective is not to expose private model reasoning. AgentForge should make the execution algorithm inspectable through state transitions, action requests, tool outcomes, events, and tests.

## 2. Scope

The MVP ReAct agent should:
- Accept a task and bounded execution options.
- Use a supplied model integration through an explicit decision strategy.
- Receive a scoped collection of tools.
- Validate and authorize every requested tool invocation.
- Return tool observations to the model.
- Stop deterministically when the task is complete or a run limit is reached.
- Propagate cancellation and distinguish failures.
- Emit structured execution events without logging sensitive content by default.

This design does not include durable workflow persistence, distributed execution, parallel tool execution, browser automation, or unrestricted code execution.

## 3. Conceptual execution cycle

```text
           User task + execution context
                       |
                       v
             Build model interaction
                       |
                       v
               Request next decision
                       |
                       v
             Normalize model response
                       |
             +---------+----------+
             |                    |
          Final answer         Tool action(s)
             |                    |
             v                    v
       Complete run        Validate tool request
                                  |
                            Authorize / approve
                                  |
                            Execute within limits
                                  |
                            Normalize observation
                                  |
                            Append observation
                                  |
                            Continue loop
```

Every arrow should correspond to a testable transition. Model-specific wire formats must be normalized before the agent loop handles the result.

## 4. Inputs and configuration

A ReAct run needs the following conceptual inputs:

- **Task:** the user's objective or instruction for this run.
- **Instructions:** application-controlled behavior and safety instructions.
- **Model decision strategy:** the mechanism used to obtain a final answer or action request.
- **Available tools:** the explicitly scoped tool set.
- **Execution context:** correlation, application-provided scoped services, and relevant run metadata.
- **Budget:** limits for model turns, tool calls, elapsed time, and optionally tokens or cost.
- **Cancellation:** the language/runtime cancellation mechanism.

The public API may provide a builder or options object, but the implementation must keep these concepts explicit and independently testable.

## 5. Model decision strategies

The ReAct algorithm should not assume that every provider offers identical native tool calling.

The agent should use an explicit decision strategy that converts a model interaction into one of two normalized outcomes:

1. **Final response** — the model indicates that it has completed the current task.
2. **Action request** — the model requests one or more named tools with arguments.

Potential strategies include:
- Native provider tool calling through an optional model capability.
- Structured-output-based action selection.
- A carefully validated prompt-and-parse strategy for models without the preceding capabilities.

The first implementation should begin with one strategy that has strong tests. Additional strategies can be added as independent implementations. The agent must report an unsupported capability clearly rather than silently switching to an unsafe or unreliable fallback.

## 6. Normalized decision model

The agent loop should not parse provider-specific JSON or SDK objects directly.

A normalized decision should represent:
- Final response content.
- One or more tool-call proposals, including call identifiers where available.
- A provider refusal or non-action terminal outcome where the provider exposes one.
- A malformed or unsupported decision outcome.

A tool-call proposal is untrusted data. The agent must resolve the name against its current tool scope, validate arguments, and enforce application policy before invocation.

The model may return explanatory content alongside an action request. The runtime should not require or expose hidden chain-of-thought. Only the normalized action, required message content, and permitted diagnostic metadata should be retained.

## 7. Detailed loop semantics

### Initialization

1. Validate the task and run options.
2. Resolve the configured model decision strategy.
3. Build the tool scope for this run.
4. Initialize bounded execution state.
5. Emit a run-start event.

### Model turn

1. Assemble the normalized messages required by the current step.
2. Include only the tools available to the current run.
3. Invoke the selected decision strategy with cancellation.
4. Normalize the response.
5. Record a model-turn event with safe metadata such as duration, usage when available, and normalized outcome.

### Final response

When the normalized outcome is a final response:
1. Validate any required output constraints.
2. Record the final result and status.
3. Emit a run-completed event.
4. Return without executing additional tools.

### Tool action

For each requested tool action:
1. Check that the tool exists in the run's allowed tool scope.
2. Validate the arguments against the canonical tool contract.
3. Invoke the application-controlled authorization policy.
4. Request human approval when required and supported by the runtime.
5. Check remaining tool-call, time, and resource budgets.
6. Execute through the tool executor with cancellation.
7. Normalize the result or failure into an observation.
8. Emit a tool-execution event.
9. Append the observation to the agent's current interaction state.

The model's request never bypasses a step in this sequence.

### Continue

After the tool observation is appended, the agent may request another model decision if the run has remaining budget.

The next model turn should receive the relevant interaction history, including the tool call and its observation, but should not automatically receive unrelated application state or secrets.

## 8. Multiple tool calls

Some providers may return multiple tool-call proposals in one model turn.

For the first implementation:
- The runtime should validate every proposal before invoking it.
- Tool execution should be serial by default.
- Each result should be correlated with its original tool-call identifier where available.
- A denial or failure for one tool must not be disguised as success.
- The policy for continuing after one tool fails must be explicit.

Parallel execution is deferred until concurrency limits, ordering, partial failure, cancellation, and side-effect semantics are designed and tested.

## 9. Termination conditions

A ReAct run must always be bounded.

Candidate terminal outcomes:

| Outcome | Meaning |
|---|---|
| Completed | A final response was produced and accepted |
| Cancelled | The caller cancelled the run |
| Timed out | The run-level time budget expired |
| Model turn limit reached | No further model decision is allowed |
| Tool-call limit reached | No further tool invocation is allowed |
| Resource budget exhausted | A configured token, cost, or other budget was reached |
| Failed | A non-recoverable model, policy, tool, or runtime failure occurred |
| Unsupported capability | The required model decision capability is unavailable |

The result type should preserve the terminal status and relevant diagnostic details. Reaching a limit must not be represented as a normal completed answer unless the application explicitly chooses a separate partial-result policy.

### Counting limits

The design should distinguish:
- **Model turns:** calls made to obtain a decision.
- **Tool calls:** individual tool invocations.
- **Elapsed time:** total wall-clock duration.
- **Optional usage budget:** tokens or cost, when reliable provider information is available.

A single ambiguous `MaxIterations` option may be provided for convenience, but the semantics of what it counts must be documented. Separate budgets are safer for the execution core.

## 10. Failure and recovery behavior

The agent should distinguish:
- Invalid run configuration.
- Model request or provider failure.
- Malformed model decision.
- Unknown or disallowed tool.
- Invalid tool arguments.
- Authorization denial.
- Approval rejection or unavailability.
- Tool execution failure.
- Cancellation or timeout.
- Budget exhaustion.

A recoverable tool failure may be returned to the model as a sanitized observation when the configured policy allows it. A security denial must not be transformed into an instruction to try another privileged path.

Retries must be bounded and explicit. The agent should not automatically repeat a side-effecting tool call when its completion outcome is unknown.

## 11. State ownership

The ReAct agent owns the state required for one run:
- Original task and instructions.
- Normalized interaction messages.
- Tool-call and observation correlations.
- Current budgets and counters.
- Terminal status and final result.

Long-term memory, cross-run user profiles, durable checkpointing, and retrieval stores are separate capabilities. They should not become implicit global state inside the ReAct agent.

The run should be reproducible in tests when supplied with deterministic model and tool fakes.

## 12. Observability

The agent should emit normalized events such as:

- Run started.
- Model turn started/completed.
- Decision normalized.
- Tool request proposed.
- Tool request rejected.
- Authorization decision made.
- Approval requested/resolved.
- Tool execution started/completed/failed.
- Budget limit reached.
- Run completed/cancelled/failed.

Events should include correlation identifiers and safe timing/status information. Prompt text, hidden reasoning, tool arguments, tool output, and secrets must not be logged by default.

The execution event model should be sufficient to reconstruct the visible control flow without storing sensitive content.

## 13. Testing strategy

Use a deterministic scripted model fake that returns a known sequence of decisions. Use fake tools with observable invocation counts and configurable success/failure/cancellation behavior.

Minimum scenarios:

1. Model returns a final answer without requesting a tool.
2. Model requests one valid tool; the result is returned as an observation; model then completes.
3. Model requests an unknown tool.
4. Model requests a tool outside the allowed scope.
5. Model supplies invalid tool arguments.
6. Authorization denies a valid tool request.
7. Approval is required and then approved.
8. Approval is rejected or unavailable.
9. Tool returns a business-level error result.
10. Tool execution throws or returns a normalized failure.
11. Model returns a malformed or unsupported decision.
12. Model turn limit is reached.
13. Tool-call limit is reached.
14. Cancellation occurs during a model call.
15. Cancellation occurs during tool execution.
16. Run timeout expires.
17. The run does not execute a tool twice due to an ambiguous automatic retry.
18. Default events and logs do not expose protected prompt or tool data.

Tests should verify both the final result and the exact sequence of observable state transitions.

## 14. API sketch

The public API should remain small. A possible usage shape is:

```csharp
var agent = ReActAgent.Create()
    .WithModelDecisionStrategy(decisionStrategy)
    .WithTools(toolSet)
    .WithExecutionLimits(new ReActExecutionLimits
    {
        MaxModelTurns = 8,
        MaxToolCalls = 12,
        Timeout = TimeSpan.FromSeconds(45)
    })
    .Build();

var result = await agent.RunAsync(
    "Find the current status of the requested order.",
    cancellationToken);
```

This is a conceptual example. Names, builder design, defaults, and package boundaries are not approved.

The API should not require a particular model vendor, web browser, memory provider, or vector database.

## 15. Alternatives considered

### Ask the model for unrestricted reasoning text

**Rejected.** The runtime needs an explicit, validated action representation. Requiring private reasoning traces is unnecessary for implementing and testing the observable ReAct control loop.

### Let the provider SDK own the entire agent loop

**Not selected as the core design.** Provider SDKs may offer useful high-level agent abstractions, but AgentForge's ReAct implementation should own the documented loop and policy boundary so its behavior remains inspectable and testable.

### Use one universal agent base class with many strategy switches

**Not selected as the default.** Excessive generic configuration could hide the actual algorithm. Shared runtime utilities are useful, but the ReAct decision and execution loop should remain explicit.

## 16. Open design questions

1. What is the minimum normalized decision schema needed for final responses, native tool calls, structured action output, and refusals?
2. Should the first implementation use native provider tool calling only, or include a structured-output decision strategy from the start?
3. How should model-call and tool-call budgets interact when a model returns several tool proposals in one turn?
4. What is the exact policy for continuing after one of several requested tools is denied or fails?
5. Should a ReAct run support partial results when a budget is exhausted?
6. Which events belong in the cross-language conformance contract?
7. How should the runtime represent provider refusal separately from ordinary final completion?

## 17. Decision status

This note recommends an **explicit, bounded ReAct control loop with normalized model decisions and policy-controlled tool execution**.

It is a design proposal, not an approved API or claim of implementation. Prototype the loop with a deterministic fake model and tools, then validate the observable behavior against the shared specification and the idioms of .NET, TypeScript, and Python.
