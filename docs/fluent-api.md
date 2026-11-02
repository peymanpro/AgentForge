# Fluent API Design

The fluent API is the primary construction experience for AgentForge. It should expose the essential parameters of an agent in a readable chain and hide repetitive object wiring without hiding the architecture.

## 1. Design goals

- Short happy path.
- Explicit required dependencies.
- Discoverable, typed configuration methods.
- Compile-time type safety wherever possible.
- Configuration errors detected during `Build()`, before a run starts.
- Immutable configuration after building.
- No implicit network calls or external side effects during building.
- Optional advanced controls without burdening ordinary usage.
- One consistent run experience across all agent modules.

## 2. Common execution experience

A built agent is exposed as an `AgentHandle<TInput,TOutput>`:

```csharp
var result = await agent.RunAsync(input, cancellationToken);
```

Advanced hosts can pass run budgets, event sinks, and correlation context:

```csharp
var result = await agent.RunAsync(
    input,
    context: AgentRunContext.Create(parentRunId),
    options: new AgentRunOptions
    {
        MaxSteps = 12,
        MaxModelRequests = 8,
        MaxToolInvocations = 10,
        Timeout = TimeSpan.FromSeconds(30),
        EventSink = eventSink
    },
    cancellationToken);
```

The simple overload creates a run context and default options. It does not remove the advanced execution contract.

## 3. Module-specific builders

Each architecture has its own builder. The builder surface reflects the concepts needed by that architecture.

### Simple Reflex

```csharp
var agent = SimpleReflexAgentBuilder
    .Create<Percept, Action>()
    .When("high", percept => percept.Value > 10, Action.Reduce)
    .When("low", percept => percept.Value < 5, Action.Increase)
    .Build();
```

### Model-Based Reflex

```csharp
var agent = ModelBasedReflexAgentBuilder
    .Create<Percept, WorldState, Action>()
    .WithStateUpdater((previous, percept) => Update(previous, percept))
    .When("blocked", state => state.IsBlocked, Action.Replan)
    .Build();
```

### ReAct

The intended API shape is:

```csharp
var agent = ReActAgentBuilder
    .Create()
    .WithModel(chatModel)
    .WithSystemPrompt("You are a support agent.")
    .WithTools(tools => tools
        .Add<FindOrderTool>()
        .Add<RefundRequestTool>())
    .WithAuthorization(toolPolicy)
    .WithMaxModelTurns(8)
    .WithMaxToolInvocations(12)
    .WithTimeout(TimeSpan.FromSeconds(45))
    .Build();

var result = await agent.RunAsync("Where is order 123?", cancellationToken);
```

The exact tool-registration API is implementation-dependent; authorization remains an application-controlled requirement.

### Planning

```csharp
var agent = PlanAndExecuteAgentBuilder
    .Create()
    .WithPlanner(planner)
    .WithStepHandler<LookupCustomerHandler>()
    .WithStepHandler<CalculateEligibilityHandler>()
    .WithMaxPlanSteps(8)
    .Build();
```

### Multi-agent coordination

```csharp
var team = SupervisorTeamBuilder
    .Create()
    .WithSupervisor(supervisor)
    .AddWorker("research", researchAgent)
    .AddWorker("writer", writingAgent)
    .AddWorker("reviewer", reviewAgent)
    .WithMaxDelegations(10)
    .Build();

var result = await team.RunAsync(task, cancellationToken);
```

These snippets define target API direction. They are not claims that every builder currently exists.

## 4. Common method vocabulary

Use the same names where the concept is genuinely common:

- `Create<T...>()` or `Create()`: begin configuration.
- `WithName`, `WithDescription`: optional metadata.
- `WithModel`, `WithTools`, `WithMemory`, `WithRetrieval`: optional capabilities when relevant.
- `WithTimeout`, `WithMaxSteps`, `WithBudget`: resource controls when relevant.
- `UseMiddleware`: cross-cutting pipeline behavior when the module supports it.
- `WithTimeProvider`: deterministic time control.
- `Build()`: validate and create the immutable agent.

Agent-specific methods should remain specific. A deterministic reflex agent does not need `WithModel`; a model-driven ReAct agent does not need `WithStateUpdater`.

## 5. Build-time validation

`Build()` should reject:
- Missing required strategies or adapters.
- Duplicate IDs and names.
- Incompatible capabilities.
- Invalid limits.
- Invalid references between steps or policies.
- Configurations that can only fail deterministically before execution.

Runtime errors from providers, tools, stores, and user delegates are normalized during execution.

## 6. No service locator and no hidden effects

A builder MUST NOT silently resolve arbitrary services from global state. It may offer an explicit DI extension as an alternative composition mechanism.

A builder MUST NOT perform remote model calls, tool invocations, data writes, or expensive initialization during `Build()`. Such work begins only when the user runs the built agent or explicitly invokes a setup operation.

## 7. Streaming, sessions, and advanced execution

Not every agent is conversational or streaming. These are optional contracts, not mandatory methods on every agent.

The architecture should eventually support:
- Streaming run updates.
- Explicit multi-turn sessions.
- Session persistence and restore.
- Pause/resume and human approval.
- Nested agent execution with aggregate budgets.

The basic `RunAsync` API remains available for single-run architectures.
