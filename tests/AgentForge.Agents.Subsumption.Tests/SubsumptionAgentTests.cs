using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Subsumption.Builders;
using AgentForge.Agents.Subsumption.Models;
using Xunit;

namespace AgentForge.Agents.Subsumption.Tests;

public sealed class SubsumptionAgentTests
{
    [Fact]
    public async Task HighestPriorityActiveLayerWins()
    {
        var agent = SubsumptionAgentBuilder.Create<Percept, string>()
            .WithLayer("wander", 1, _ => true, _ => "move")
            .WithLayer("avoid", 10, percept => percept.Distance < 2, _ => "turn")
            .WithLayer("stop", 100, percept => percept.Distance < 0.5, _ => "stop")
            .Build();

        var result = await agent.RunAsync(new SubsumptionAgentInput<Percept>(new Percept(0.2)));

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.Equal("stop", result.Output?.Action);
        Assert.Equal("stop", result.Output?.WinningLayer);
        Assert.Equal(100, result.Output?.WinningPriority);
    }

    [Fact]
    public async Task LowerPriorityLayersAreNotEvaluatedAfterWinner()
    {
        var lowerLayerEvaluations = 0;
        var agent = SubsumptionAgentBuilder.Create<Percept, string>()
            .WithLayer("priority", 2, _ => true, _ => "selected")
            .WithLayer("fallback", 1, _ =>
            {
                lowerLayerEvaluations++;
                return true;
            }, _ => "fallback")
            .Build();

        var result = await agent.RunAsync(new SubsumptionAgentInput<Percept>(new Percept(5)));

        Assert.Equal("selected", result.Output?.Action);
        Assert.Equal(0, lowerLayerEvaluations);
    }

    [Fact]
    public async Task EqualPriorityPreservesRegistrationOrder()
    {
        var agent = SubsumptionAgentBuilder.Create<Percept, string>()
            .WithLayer("first", 5, _ => true, _ => "first")
            .WithLayer("second", 5, _ => true, _ => "second")
            .Build();

        var result = await agent.RunAsync(new SubsumptionAgentInput<Percept>(new Percept(5)));

        Assert.Equal("first", result.Output?.WinningLayer);
    }

    [Fact]
    public async Task NoActiveLayerReturnsSuccessfulNoAction()
    {
        var agent = SubsumptionAgentBuilder.Create<Percept, string>()
            .WithLayer("nearby", 10, percept => percept.Distance < 1, _ => "stop")
            .Build();

        var result = await agent.RunAsync(new SubsumptionAgentInput<Percept>(new Percept(5)));

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.False(result.Output!.HasAction);
        Assert.Null(result.Output.Action);
        Assert.Null(result.Output.WinningLayer);
    }

    [Fact]
    public void BuilderRequiresAtLeastOneLayerAndUniqueNames()
    {
        Assert.Throws<InvalidOperationException>(() =>
            SubsumptionAgentBuilder.Create<Percept, string>().Build());

        var builder = SubsumptionAgentBuilder.Create<Percept, string>()
            .WithLayer("same", 1, _ => false, _ => "first");

        Assert.Throws<ArgumentException>(() =>
            builder.WithLayer("same", 2, _ => true, _ => "second"));
    }

    [Fact]
    public async Task CancellationBeforeArbitrationReturnsCancelled()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var agent = SubsumptionAgentBuilder.Create<Percept, string>()
            .WithLayer("always", 1, _ => true, _ => "action")
            .Build();

        var result = await agent.RunAsync(new SubsumptionAgentInput<Percept>(new Percept(5)), source.Token);

        Assert.Equal(AgentRunStatus.Cancelled, result.Status);
    }

    private sealed record Percept(double Distance);
}