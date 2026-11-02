using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Classical;

namespace AgentForge.Agents.Classical.Tests;

public sealed class SimpleReflexAgentTests
{
    [Fact]
    public async Task RunAsync_SelectsTheFirstMatchingRule()
    {
        var rules = new[]
        {
            new ReflexRule<int, string>("urgent", value => value >= 10, _ => "urgent-action"),
            new ReflexRule<int, string>("normal", value => value >= 5, _ => "normal-action")
        };
        var agent = new SimpleReflexAgent<int, string>(rules);

        var result = await agent.RunAsync(12, AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.NotNull(result.Output);
        Assert.True(result.Output.Decision.HasAction);
        Assert.Equal("urgent-action", result.Output.Decision.Action);
        Assert.Equal("urgent", result.Output.AppliedRuleName);
    }

    [Fact]
    public async Task RunAsync_ReturnsSuccessfulNoActionWhenNoRuleMatches()
    {
        var rules = new[]
        {
            new ReflexRule<int, string>("high", value => value > 10, _ => "cool")
        };
        var agent = new SimpleReflexAgent<int, string>(rules);

        var result = await agent.RunAsync(5, AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.NotNull(result.Output);
        Assert.False(result.Output.Decision.HasAction);
        Assert.Null(result.Output.AppliedRuleName);
    }

    [Fact]
    public async Task RunAsync_RejectsInvalidRunOptions()
    {
        var agent = new SimpleReflexAgent<int, string>(
        [
            new ReflexRule<int, string>("always", _ => true, _ => "act")
        ]);

        var result = await agent.RunAsync(
            1,
            AgentRunContext.Create(),
            new AgentRunOptions { MaxSteps = 0 });

        Assert.Equal(AgentRunStatus.Rejected, result.Status);
        Assert.Equal(AgentFailureCategory.InvalidInput, result.Failure?.Category);
    }

    [Fact]
    public async Task RunAsync_ReturnsCancelledWhenTokenIsAlreadyCancelled()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var agent = new SimpleReflexAgent<int, string>(
        [
            new ReflexRule<int, string>("always", _ => true, _ => "act")
        ]);

        var result = await agent.RunAsync(
            1,
            AgentRunContext.Create(),
            new AgentRunOptions(),
            source.Token);

        Assert.Equal(AgentRunStatus.Cancelled, result.Status);
    }

    [Fact]
    public void Constructor_RejectsDuplicateRuleNames()
    {
        var rules = new[]
        {
            new ReflexRule<int, string>("same", _ => true, _ => "first"),
            new ReflexRule<int, string>("same", _ => true, _ => "second")
        };

        Assert.Throws<ArgumentException>(() => new SimpleReflexAgent<int, string>(rules));
    }
}
