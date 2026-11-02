# AgentForge.Agents.Reflection

A bounded candidate-refinement agent that separates generation from independent evaluation.

## Quick Start

Register an application-specific \`IReflectionGenerationStrategy\` and an \`IReflectionEvaluator\`. The evaluator owns the acceptance criteria and returns actionable feedback for rejected candidates.

\`\`\`csharp
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
\`\`\`

## Behavior

- Generation and evaluation are independent application-supplied strategies.
- Rejected feedback is passed to the next generation attempt.
- The best-scoring candidate is retained when comparable scores are supplied.
- Execution stops on acceptance, configured no-improvement, attempt limit, timeout, or cancellation.
- Scores from different evaluators are not comparable.
