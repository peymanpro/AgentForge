# Fluent API Design

Fluent construction is the primary developer experience of AgentForge. A developer should be able to install one agent package, configure that agent with a short strongly typed chain, build it, and run it.

## 1. The intended developer journey

```text
Choose an agent
    -> Install its NuGet package
    -> Configure with its module-specific Fluent API
    -> Build()
    -> RunAsync(input)
```

For example, a deterministic Learning Agent requires only its own module:

```bash
dotnet add package AgentForge.Agents.Learning
```

```csharp
using AgentForge.Agents.Learning.Builders;

var agent = LearningAgentBuilder.Create<string>()
    .WithEpsilonGreedy(epsilon: 0.1)
    .Build();

var result = await agent.RunAsync(input, cancellationToken);
```

The installation command becomes available after the package is published. Examples must always match the actual public API of the referenced module; proposed APIs must be labeled as illustrative.

## 2. Module-specific builders, shared experience

Every agent module owns its builder and exposes only options meaningful to that architecture.

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

### Goal-Based

```csharp
var agent = GoalBasedAgentBuilder
    .Create<State, Goal, Action>()
    .WithSuccessors(GetSuccessors)
    .WithGoalPredicate((state, goal) => IsSatisfied(state, goal))
    .WithLimits(maxDepth: 20, maxExploredStates: 5_000)
    .Build();
```

### Utility-Based

```csharp
var agent = UtilityBasedAgentBuilder
    .Create<Percept, Action>()
    .WithCandidates(GetCandidates)
    .WithUtility((percept, action) => Evaluate(percept, action))
    .Build();
```

### Learning

```csharp
var agent = LearningAgentBuilder.Create<string>()
    .WithEpsilonGreedy(epsilon: 0.1)
    .Build();
```

### ReAct

```csharp
var agent = ReActAgentBuilder.Create()
    .WithDecisionStrategy(decisionStrategy)
    .WithTools(approvedTools)
    .WithAuthorization(authorizationPolicy)
    .WithSystemPrompt("Use approved tools when useful.")
    .WithMaxModelTurns(8)
    .WithMaxToolInvocations(12)
    .Build();
```

ReAct, Plan-and-Execute, and Reflection now expose module-specific builders.

### Plan-and-Execute

```csharp
var agent = PlanAndExecuteAgentBuilder.Create()
    .WithPlanner(planner)
    .WithStepHandlers(approvedHandlers)
    .WithMaxPlanSteps(8)
    .WithTimeout(TimeSpan.FromSeconds(60))
    .Build();
```

### Reflection

```csharp
var agent = ReflectionAgentBuilder.Create()
    .WithGenerator(generator)
    .WithEvaluator(evaluator)
    .WithMaxAttempts(3)
    .WithStopOnNoImprovement()
    .Build();
```

## 3. Consistent construction and execution

Where the concept is genuinely shared, builders should use a common vocabulary:

- `Create<T...>()` or `Create()`: start configuration.
- `WithName(...)`, `WithDescription(...)`: optional metadata.
- `WithTimeout(...)`, `WithMaxSteps(...)`, `WithBudget(...)`: relevant execution limits.
- `WithTimeProvider(...)`: deterministic time control where timestamps are used.
- `Build()`: validate and create the configured immutable agent.

A built agent exposes the common handle:

```csharp
var result = await agent.RunAsync(input, cancellationToken);
```

Advanced hosts may pass explicit run context and options. The common execution surface does not mean every agent supports every capability. Streaming, sessions, tools, memory, and model access remain opt-in contracts.

## 4. Do not build a universal mega-builder

AgentForge MUST NOT expose one builder with unrelated switches for every architecture.

- A deterministic reflex agent should not expose `WithModel`.
- A ReAct agent should expose model/tool configuration because they are central to its behavior.
- A Goal-Based agent should expose successor, goal, and search-limit configuration.
- A Learning Agent should expose its learning policy and relevant learning parameters.

Shared builder infrastructure may validate common metadata or create the common handle, but module-specific builders own their public vocabulary.

## 5. Build-time validation and immutability

`Build()` should reject missing required strategies, predicates, or adapters; duplicate identifiers or invalid names; incompatible capabilities; invalid limits and numerical parameters; and invalid references between steps, tools, or policies.

After Build, configuration must not be mutable through the builder. The built agent should own a defensive snapshot of the configuration and should be safe for concurrent runs unless the module explicitly documents otherwise.

## 6. No service locator and no hidden effects

A builder MUST NOT silently resolve arbitrary services from global state. An explicit dependency-injection extension may be provided as an alternative composition path.

A builder MUST NOT perform remote model calls, tool invocations, data writes, or expensive external initialization during Build. Such work begins only when the user runs the built agent or explicitly invokes a setup operation.

## 7. Package independence

The public module package is the consumer-facing unit. Its shared abstractions and required internal projects are resolved transitively by NuGet.

Installing one agent MUST NOT install unrelated agent modules. Optional providers, tools, memory, retrieval, evaluation, and observability packages are installed only when selected by the application.

A mandatory `AgentForge` meta-package that depends on every agent is explicitly out of scope. An optional convenience bundle may be considered later without changing the modular default.

## 8. Fluent API completion gate

An agent module is not considered developer-ready until it has a module-specific builder with a concise happy path; strongly typed configuration methods; build-time validation and immutable built configuration; the common `AgentHandle<TInput,TOutput>` execution surface; tests for valid/invalid builder configuration and execution; a runnable sample and package README; and CI verification of build, tests, package contents, and README.
