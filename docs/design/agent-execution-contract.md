# Agent Execution Contract — Design Note

**Status:** Proposal; no public API is approved  
**Initial target:** .NET / C# on .NET 10 LTS  
**Cross-language scope:** Shared run semantics for all AgentForge implementations

## 1. Purpose

AgentForge agents need a common way to execute a task and report what happened. ReAct, Planning, Reflection, and future agents may have very different internal algorithms, but callers still need to start a run, pass a context, cancel it, enforce limits, observe progress, and receive a result.

Without a shared execution model, every agent would invent its own run identifiers, result shape, timeout behavior, failure categories, and diagnostics. That would make composition, testing, and cross-language conformance unnecessarily difficult.

This note proposes a small execution contract that standardizes the run lifecycle while leaving each agent's algorithm independent.

## 2. Design goals

The shared execution model should:

1. Support different input and output types.
2. Provide a consistent run lifecycle and terminal outcome.
3. Propagate cancellation.
4. Enforce explicit step, tool, model, time, and optional resource budgets.
5. Support nested agent runs and correlation.
6. Preserve partial results where an algorithm can safely provide them.
7. Distinguish invalid input, unsupported capability, policy denial, external failure, cancellation, timeout, and budget exhaustion.
8. Expose structured events without requiring callers to capture sensitive content.
9. Avoid turning the execution context into a service locator.
10. Remain idiomatic in every language.

## 3. A small execution contract

The agent contract should express only the operation that every runnable agent needs: execute an input using a run context and options, with cancellation.

A possible .NET-shaped sketch is:

```csharp
public interface IAgent<in TInput, TOutput>
{
    Task<AgentRunResult<TOutput>> RunAsync(
        TInput input,
        AgentRunContext context,
        AgentRunOptions options,
        CancellationToken cancellationToken = default);
}
```

This is illustrative. Exact variance, result types, default options, async abstraction, namespaces, and package placement are not approved.

The contract deliberately does not expose an agent's internal model calls, tools, prompts, plan representation, or evaluation strategy. Those belong to the specific agent implementation.

A workflow may invoke an agent through this public contract or through a language-native adapter that preserves its semantics.

## 4. Run lifecycle

Every run should have a recognizable lifecycle:

```text
        Create run context
                |
         Validate options/input
                |
           Run started
                |
        Execute agent algorithm
                |
      +---------+----------+
      |         |          |
   Complete   Partial    Terminal
      |       result     failure/stop
      +---------+----------+
                |
          Final result
                |
            Run ended
```

The exact intermediate events differ by agent. The shared lifecycle should define only events and outcomes that apply to all agents.

An agent must not report success if a required operation failed or a configured execution limit prevented the task from completing. When a meaningful partial result exists, it may be returned with an explicit non-success status.

## 5. Context versus options

Keep per-run identity/state separate from caller-selected limits.

### AgentRunContext

The context should carry information that identifies or correlates the current execution, such as:
- Run identifier.
- Optional parent run identifier for nested agent/workflow execution.
- Creation/start timestamp.
- Optional safe correlation metadata.
- A reference to the shared execution budget or accounting scope.

It should not automatically contain:
- Every application service.
- All registered tools.
- Credentials or secrets.
- Arbitrary mutable global state.
- Unrelated user or tenant data.

The context is not a service locator. Dependencies should be supplied to agents and adapters through normal dependency injection or explicit constructor arguments.

Application identity and authorization data are application-specific. The runtime must provide an explicit, secure way to pass required policy context without hard-coding one identity or authorization framework into the core contract.

### AgentRunOptions

Run options should contain caller-configurable execution constraints and safe behavior settings, such as:
- Maximum steps or iterations.
- Maximum model requests.
- Maximum tool invocations.
- Overall timeout or deadline.
- Optional token/cost budgets when reliable usage data is available.
- Partial-result policy where supported by the agent.

Agent-specific limits may extend the common options. For example, Reflection needs an attempt limit, while Planning needs a maximum plan size and step count.

The common options should not accumulate dozens of fields used by only one agent. Agent-specific configuration should remain in that agent's own options type.

## 6. Budget semantics

Budgets are limits, not suggestions.

The execution runtime should support a shared budget that can be consumed by nested operations. A child agent should not receive a fresh, unlimited budget when invoked from a workflow.

Candidate budget dimensions:

| Dimension | Meaning |
|---|---|
| Elapsed time | Wall-clock time available for the complete run |
| Steps | Number of agent/workflow execution steps |
| Model requests | Number of model calls |
| Tool invocations | Number of tool execution attempts |
| Input/output tokens | Usage limits when the provider reports reliable counts |
| Cost | Optional monetary budget when the provider exposes trustworthy price/usage data |

A limit should be checked before starting an operation where possible. If an external operation has already started when the deadline expires, cancellation does not guarantee that the external side effect has been rolled back.

Budget accounting should define whether a failed attempt consumes a unit. The safe default is that an attempted model or tool operation consumes its respective call budget, even if it fails.

Costs and token counts may be unknown. The runtime must not fabricate them or pretend that all providers report equivalent measurements.

## 7. Cancellation and timeouts

Cancellation is cooperative and must propagate to active model and tool operations.

The caller's cancellation token (or language-specific equivalent) should be combined with the run deadline and budget cancellation mechanism without losing the distinction between:
- Caller requested cancellation.
- Run timeout expired.
- An internal budget was exhausted.

An agent should stop starting new work after cancellation or terminal budget exhaustion.

A tool or provider adapter must document whether its cancellation can stop the underlying operation or merely stop waiting for the response.

## 8. Result semantics

The final result should communicate:
- Terminal status.
- Optional output.
- Normalized failure information, if applicable.
- Start/end time or duration.
- Safe usage metadata when available.
- Optional agent-specific partial state or progress summary.

Candidate cross-language terminal statuses:

| Status | Meaning |
|---|---|
| Completed | The agent reached its defined successful terminal condition |
| PartiallyCompleted | Useful output exists, but the requested operation did not fully complete |
| Failed | A non-recoverable failure occurred |
| Cancelled | The caller or parent execution requested cancellation |
| TimedOut | The run deadline expired |
| BudgetExhausted | A configured non-time resource limit was reached |
| Rejected | Input or configuration was rejected before meaningful execution |

Unsupported capability may be a specific failure category or a rejected outcome depending on when it is discovered. The specification should define the semantic distinction.

The exact C# result type may use a discriminated outcome, a status plus optional output/error, or another idiomatic form. Invalid combinations—such as Completed with a mandatory failure—must be prevented by constructors or validation.

## 9. Failure taxonomy

The shared semantics should distinguish at least:
- Invalid input or configuration.
- Unsupported model or runtime capability.
- Authentication/authorization failure.
- Policy denial.
- Model-provider failure.
- Tool failure.
- Retrieval failure.
- Plan or output validation failure.
- Evaluation failure.
- Cancellation.
- Timeout.
- Budget exhaustion.
- Unknown completion state after a possible external side effect.

Language-native exception types may differ. The normalized category and safe diagnostic meaning should be comparable.

Sensitive provider payloads, prompts, retrieved documents, tool arguments, results, and credentials must not be exposed in default exception messages or logs.

## 10. Execution events

A common event envelope may include:
- Event identifier and event type.
- Run identifier and optional parent run identifier.
- Step or operation identifier when relevant.
- Timestamp.
- Safe status and duration metadata.
- Optional structured, explicitly non-sensitive attributes.

Common lifecycle events may include:
- Run started.
- Run completed.
- Run partially completed.
- Run failed.
- Run cancelled.
- Run timed out.
- Budget exhausted.

Agent-specific events—such as tool requested, plan validated, candidate evaluated, or branch selected—should be defined by the relevant module and mapped to common run correlation identifiers.

The event model should not require storing private chain-of-thought or raw prompts. It should describe observable operations and state transitions.

### Event delivery

Event delivery is a design choice that needs a prototype. Options include:
- A caller-provided observer/sink.
- A stream returned alongside the run result.
- Integration with standard .NET diagnostics and OpenTelemetry.

The first design should avoid forcing an event-stream abstraction on every simple agent call. A non-configured observer should impose minimal overhead. Observer failures should not silently change the business outcome of the agent run by default.

## 11. Partial results

Partial results are agent-specific.

Examples:
- Planning may have completed two steps before a later step failed.
- Reflection may have a best candidate that did not meet acceptance criteria.
- Retrieval may have found sources but model generation may have failed.
- ReAct may have a useful observation but no final response.

The common execution result should allow an agent to return a typed partial output with PartiallyCompleted where appropriate. It must not invent a partial value when none exists.

Applications decide whether and how to display a partial result. The result must identify why it is partial.

## 12. Nested execution

When an agent invokes another agent or a workflow invokes an agent:
- The child run receives a child run identifier.
- The parent run identifier is preserved for correlation.
- Cancellation propagates from parent to child.
- The child consumes the shared budget or a bounded sub-budget.
- Tool and data permissions remain explicitly scoped.
- Child failures are represented in the parent result according to a documented composition policy.

A child should not silently reset the time or resource budget.

## 13. Testing strategy

Use deterministic fake collaborators and a controllable time source where required.

Minimum shared tests:

1. Valid input produces a Completed result.
2. Invalid input is rejected before execution.
3. A failure produces a normalized Failed result.
4. Cancellation propagates to the active collaborator.
5. Timeout is distinguishable from caller cancellation.
6. Step budget prevents the next step from starting.
7. Model/tool budgets are shared across nested operations.
8. Failed operation attempts consume the documented budget.
9. A partial result is returned only with PartiallyCompleted status.
10. Child runs preserve parent correlation.
11. Child runs cannot reset the overall budget.
12. Unsupported optional capability is represented explicitly.
13. Events have consistent run correlation.
14. Sensitive content is absent from default events and diagnostics.
15. Observer failure does not silently convert a successful agent operation into a business failure under the default policy.

Each agent still owns its algorithm-specific tests.

## 14. Alternatives considered

### One generic base class with protected hooks for every agent

**Not selected as the default.** It risks forcing ReAct, Planning, Reflection, and future agents into one inheritance lifecycle and can hide their algorithms behind hooks.

### Every agent defines its own run/result/cancellation model

**Rejected.** This makes orchestration, observability, composition, and cross-language conformance inconsistent.

### Small generic execution contract plus agent-specific options and behavior

**Recommended for prototyping.** It gives callers a common way to run agents while preserving their internal independence.

## 15. Open design questions

1. Should run status be a shared closed enum or an extensible tagged outcome?
2. Should partial output be generic on the common result, or represented by a separate partial-result union?
3. How should common and agent-specific budget types compose?
4. What is the minimal safe execution context across .NET, TypeScript, and Python?
5. Should event delivery begin with a callback/observer or a stream?
6. How should observer failures and backpressure be handled?
7. Should the common contract require timestamps, or should time be optional and adapter-supplied?
8. Which normalized failure categories belong in the shared specification?

## 16. Decision status

This note recommends a **small generic agent execution contract, separate context and options, shared bounded budgets, normalized terminal outcomes, and correlated safe execution events**.

It is a design proposal, not an approved public API. Prototype it with one deterministic ReAct run and nested workflow run before promoting the semantics into the shared specification.
