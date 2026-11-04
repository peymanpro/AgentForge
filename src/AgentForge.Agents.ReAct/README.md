# AgentForge.Agents.ReAct

A tool-using agent that alternates model decisions, authorized tool execution, and observations until it receives a final response or reaches a run limit.

## Installation

Install this package from NuGet.org:

```bash
dotnet add package CSharp.AgentForge.Agents.ReAct
```

## Quick Start

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

## Behavior

- Every tool call is validated and authorized before execution.
- Model turns, tool invocations, steps, observation size, and elapsed time are bounded.
- Provider adapters are application-supplied; no vendor SDK is bundled.
- The fluent builder is the recommended construction API. Advanced hosts may still instantiate `ReActAgent` directly when they need explicit low-level composition.
