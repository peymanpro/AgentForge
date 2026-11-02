# AgentForge.Abstractions

Shared, provider-neutral contracts used by independently selectable AgentForge modules.

## Execution contracts

- `IAgent<TInput,TOutput>`: one bounded agent execution.
- `AgentRunContext`: run identity and optional parent/child correlation.
- `AgentRunOptions`: caller-selected budgets and optional event sink.
- `AgentRunResult<TOutput>`: normalized completed, partial, failed, rejected, cancelled, timed-out, and budget-exhausted outcomes.
- `AgentHandle<TInput,TOutput>`: concise public execution wrapper returned by fluent builders.

## Common use

Most users execute a configured agent with:

```csharp
var result = await agent.RunAsync(input, cancellationToken);
```

Advanced callers can provide options or a full context:

```csharp
var result = await agent.RunAsync(
    input,
    AgentRunContext.Create(parentRunId),
    new AgentRunOptions
    {
        MaxSteps = 10,
        Timeout = TimeSpan.FromSeconds(20),
        EventSink = eventSink
    },
    cancellationToken);
```

## Design boundary

This package intentionally contains no model-provider SDK, tool implementation, storage engine, vector database, or orchestration policy. Agent modules and optional capabilities depend on these contracts; the abstractions package does not depend on them.
