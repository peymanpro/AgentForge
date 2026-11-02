using AgentForge.Abstractions.Execution;
using AgentForge.Agents.SimpleReflex.Builders;
using Xunit;

namespace AgentForge.Agents.SimpleReflex.Tests;

public sealed class SimpleReflexAgentBuilderTests
{
    [Fact]
    public async Task Build_CreatesAnAgentThatUsesTheFirstMatchingRule()
    {
        var agent = SimpleReflexAgentBuilder
            .Create<int, string>()
            .When("urgent", value => value >= 10, "urgent-action")
            .When("normal", value => value >= 5, "normal-action")
            .Build();

        var result = await agent.RunAsync(12);

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.NotNull(result.Output);
        Assert.True(result.Output.Decision.HasAction);
        Assert.Equal("urgent-action", result.Output.Decision.Action);
        Assert.Equal("urgent", result.Output.MatchedRuleName);
    }

    [Fact]
    public async Task Build_ReturnsNoActionWhenNoRuleMatches()
    {
        var agent = SimpleReflexAgentBuilder
            .Create<int, string>()
            .When("high", value => value > 10, "cool")
            .Build();

        var result = await agent.RunAsync(5);

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.NotNull(result.Output);
        Assert.False(result.Output.Decision.HasAction);
        Assert.Null(result.Output.MatchedRuleName);
    }

    [Fact]
    public async Task RunAsync_UsesSuppliedOptions()
    {
        var agent = SimpleReflexAgentBuilder
            .Create<int, string>()
            .When("always", _ => true, "act")
            .Build();

        var result = await agent.RunAsync(1, new AgentRunOptions { MaxSteps = 0 });

        Assert.Equal(AgentRunStatus.Rejected, result.Status);
        Assert.Equal(AgentFailureCategory.InvalidInput, result.Failure?.Category);
    }

    [Fact]
    public async Task RunAsync_HonorsCancellation()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();

        var agent = SimpleReflexAgentBuilder
            .Create<int, string>()
            .When("always", _ => true, "act")
            .Build();

        var result = await agent.RunAsync(1, source.Token);

        Assert.Equal(AgentRunStatus.Cancelled, result.Status);
    }

    [Fact]
    public void Build_RejectsAnEmptyRuleSet()
    {
        var builder = SimpleReflexAgentBuilder.Create<int, string>();
        Assert.Throws<ArgumentException>(() => builder.Build());
    }

    [Fact]
    public void Build_RejectsDuplicateRuleNames()
    {
        var builder = SimpleReflexAgentBuilder
            .Create<int, string>()
            .When("same", _ => true, "first")
            .When("same", _ => true, "second");

        Assert.Throws<ArgumentException>(() => builder.Build());
    }
}
