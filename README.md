# AgentForge

**A modular .NET 10 library for building, running, and composing distinct agent architectures.**

Each independently useful agent architecture has its own module, focused API, and optional dependencies. This README contains the documentation and Quick Start for every current agent. Each NuGet module also ships its own package README.

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/)
[![CI](https://github.com/peymanpro/AgentForge/actions/workflows/agentforge-ci.yml/badge.svg?branch=main)](https://github.com/peymanpro/AgentForge/actions/workflows/agentforge-ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

> **Status:** Experimental. Public APIs may change. NuGet packages are not published yet.

## Contents

- [Why AgentForge](#why-agentforge)
- [Agent Quick Starts](#agent-quick-starts)
  - [Simple Reflex](#simple-reflex-quick-start)
  - [Model-Based Reflex](#model-based-reflex-quick-start)
  - [ReAct](#react-quick-start)
  - [Plan-and-Execute](#plan-and-execute-quick-start)
  - [Reflection](#reflection-quick-start)
- [NuGet packages](#nuget-packages)
- [Architecture](#architecture)
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

## Agent Quick Starts

Choose an agent to jump directly to its Quick Start. Full behavior, extension points, limits, and results are documented in the same section.

| Agent | Purpose | Quick Start |
|---|---|---|
| **Simple Reflex** | Ordered condition-action rules. | [Build and run](#simple-reflex-quick-start) |
| **Model-Based Reflex** | State update followed by rule-based action selection. | [Build and run](#model-based-reflex-quick-start) |
| **ReAct** | Model decisions, authorized tool calls, and observations. | [Build and run](#react-quick-start) |
| **Plan-and-Execute** | Validated plans and registered sequential step handlers. | [Build and run](#plan-and-execute-quick-start) |
| **Reflection** | Candidate generation, independent evaluation, and refinement. | [Build and run](#reflection-quick-start) |

## Agent Modules

Each module below has its own installation command, Quick Start, and agent-specific behavior and execution limits.

### Simple Reflex

A deterministic agent that selects an action from the current percept using ordered condition-action rules.

#### Installation

After this package is published to NuGet.org, install it with:

```bash
dotnet add package AgentForge.Agents.SimpleReflex
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

After this package is published to NuGet.org, install it with:

```bash
dotnet add package AgentForge.Agents.ModelBasedReflex
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

### ReAct

A tool-using agent that alternates model decisions, authorized tool execution, and observations until it receives a final response or reaches a run limit.

#### Installation

After this package is published to NuGet.org, install it with:

```bash
dotnet add package AgentForge.Agents.ReAct
```

#### ReAct Quick Start
Register an `IToolCallingChatModel` provider adapter, the tools available to this application, and an `IToolAuthorizationPolicy` with your application's dependency injection container.

```csharp
using AgentForge.Abstractions.Execution;
using AgentForge.Agents.ReAct;
using AgentForge.Models.Contracts;
using AgentForge.Tools;
using Microsoft.Extensions.DependencyInjection;

var agent = new AgentHandle<string, ReActAgentOutput>(
    new ReActAgent(
        new NativeToolCallingDecisionStrategy(
            services.GetRequiredService<IToolCallingChatModel>()),
        new ToolRegistry(services.GetServices<ITool>()),
        services.GetRequiredService<IToolAuthorizationPolicy>(),
        new ReActAgentOptions(
            systemPrompt: "Use available tools when needed.",
            maxModelTurns: 8,
            maxToolInvocations: 12)));

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
- The current preview uses constructor-based composition. A module-specific fluent builder is part of the planned module migration.

### Plan-and-Execute

A structured Plan-and-Execute agent that validates a generated plan before running its registered step handlers.

#### Installation

After this package is published to NuGet.org, install it with:

```bash
dotnet add package AgentForge.Agents.Planning
```

#### Plan-and-Execute Quick Start
Register an application-specific `IPlanningStrategy` and only the `IPlanStepHandler` implementations that the agent is allowed to execute.

```csharp
using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Planning;
using AgentForge.Agents.Planning.Execution;
using AgentForge.Agents.Planning.Planning;
using Microsoft.Extensions.DependencyInjection;

var handlers = new PlanStepHandlerRegistry(
    services.GetServices<IPlanStepHandler>());

var agent = new AgentHandle<string, PlanAndExecuteOutput>(
    new PlanAndExecuteAgent(
        services.GetRequiredService<IPlanningStrategy>(),
        handlers,
        new PlanAndExecuteAgentOptions(
            maxPlanSteps: 8,
            timeout: TimeSpan.FromSeconds(60))));

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

After this package is published to NuGet.org, install it with:

```bash
dotnet add package AgentForge.Agents.Reflection
```

#### Reflection Quick Start
Register an application-specific `IReflectionGenerationStrategy` and an `IReflectionEvaluator`. The evaluator owns the acceptance criteria and returns actionable feedback for rejected candidates.

```csharp
using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Reflection;
using AgentForge.Agents.Reflection.Evaluation;
using AgentForge.Agents.Reflection.Generation;
using Microsoft.Extensions.DependencyInjection;

var agent = new AgentHandle<string, ReflectionAgentOutput>(
    new ReflectionAgent(
        services.GetRequiredService<IReflectionGenerationStrategy>(),
        services.GetRequiredService<IReflectionEvaluator>(),
        new ReflectionAgentOptions(
            maxAttempts: 3,
            timeout: TimeSpan.FromSeconds(90))));

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

## NuGet packages

Each agent is distributed as an independent package. The package IDs and package-specific READMEs are:

| Package ID | Package README |
|---|---|
| `AgentForge.Agents.SimpleReflex` | [Simple Reflex](https://github.com/peymanpro/AgentForge/blob/main/src/AgentForge.Agents.SimpleReflex/README.md) |
| `AgentForge.Agents.ModelBasedReflex` | [Model-Based Reflex](https://github.com/peymanpro/AgentForge/blob/main/src/AgentForge.Agents.ModelBasedReflex/README.md) |
| `AgentForge.Agents.ReAct` | [ReAct](https://github.com/peymanpro/AgentForge/blob/main/src/AgentForge.Agents.ReAct/README.md) |
| `AgentForge.Agents.Planning` | [Plan-and-Execute](https://github.com/peymanpro/AgentForge/blob/main/src/AgentForge.Agents.Planning/README.md) |
| `AgentForge.Agents.Reflection` | [Reflection](https://github.com/peymanpro/AgentForge/blob/main/src/AgentForge.Agents.Reflection/README.md) |

Each package project declares its module README through `PackageReadmeFile` and includes that Markdown file in the `.nupkg`. NuGet.org therefore displays the matching agent's description and Quick Start on that package's page.

Packages have not been published yet. Once released, install the selected module with:

```bash
dotnet add package AgentForge.Agents.SimpleReflex
```

Replace the package ID with the module you need.

## Architecture

Each agent module follows Clean Architecture where it adds value:

- **Domain:** agent concepts, rules, and invariants.
- **Application:** the decision process and use cases.
- **Infrastructure:** optional external adapters, only when required.
- **Public API:** the module's construction and composition surface.

Agent modules use the shared execution contract and do not depend on other agents' concrete implementations. See [Module Architecture](docs/agent-modules.md) and [System Architecture](docs/architecture.md).

## Development

Requirements: .NET SDK `10.0.403` (see [global.json](global.json)).

```bash
dotnet restore AgentForge.sln
dotnet build AgentForge.sln --configuration Release --no-restore
dotnet test AgentForge.sln --configuration Release --no-build --no-restore
```

## License

MIT. See [LICENSE](LICENSE).
