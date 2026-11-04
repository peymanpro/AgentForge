# AgentForge

**A modular .NET 10 agent framework for building, configuring, and running different agent architectures through concise Fluent APIs.**

Install only the agent module you need. Each agent is an independently versioned NuGet package with its own focused Fluent API; shared abstractions arrive transitively, while unrelated agents and optional capabilities stay out of your dependency graph. This README contains the documentation and Quick Start for every current agent. Each NuGet module also ships its own package README.

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/)
[![NuGet packages](https://img.shields.io/badge/NuGet-Browse%20packages-004880?logo=nuget)](https://www.nuget.org/packages?q=CSharp.AgentForge)
[![CI](https://github.com/peymanpro/AgentForge/actions/workflows/agentforge-ci.yml/badge.svg?branch=main)](https://github.com/peymanpro/AgentForge/actions/workflows/agentforge-ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

> **Status:** Published on NuGet.org (v1.0.0). Public APIs may evolve as the framework develops.

## Contents

- [Why AgentForge](#why-agentforge)
- [Install only what you need](#install-only-what-you-need)
- [Fluent construction model](#fluent-construction-model)
- [Agent Quick Starts](#agent-quick-starts)
  - [Simple Reflex](#simple-reflex-quick-start)
  - [Model-Based Reflex](#model-based-reflex-quick-start)
  - [ReAct](#react-quick-start)
  - [Plan-and-Execute](#plan-and-execute-quick-start)
  - [Reflection](#reflection-quick-start)
  - [Utility-Based](#utility-based-quick-start)
  - [Goal-Based](#goal-based-quick-start)
  - [Learning](#learning-quick-start)
  - [Rule-Based / Production System](#rule-based-production-system-quick-start)
  - [Subsumption](#subsumption-quick-start)
  - [Contextual LinUCB](#contextual-linucb-quick-start)
- [NuGet packages](#nuget-packages)
- [Architecture](#architecture)
- [Learning benchmark protocol](#learning-benchmark-protocol)
- [Benchmarks](#benchmarks)
- [Development](#development)
- [License](#license)

## Why AgentForge

Different problems require different decision architectures. A deterministic rule-driven agent, a stateful controller, a tool-using LLM agent, and an iterative refinement agent should not be forced into one generic loop.

AgentForge separates four concerns:

- **Agents:** independent, architecture-specific modules.
- **Algorithms:** reusable search, planning, optimization, and learning methods.
- **Capabilities:** optional models, tools, memory, retrieval, evaluation, and security.
- **Composition:** workflows and multi-agent coordination through shared execution contracts.

This keeps dependencies small and makes each agent's decision process explicit and independently testable.

## Install only what you need

AgentForge is distributed as modular NuGet packages, not as a mandatory all-in-one bundle. Install the package for the architecture you want; NuGet resolves its required shared abstractions automatically. Add optional capabilities (models, tools, memory, retrieval, and so on) only when the selected agent needs them. There is no requirement to install unrelated agent packages.

For example, a deterministic Learning Agent can be used without installing ReAct, RAG, or any model provider:

```bash
dotnet add package CSharp.AgentForge.Agents.Learning
```

All current agent modules are published on NuGet.org. Browse the [AgentForge packages](https://www.nuget.org/packages?q=CSharp.AgentForge) and install only the module you need.

## Fluent construction model

Each module exposes a discoverable, strongly typed builder. The method names are consistent where concepts are shared, while agent-specific configuration remains specific to that architecture. `Build()` validates configuration and returns the common `AgentHandle<TInput,TOutput>` execution surface.

```csharp
var agent = LearningAgentBuilder.Create<string>()
    .WithEpsilonGreedy(epsilon: 0.1)
    .Build();

var result = await agent.RunAsync(input, cancellationToken);
```

There is deliberately no mandatory universal builder or package that references every agent. This keeps installation minimal and avoids exposing irrelevant options on agents that do not support them.

## Agent Quick Starts

Choose an agent to jump directly to its Quick Start. Full behavior, extension points, limits, and results are documented in the same section.

| Agent | Purpose | Quick Start |
|---|---|---|
| **Simple Reflex** | Ordered condition-action rules. | [Build and run](#simple-reflex-quick-start) |
| **Model-Based Reflex** | State update followed by rule-based action selection. | [Build and run](#model-based-reflex-quick-start) |
| **ReAct** | Model decisions, authorized tool calls, and observations. | [Build and run](#react-quick-start) |
| **Plan-and-Execute** | Validated plans and registered sequential step handlers. | [Build and run](#plan-and-execute-quick-start) |
| **Reflection** | Candidate generation, independent evaluation, and refinement. | [Build and run](#reflection-quick-start) |
| **Utility-Based** | Candidate evaluation and maximum-utility action selection. | [Build and run](#utility-based-quick-start) |
| **Goal-Based** | Bounded state-space search for a shortest goal-reaching action sequence. | [Build and run](#goal-based-quick-start) |
| **Learning** | Feedback-driven action selection using epsilon-greedy and contextual LinUCB policies. | [Build and run](#learning-quick-start) |
| **Rule-Based / Production System** | Forward-chaining condition/action rules over explicit working memory. | [Build and run](#rule-based-production-system-quick-start) |
| **Subsumption** | Priority-ordered reactive behavior layers with suppression. | [Build and run](#subsumption-quick-start) |

## Agent Modules

Each module below has its own installation command, Quick Start, and agent-specific behavior and execution limits.

### Simple Reflex

A deterministic agent that selects an action from the current percept using ordered condition-action rules.

#### Installation

Install the package from NuGet.org:

```bash
dotnet add package CSharp.AgentForge.Agents.SimpleReflex
```

#### Simple Reflex Quick Start
Define the percept and action types, then build and run the agent:

```csharp
using AgentForge.Agents.SimpleReflex.Builders;

var agent = SimpleReflexAgentBuilder
    .Create<ThermostatPercept, ThermostatAction>()
    .When(
        "too-hot",
        percept => percept.TemperatureCelsius > percept.TargetCelsius + 1,
        ThermostatAction.Cool)
    .When(
        "too-cold",
        percept => percept.TemperatureCelsius < percept.TargetCelsius - 1,
        ThermostatAction.Heat)
    .Build();

var result = await agent.RunAsync(
    new ThermostatPercept(29, 22),
    cancellationToken);

var output = result.Output;
if (output?.Decision.HasAction == true)
{
    Console.WriteLine(output.Decision.Action);
}

public sealed record ThermostatPercept(double TemperatureCelsius, double TargetCelsius);

public enum ThermostatAction
{
    Cool,
    Heat
}
```

#### Behavior

- Rules run in declaration order; the first match wins.
- No matching rule returns a successful no-action decision.
- The agent proposes an action; the host performs the side effect.
- No model, provider, tool registry, or database is required.

### Model-Based Reflex

A deterministic agent that updates an explicit internal state from the previous state and current percept before selecting an action.

#### Installation

Install the package from NuGet.org:

```bash
dotnet add package CSharp.AgentForge.Agents.ModelBasedReflex
```

#### Model-Based Reflex Quick Start
Define the percept, state, and action types. Supply the state transition and ordered rules:

```csharp
using AgentForge.Agents.ModelBasedReflex.Builders;
using AgentForge.Agents.ModelBasedReflex.Domain.Decisions;

var agent = ModelBasedReflexAgentBuilder
    .Create<ThermostatPercept, ThermostatState, ThermostatAction>()
    .WithStateUpdater((previous, percept) => new ThermostatState(
        PreviousTemperature: previous.CurrentTemperature,
        CurrentTemperature: percept.TemperatureCelsius,
        TargetTemperature: percept.TargetCelsius))
    .When(
        "rising-above-target",
        state => state.CurrentTemperature > state.PreviousTemperature
            && state.CurrentTemperature > state.TargetTemperature,
        ThermostatAction.Cool)
    .When(
        "falling-below-target",
        state => state.CurrentTemperature < state.PreviousTemperature
            && state.CurrentTemperature < state.TargetTemperature,
        ThermostatAction.Heat)
    .Build();

var result = await agent.RunAsync(
    new ModelBasedReflexInput<ThermostatPercept, ThermostatState>(
        new ThermostatPercept(29, 22),
        new ThermostatState(27, 28, 22)),
    cancellationToken);

var output = result.Output;
if (output is not null)
{
    Console.WriteLine(output.CurrentState);

    if (output.Decision.HasAction)
    {
        Console.WriteLine(output.Decision.Action);
    }
}

public sealed record ThermostatPercept(double TemperatureCelsius, double TargetCelsius);

public sealed record ThermostatState(
    double PreviousTemperature,
    double CurrentTemperature,
    double TargetTemperature);

public enum ThermostatAction
{
    Cool,
    Heat
}
```

#### Behavior

- The state updater runs before rule evaluation.
- Rules run in declaration order; the first match wins.
- The host owns state persistence and supplies the previous state to each run.
- No matching rule returns the updated state with a no-action decision.
- No model, provider, or database is required.

### Rule-Based / Production System

A deterministic production-system agent that applies ordered rules to explicit working memory. After each rule firing, it restarts evaluation from the first rule until the memory is stable or a firing budget is exhausted.

#### Installation

```bash
dotnet add package CSharp.AgentForge.Agents.RuleBased
```

#### Rule-Based Production System Quick Start

```csharp
using AgentForge.Agents.RuleBased.Builders;
using AgentForge.Agents.RuleBased.Models;

var agent = RuleBasedAgentBuilder.Create<Inventory, string>()
    .When(
        "reorder",
        inventory => inventory.Stock < inventory.ReorderPoint,
        inventory => $"Order {inventory.Sku}",
        inventory => inventory with { Stock = inventory.Stock + inventory.ReorderQuantity })
    .WithMaxFirings(8)
    .Build();

var result = await agent.RunAsync(new RuleBasedAgentInput<Inventory>(
    new Inventory("SKU-42", 3, 5, 10)), cancellationToken);

Console.WriteLine(result.Output?.FinalWorkingMemory);
Console.WriteLine(string.Join(", ", result.Output?.FiredRules ?? []));

public sealed record Inventory(string Sku, int Stock, int ReorderPoint, int ReorderQuantity);
```

#### Behavior

- Rules run in registration order; the first applicable rule fires.
- The selected rule emits an action and transforms working memory; evaluation then restarts.
- The run ends when no rule applies or returns `BudgetExhausted` when a firing limit is reached.
- The host owns memory persistence, action authorization, and external side effects.
- Unlike Simple Reflex, this module iterates over changing working memory rather than making one decision from one percept.

### Subsumption

A layered reactive agent inspired by the Subsumption Architecture. Independent behaviors are registered at different priorities; the highest-priority active layer selects the action and suppresses lower layers for that decision cycle.

#### Installation

```bash
dotnet add package CSharp.AgentForge.Agents.Subsumption
```

#### Subsumption Quick Start

```csharp
using AgentForge.Agents.Subsumption.Builders;
using AgentForge.Agents.Subsumption.Models;

var agent = SubsumptionAgentBuilder.Create<RobotPercept, RobotAction>()
    .WithLayer("emergency-stop", 100,
        percept => percept.ObstacleDistanceMeters < 0.5, _ => RobotAction.Stop)
    .WithLayer("avoid-obstacle", 50,
        percept => percept.ObstacleDistanceMeters < 2, _ => RobotAction.TurnAway)
    .WithLayer("wander", 10, _ => true, _ => RobotAction.MoveForward)
    .Build();

var result = await agent.RunAsync(
    new SubsumptionAgentInput<RobotPercept>(new RobotPercept(0.3)),
    cancellationToken);

Console.WriteLine(result.Output?.Action);

public sealed record RobotPercept(double ObstacleDistanceMeters);
public enum RobotAction { Stop, TurnAway, MoveForward }
```

#### Behavior

- Layers are evaluated by descending priority; ties preserve registration order.
- The first active layer wins and lower layers are not evaluated.
- No active layer returns a successful no-action result.
- Each run processes one percept; the host supplies the next percept on the next cycle.
- Unlike a flat reflex rule list, the layer structure makes behavior suppression and safety overrides explicit.

### ReAct

A tool-using agent that alternates model decisions, authorized tool execution, and observations until it receives a final response or reaches a run limit.

#### Installation

Install the package from NuGet.org:

```bash
dotnet add package CSharp.AgentForge.Agents.ReAct
```

#### ReAct Quick Start
Register an `IToolCallingChatModel` provider adapter, the tools available to this application, and an `IToolAuthorizationPolicy` with your application's dependency injection container.

```csharp
using AgentForge.Abstractions.Execution;
using AgentForge.Agents.ReAct;
using AgentForge.Agents.ReAct.Builders;
using AgentForge.Models.Contracts;
using AgentForge.Tools;
using Microsoft.Extensions.DependencyInjection;

var agent = ReActAgentBuilder.Create()
    .WithModel(services.GetRequiredService<IToolCallingChatModel>())
    .WithTools(services.GetServices<ITool>())
    .WithAuthorization(services.GetRequiredService<IToolAuthorizationPolicy>())
    .WithSystemPrompt("Use available tools when needed.")
    .WithMaxModelTurns(8)
    .WithMaxToolInvocations(12)
    .Build();

var result = await agent.RunAsync(
    "Find the status of order 123.",
    cancellationToken);

Console.WriteLine(result.Output?.FinalResponse);
```

The host must register the provider adapter, explicitly approved tools, and authorization policy. The model's tool-call proposal is never treated as authorization.

#### Behavior

- Every tool call is validated and authorized before execution.
- Model turns, tool invocations, steps, observation size, and elapsed time are bounded.
- Provider adapters are application-supplied; no vendor SDK is bundled.
- The module-specific Fluent API is the recommended construction path; direct constructor composition remains available for advanced hosts.

### Plan-and-Execute

A structured Plan-and-Execute agent that validates a generated plan before running its registered step handlers.

#### Installation

Install the package from NuGet.org:

```bash
dotnet add package CSharp.AgentForge.Agents.Planning
```

#### Plan-and-Execute Quick Start
Register an application-specific `IPlanningStrategy` and only the `IPlanStepHandler` implementations that the agent is allowed to execute.

```csharp
using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Planning;
using AgentForge.Agents.Planning.Builders;
using AgentForge.Agents.Planning.Execution;
using AgentForge.Agents.Planning.Planning;
using Microsoft.Extensions.DependencyInjection;

var agent = PlanAndExecuteAgentBuilder.Create()
    .WithPlanner(services.GetRequiredService<IPlanningStrategy>())
    .WithStepHandlers(services.GetServices<IPlanStepHandler>())
    .WithMaxPlanSteps(8)
    .WithTimeout(TimeSpan.FromSeconds(60))
    .Build();

var result = await agent.RunAsync(
    "Find the customer account and calculate the renewal date.",
    cancellationToken);

Console.WriteLine($"Completed steps: {result.Output?.CompletedStepCount}");
```

The planner and step handlers are application-owned dependencies. Register only handlers permitted for this workload.

#### Behavior

- The complete plan is validated before any handler executes.
- Step types must exist in the immutable handler registry.
- Steps execute sequentially and receive only explicitly referenced prior outputs.
- The first failed step stops the run; later steps are marked blocked.
- Automatic replanning, retries, parallel execution, and durable state are not included.

### Reflection

A bounded candidate-refinement agent that separates generation from independent evaluation.

#### Installation

Install the package from NuGet.org:

```bash
dotnet add package CSharp.AgentForge.Agents.Reflection
```

#### Reflection Quick Start
Register an application-specific `IReflectionGenerationStrategy` and an `IReflectionEvaluator`. The evaluator owns the acceptance criteria and returns actionable feedback for rejected candidates.

```csharp
using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Reflection;
using AgentForge.Agents.Reflection.Builders;
using AgentForge.Agents.Reflection.Evaluation;
using AgentForge.Agents.Reflection.Generation;
using Microsoft.Extensions.DependencyInjection;

var agent = ReflectionAgentBuilder.Create()
    .WithGenerator(services.GetRequiredService<IReflectionGenerationStrategy>())
    .WithEvaluator(services.GetRequiredService<IReflectionEvaluator>())
    .WithMaxAttempts(3)
    .WithTimeout(TimeSpan.FromSeconds(90))
    .Build();

var result = await agent.RunAsync(
    "Write a concise explanation of the CAP theorem.",
    cancellationToken);

Console.WriteLine(result.Output?.BestCandidate);
Console.WriteLine($"Accepted: {result.Output?.IsAccepted}");
```

#### Behavior

- Generation and evaluation are independent application-supplied strategies.
- Rejected feedback is passed to the next generation attempt.
- The best-scoring candidate is retained when comparable scores are supplied.
- Execution stops on acceptance, configured no-improvement, attempt limit, timeout, or cancellation.
- Scores from different evaluators are not comparable.

### Utility-Based

A decision agent that evaluates the available candidate actions and proposes the action with the greatest utility score.

#### Installation

Install the package from NuGet.org:

```bash
dotnet add package CSharp.AgentForge.Agents.UtilityBased
```

#### Utility-Based Quick Start

The candidate provider defines the actions available for the current percept. The utility evaluator assigns a score to each candidate:

```csharp
using AgentForge.Agents.UtilityBased.Builders;
using AgentForge.Agents.UtilityBased.Domain.Policies;

var agent = UtilityBasedAgentBuilder
    .Create<ThermostatPercept, ThermostatAction>()
    .WithCandidates(_ => new[]
    {
        ThermostatAction.Cool,
        ThermostatAction.Heat,
        ThermostatAction.Maintain
    })
    .WithUtility((percept, action) => action switch
    {
        ThermostatAction.Cool =>
            percept.TemperatureCelsius - percept.TargetCelsius,
        ThermostatAction.Heat =>
            percept.TargetCelsius - percept.TemperatureCelsius,
        ThermostatAction.Maintain =>
            -Math.Abs(percept.TemperatureCelsius - percept.TargetCelsius),
        _ => throw new ArgumentOutOfRangeException(nameof(action))
    })
    .WithTieBreakPolicy(UtilityTieBreakPolicy.FirstCandidate)
    .Build();

var result = await agent.RunAsync(
    new ThermostatPercept(29, 22),
    cancellationToken);

if (result.Output?.Decision.HasAction == true)
{
    Console.WriteLine(result.Output.Decision.Action);
    Console.WriteLine(result.Output.Decision.Utility);
}

public sealed record ThermostatPercept(double TemperatureCelsius, double TargetCelsius);

public enum ThermostatAction
{
    Cool,
    Heat,
    Maintain
}
```

#### Behavior

- Every candidate is evaluated exactly once and in the supplied order.
- The candidate with the greatest finite utility score is selected.
- Negative and zero scores are valid; NaN and infinite scores fail the run.
- An empty candidate set returns a successful no-action decision.
- Equal maximum scores follow the configured `FirstCandidate`, `LastCandidate`, or `RejectAmbiguous` policy.
- The output preserves all candidate evaluations, not only the winner.
- The agent proposes an action; the host performs the side effect.
- The utility function is application-supplied. No model provider, tool registry, or database is required.

### Goal-Based

A goal-directed search agent that explores the state space and returns a shortest sequence of proposed actions satisfying an explicit goal predicate. The initial implementation uses bounded breadth-first search.

#### Installation

Install the package from NuGet.org:

```bash
dotnet add package CSharp.AgentForge.Agents.GoalBased
```

#### Goal-Based Quick Start

The application supplies the available state transitions and a predicate that determines whether a state satisfies the goal:

```csharp
using AgentForge.Agents.GoalBased.Builders;
using AgentForge.Agents.GoalBased.Domain.Models;

var routes = new Dictionary<string, GoalBasedTransition<string, string>[]>
{
    ["Home"] = [new("Go to station", "Station"), new("Go to park", "Park")],
    ["Station"] = [new("Take the train", "Museum")],
    ["Park"] = [new("Walk to museum", "Museum")],
    ["Museum"] = []
};

var agent = GoalBasedAgentBuilder
    .Create<string, string, string>()
    .WithSuccessors(state => routes[state])
    .WithGoalPredicate((state, goal) => state == goal)
    .WithLimits(maxDepth: 8, maxExploredStates: 100)
    .Build();

var result = await agent.RunAsync(
    new GoalBasedInput<string, string>("Home", "Museum"),
    cancellationToken);

if (result.Output?.Plan is { } plan)
{
    Console.WriteLine(string.Join(" -> ", plan.Actions));
}
```

#### Behavior

- Breadth-first search returns a shortest plan when all transitions have equal cost.
- State equality prevents revisiting already discovered states; a custom comparer can be supplied.
- Search is bounded by maximum depth and explored-state count.
- Results distinguish a reached goal, an exhausted reachable state space, and a search limit.
- The agent proposes a plan; the host authorizes and executes actions.
- No LLM, tool registry, or external infrastructure is required.


### Learning

A feedback-driven agent that improves action selection from reward observations using an epsilon-greedy multi-armed bandit policy.

#### Installation

~~~bash
dotnet add package CSharp.AgentForge.Agents.Learning
~~~

#### Learning Quick Start

~~~csharp
using AgentForge.Agents.Learning.Builders;
using AgentForge.Agents.Learning.Domain.Models;

var agent = LearningAgentBuilder.Create<string>().WithEpsilonGreedy(0.1).Build();
var state = new LearningState<string>();
var decision = await agent.RunAsync(new LearningAgentInput<string>(["search", "fallback"], state), cancellationToken);
var selected = decision.Output!.SelectedAction;

// The host executes and evaluates the action, then records the reward.
var reward = selected == "search" ? 1.0 : 0.2;
state = state.Observe(selected, reward);
~~~

#### Behavior

- Unobserved candidates are tried before exploitation.
- Epsilon-greedy exploration balances trying alternatives with selecting the highest estimated reward.
- The host owns execution, authorization, reward evaluation, and persistence.
- Learning state is immutable; the host controls how it is persisted.
- The epsilon-greedy learner is a stationary, context-free bandit.
- The separate Contextual LinUCB implementation uses fixed-dimensional context features and a linear reward model per action.
- Neither implementation is a general MDP or deep reinforcement-learning system.

#### Contextual LinUCB Quick Start

```csharp
using AgentForge.Agents.Learning.Builders;
using AgentForge.Agents.Learning.Domain.Models;

var agent = ContextualLearningAgentBuilder.Create<string>()
    .WithExplorationCoefficient(1.0)
    .Build();

var state = new ContextualLearningState<string>(featureCount: 2);
var context = new[] { 0.7, 1.0 };
var decision = await agent.RunAsync(
    new ContextualLearningAgentInput<string>(["search", "recommend"], context, state),
    cancellationToken);

var selected = decision.Output!.SelectedAction;
var reward = selected == "search" ? 1.0 : 0.4;
state = state.Observe(selected, context, reward);
```

The host supplies and scales features, executes actions, evaluates rewards, and persists state. LinUCB uses a Cholesky-based solver and returns the predicted reward, exploration bonus, and upper-confidence score for the selected action.

## NuGet packages

Each agent is distributed as an independent package. The package IDs and package-specific READMEs are:

| Package ID | NuGet | Package README |
|---|---|
| `CSharp.AgentForge.Agents.SimpleReflex` | [1.0.0](https://www.nuget.org/packages/CSharp.AgentForge.Agents.SimpleReflex/) | [Simple Reflex](https://github.com/peymanpro/AgentForge/blob/main/src/AgentForge.Agents.SimpleReflex/README.md) |
| `CSharp.AgentForge.Agents.ModelBasedReflex` | [1.0.0](https://www.nuget.org/packages/CSharp.AgentForge.Agents.ModelBasedReflex/) | [Model-Based Reflex](https://github.com/peymanpro/AgentForge/blob/main/src/AgentForge.Agents.ModelBasedReflex/README.md) |
| `CSharp.AgentForge.Agents.ReAct` | [1.0.0](https://www.nuget.org/packages/CSharp.AgentForge.Agents.ReAct/) | [ReAct](https://github.com/peymanpro/AgentForge/blob/main/src/AgentForge.Agents.ReAct/README.md) |
| `CSharp.AgentForge.Agents.Planning` | [1.0.0](https://www.nuget.org/packages/CSharp.AgentForge.Agents.Planning/) | [Plan-and-Execute](https://github.com/peymanpro/AgentForge/blob/main/src/AgentForge.Agents.Planning/README.md) |
| `CSharp.AgentForge.Agents.Reflection` | [1.0.0](https://www.nuget.org/packages/CSharp.AgentForge.Agents.Reflection/) | [Reflection](https://github.com/peymanpro/AgentForge/blob/main/src/AgentForge.Agents.Reflection/README.md) |
| `CSharp.AgentForge.Agents.UtilityBased` | [1.0.0](https://www.nuget.org/packages/CSharp.AgentForge.Agents.UtilityBased/) | [Utility-Based](https://github.com/peymanpro/AgentForge/blob/main/src/AgentForge.Agents.UtilityBased/README.md) |
| `CSharp.AgentForge.Agents.GoalBased` | [1.0.0](https://www.nuget.org/packages/CSharp.AgentForge.Agents.GoalBased/) | [Goal-Based](https://github.com/peymanpro/AgentForge/blob/main/src/AgentForge.Agents.GoalBased/README.md) |
| `CSharp.AgentForge.Agents.Learning` | [1.0.0](https://www.nuget.org/packages/CSharp.AgentForge.Agents.Learning/) | [Learning](https://github.com/peymanpro/AgentForge/blob/main/src/AgentForge.Agents.Learning/README.md) |
| `CSharp.AgentForge.Agents.RuleBased` | [1.0.0](https://www.nuget.org/packages/CSharp.AgentForge.Agents.RuleBased/) | [Rule-Based / Production System](https://github.com/peymanpro/AgentForge/blob/main/src/AgentForge.Agents.RuleBased/README.md) |
| `CSharp.AgentForge.Agents.Subsumption` | [1.0.0](https://www.nuget.org/packages/CSharp.AgentForge.Agents.Subsumption/) | [Subsumption](https://github.com/peymanpro/AgentForge/blob/main/src/AgentForge.Agents.Subsumption/README.md) |

Each package project declares its module README through `PackageReadmeFile` and includes that Markdown file in the `.nupkg`. NuGet.org therefore displays the matching agent's description and Quick Start on that package's page.

All 10 agent modules are available on NuGet.org. Select a package above to open its NuGet Gallery page, or install a module directly:

```bash
dotnet add package CSharp.AgentForge.Agents.SimpleReflex
```

Replace the package ID with the module you need.

## Architecture

Each agent module follows Clean Architecture where it adds value:

- **Domain:** agent concepts, rules, and invariants.
- **Application:** the decision process and use cases.
- **Infrastructure:** optional external adapters, only when required.
- **Public API:** the module's construction and composition surface.

Agent modules use the shared execution contract and do not depend on other agents' concrete implementations. See [Module Architecture](docs/agent-modules.md), [System Architecture](docs/architecture.md), and the [Goal-Based search design](docs/goal-based-agent-design.md).

## Learning benchmark protocol

AgentForge keeps stationary context-free bandits and contextual bandits as separate evaluation families. The benchmark protocol defines comparable environments, regret and reward metrics, reproducible seeds, parameter-tuning controls, and interpretation limits. See [Learning Bandit Benchmark Protocol](docs/learning-bandit-benchmark.md).

## Benchmarks

Reproducible experiments for learning policies live under [benchmarks](benchmarks/README.md). The benchmark runner compares Epsilon-Greedy with Beta-Bernoulli Thompson Sampling on stationary context-free bandits, and LinUCB with Bayesian Linear Thompson Sampling on contextual bandits. The two families produce separate reports and are not ranked against each other.

## Development

Requirements: .NET SDK `10.0.403` (see [global.json](global.json)).

```bash
dotnet restore AgentForge.sln
dotnet build AgentForge.sln --configuration Release --no-restore
dotnet test AgentForge.sln --configuration Release --no-build --no-restore
```

## License

MIT. See [LICENSE](LICENSE).
