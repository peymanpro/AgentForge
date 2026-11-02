# AgentForge.Agents.ReAct

A tool-using agent that alternates model decisions, authorized tool execution, and observations until it receives a final response or reaches a run limit.

## Quick Start

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

## Behavior

- Every tool call is validated and authorized before execution.
- Model turns, tool invocations, steps, observation size, and elapsed time are bounded.
- Provider adapters are application-supplied; no vendor SDK is bundled.
- The current preview uses constructor-based composition. A module-specific fluent builder is part of the planned module migration.
