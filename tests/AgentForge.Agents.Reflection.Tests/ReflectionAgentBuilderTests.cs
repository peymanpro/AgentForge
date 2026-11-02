using AgentForge.Agents.Reflection.Builders;
using AgentForge.Agents.Reflection.Evaluation;
using AgentForge.Agents.Reflection.Generation;
using Xunit;

namespace AgentForge.Agents.Reflection.Tests;

public sealed class ReflectionAgentBuilderTests
{
    [Fact]
    public void Build_RequiresGenerationStrategy()
    {
        var builder = ReflectionAgentBuilder.Create()
            .WithEvaluator(new UnusedEvaluator());

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Contains("generation strategy", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_RequiresEvaluator()
    {
        var builder = ReflectionAgentBuilder.Create()
            .WithGenerator(new UnusedGenerationStrategy());

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Contains("evaluator", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_CreatesAgentHandleWhenRequiredStrategiesAreConfigured()
    {
        var agent = ReflectionAgentBuilder.Create()
            .WithGenerator(new UnusedGenerationStrategy())
            .WithEvaluator(new UnusedEvaluator())
            .WithMaxAttempts(4)
            .WithTimeout(TimeSpan.FromSeconds(20))
            .WithStopOnNoImprovement()
            .WithMinimumScoreImprovement(0.05)
            .Build();

        Assert.NotNull(agent);
    }

    [Fact]
    public void WithLimits_RejectsInvalidValues()
    {
        var builder = ReflectionAgentBuilder.Create();

        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithMaxAttempts(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithTimeout(TimeSpan.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithMinimumScoreImprovement(double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithMinimumScoreImprovement(1.1));
    }

    private sealed class UnusedGenerationStrategy : IReflectionGenerationStrategy
    {
        public Task<string> GenerateAsync(
            ReflectionGenerationRequest request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Not invoked by builder tests.");
    }

    private sealed class UnusedEvaluator : IReflectionEvaluator
    {
        public Task<ReflectionEvaluation> EvaluateAsync(
            ReflectionEvaluationRequest request,
            CancellationToken cancellationToken = default) =>
            throw new NotSupportedException("Not invoked by builder tests.");
    }
}
