# AgentForge.Abstractions

Shared execution contracts used by AgentForge .NET modules.

## Responsibilities

- IAgent<TInput,TOutput> defines one asynchronous agent run without prescribing its algorithm.
- AgentRunContext provides run identity, correlation, and parent-child relationships.
- AgentRunOptions carries caller-supplied limits and an optional event sink.
- AgentRunResult<TOutput> normalizes completed, partially completed, and stopped outcomes.
- AgentFailure and AgentFailureCategory provide provider-neutral failure information.
- AgentRunEventPublisher emits safe lifecycle metadata and isolates observer failures from business execution.

## Lifecycle

An agent receives its input, an AgentRunContext, AgentRunOptions, and a CancellationToken. It returns a normalized AgentRunResult. Implementations own their internal model, tool, memory, or retrieval behavior.

The shared contract deliberately does not require every agent to share a base class or a universal mutable context.

## Failure and limits

Terminal outcomes distinguish Completed, PartiallyCompleted, Failed, Cancelled, TimedOut, BudgetExhausted, and Rejected. A stopped run must carry a normalized failure; useful partial output may be preserved.

Cancellation is cooperative. A limit only protects external side effects when the implementation and its dependencies honor cancellation.

## Tests

The test project verifies context identity, result invariants, option validation, and event metadata behavior.
