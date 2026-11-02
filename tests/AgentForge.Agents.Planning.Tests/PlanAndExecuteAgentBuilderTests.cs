using AgentForge.Agents.Planning.Builders;
using AgentForge.Agents.Planning.Planning;
using Xunit;

namespace AgentForge.Agents.Planning.Tests;

public sealed class PlanAndExecuteAgentBuilderTests
{
    [Fact]
    public void Build_RequiresPlanningStrategy()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            () => PlanAndExecuteAgentBuilder.Create().Build());

        Assert.Contains("planning strategy", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_CreatesAgentHandleWhenRequiredStrategyIsConfigured()
    {
        var agent = PlanAndExecuteAgentBuilder.Create()
            .WithPlanner(new UnusedPlanningStrategy())
            .WithMaxPlanSteps(5)
            .WithTimeout(TimeSpan.FromSeconds(15))
            .Build();

        Assert.NotNull(agent);
    }

    [Fact]
    public void WithLimits_RejectsNonPositiveValues()
    {
        var builder = PlanAndExecuteAgentBuilder.Create();

        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithMaxPlanSteps(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithTimeout(TimeSpan.Zero));
    }

    private sealed class UnusedPlanningStrategy : IPlanningStrategy
    {
        public Task<ExecutionPlan> GenerateAsync(
            PlanningRequest request,
            CancellationToken cancellationToken = default)
        {
            throw new NotSupportedException("This strategy is not invoked by builder tests.");
        }
    }
}
