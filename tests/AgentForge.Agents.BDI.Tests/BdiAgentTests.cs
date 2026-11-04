using AgentForge.Abstractions.Execution;
using AgentForge.Agents.BDI.Builders;
using AgentForge.Agents.BDI.Models;
using Xunit;

namespace AgentForge.Agents.BDI.Tests;

public sealed class BdiAgentTests
{
    [Fact]
    public async Task RunsBeliefDesireIntentionAndActionCycle()
    {
        var agent = CreateAgent(maxIntentions: 8);
        var result = await agent.RunAsync(new BdiAgentInput<string, string, string, string>(
            ["door-closed"],
            ["close-door", "open-window"],
            ["stay-safe", "stale-intention"],
            "door-open"));

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.Contains("door-open", result.Output!.Beliefs);
        Assert.DoesNotContain("door-closed", result.Output.Beliefs);
        Assert.Equal(new[] { "close-door" }, result.Output.EligibleDesires);
        Assert.Equal(new[] { "stay-safe", "close-door" }, result.Output.Intentions);
        Assert.True(result.Output.ProposedAction.HasAction);
        Assert.Equal("close-door", result.Output.ProposedAction.Action);
    }

    [Fact]
    public async Task DropsIntentionsThatAreNoLongerCommitted()
    {
        var agent = CreateAgent();
        var result = await agent.RunAsync(new BdiAgentInput<string, string, string, string>(
            ["door-closed"],
            [],
            ["stale-intention"],
            "no-change"));

        Assert.Empty(result.Output!.Intentions);
        Assert.False(result.Output.ProposedAction.HasAction);
        Assert.Null(result.Output.ProposedAction.Action);
    }

    [Fact]
    public async Task BoundsRetainedAndNewIntentions()
    {
        var agent = CreateAgent(maxIntentions: 2);
        var result = await agent.RunAsync(new BdiAgentInput<string, string, string, string>(
            ["door-open"],
            ["close-door", "open-window"],
            ["stay-safe"],
            "door-open"));

        Assert.Equal(2, result.Output!.Intentions.Count);
        Assert.Equal(new[] { "stay-safe", "close-door" }, result.Output.Intentions);
    }

    [Fact]
    public async Task PreservesCommittedIntentionsWithoutNewDesires()
    {
        var agent = CreateAgent();
        var result = await agent.RunAsync(new BdiAgentInput<string, string, string, string>(
            ["door-open"],
            [],
            ["stay-safe"],
            "door-open"));

        Assert.Equal(new[] { "stay-safe" }, result.Output!.Intentions);
        Assert.False(result.Output.ProposedAction.HasAction);
    }

    [Fact]
    public void BuilderRequiresEveryBdiStrategy()
    {
        Assert.Throws<InvalidOperationException>(() =>
            BdiAgentBuilder.Create<string, string, string, string, string>().Build());
    }

    [Fact]
    public async Task CancellationBeforeDeliberationReturnsCancelled()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var agent = CreateAgent();

        var result = await agent.RunAsync(new BdiAgentInput<string, string, string, string>(
            [], [], [], "observation"), source.Token);

        Assert.Equal(AgentRunStatus.Cancelled, result.Status);
    }

    [Fact]
    public async Task StrategyFailureReturnsNormalizedFailure()
    {
        var agent = BdiAgentBuilder.Create<string, string, string, string, string>()
            .WithBeliefRevision((_, _) => throw new InvalidOperationException("private detail"))
            .WithDesireSelection((_, desires) => desires)
            .WithIntentionFactory(desire => desire)
            .WithCommitmentPolicy((_, _) => true)
            .WithActionSelection((_, _) => BdiAction<string>.None)
            .Build();

        var result = await agent.RunAsync(new BdiAgentInput<string, string, string, string>(
            [], [], [], "observation"));

        Assert.Equal(AgentRunStatus.Failed, result.Status);
        Assert.Equal("bdi.deliberation_failed", result.Failure?.Code);
    }

    private static AgentForge.Abstractions.Execution.AgentHandle<
        BdiAgentInput<string, string, string, string>,
        BdiAgentOutput<string, string, string, string>> CreateAgent(int maxIntentions = 16) =>
        BdiAgentBuilder.Create<string, string, string, string, string>()
            .WithBeliefRevision((beliefs, observation) => beliefs
                .Where(belief => belief != "door-closed")
                .Append(observation)
                .Distinct()
                .ToArray())
            .WithDesireSelection((beliefs, desires) => beliefs.Contains("door-open")
                ? desires.Where(desire => desire == "close-door").ToArray()
                : Array.Empty<string>())
            .WithIntentionFactory(desire => desire)
            .WithCommitmentPolicy((beliefs, intention) =>
                intention == "stay-safe" || beliefs.Contains("door-open") && intention == "close-door")
            .WithActionSelection((_, intentions) =>
                intentions.Contains("close-door")
                    ? BdiAction<string>.From("close-door")
                    : BdiAction<string>.None)
            .WithMaxIntentions(maxIntentions)
            .Build();
}