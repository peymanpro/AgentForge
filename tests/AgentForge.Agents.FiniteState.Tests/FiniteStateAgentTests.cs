using AgentForge.Abstractions.Execution;
using AgentForge.Agents.FiniteState.Builders;
using AgentForge.Agents.FiniteState.Models;
using Xunit;

namespace AgentForge.Agents.FiniteState.Tests;

public sealed class FiniteStateAgentTests
{
    [Fact]
    public async Task AppliesFirstMatchingTransitionAndReturnsNextState()
    {
        var agent = FiniteStateAgentBuilder.Create<DoorState, DoorEvent, string>()
            .When("open", (state, trigger) => state == DoorState.Closed && trigger == DoorEvent.Open,
                (_, _) => DoorState.Open, (_, _) => "unlock")
            .When("close", (state, trigger) => state == DoorState.Open && trigger == DoorEvent.Close,
                (_, _) => DoorState.Closed, (_, _) => "lock")
            .Build();

        var result = await agent.RunAsync(new(DoorState.Closed, DoorEvent.Open));

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.True(result.Output!.Transitioned);
        Assert.Equal("open", result.Output.TransitionName);
        Assert.Equal(DoorState.Open, result.Output.CurrentState);
        Assert.Equal("unlock", result.Output.Action);
    }

    [Fact]
    public async Task UnhandledEventPreservesCurrentState()
    {
        var agent = FiniteStateAgentBuilder.Create<DoorState, DoorEvent, string>()
            .When("open", (state, trigger) => state == DoorState.Closed && trigger == DoorEvent.Open,
                (_, _) => DoorState.Open, (_, _) => "unlock")
            .Build();

        var result = await agent.RunAsync(new(DoorState.Open, DoorEvent.Open));

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.False(result.Output!.Transitioned);
        Assert.Equal(DoorState.Open, result.Output.CurrentState);
        Assert.Null(result.Output.Action);
    }

    [Fact]
    public async Task FirstMatchingTransitionWins()
    {
        var agent = FiniteStateAgentBuilder.Create<DoorState, DoorEvent, string>()
            .When("first", (_, _) => true, (_, _) => DoorState.Open, (_, _) => "first")
            .When("second", (_, _) => true, (_, _) => DoorState.Closed, (_, _) => "second")
            .Build();

        var result = await agent.RunAsync(new(DoorState.Closed, DoorEvent.Open));

        Assert.Equal("first", result.Output?.TransitionName);
    }

    [Fact]
    public void BuilderRequiresTransitionsAndUniqueNames()
    {
        Assert.Throws<InvalidOperationException>(() =>
            FiniteStateAgentBuilder.Create<DoorState, DoorEvent, string>().Build());

        var builder = FiniteStateAgentBuilder.Create<DoorState, DoorEvent, string>()
            .When("same", (_, _) => false, (_, _) => DoorState.Open, (_, _) => "one");

        Assert.Throws<ArgumentException>(() =>
            builder.When("same", (_, _) => true, (_, _) => DoorState.Closed, (_, _) => "two"));
    }

    [Fact]
    public async Task CancellationBeforeTransitionReturnsCancelled()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var agent = FiniteStateAgentBuilder.Create<DoorState, DoorEvent, string>()
            .When("open", (_, _) => true, (_, _) => DoorState.Open, (_, _) => "unlock")
            .Build();

        var result = await agent.RunAsync(new(DoorState.Closed, DoorEvent.Open), source.Token);

        Assert.Equal(AgentRunStatus.Cancelled, result.Status);
    }

    private enum DoorState { Closed, Open }
    private enum DoorEvent { Open, Close }
}