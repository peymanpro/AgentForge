using AgentForge.Abstractions.Execution;
using AgentForge.Agents.ModelBasedReflex.Builders;
using AgentForge.Agents.ModelBasedReflex.Domain.Decisions;
using Xunit;

namespace AgentForge.Agents.ModelBasedReflex.Tests;

public sealed class ModelBasedReflexAgentBuilderTests
{
    [Fact]
    public async Task Build_UpdatesStateBeforeEvaluatingRules()
    {
        var agent = ModelBasedReflexAgentBuilder
            .Create<TemperaturePercept, TemperatureState, string>()
            .WithStateUpdater((previous, percept) => new TemperatureState(
                PreviousTemperature: previous.CurrentTemperature,
                CurrentTemperature: percept.Temperature,
                TargetTemperature: percept.TargetTemperature))
            .When(
                "temperature-rising",
                state => state.CurrentTemperature > state.PreviousTemperature,
                "increase-cooling")
            .When(
                "temperature-falling",
                state => state.CurrentTemperature < state.PreviousTemperature,
                "reduce-cooling")
            .Build();

        var result = await agent.RunAsync(
            new ModelBasedReflexInput<TemperaturePercept, TemperatureState>(
                new TemperaturePercept(24, 22),
                new TemperatureState(21, 21, 22)));

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.NotNull(result.Output);
        Assert.Equal(24, result.Output.CurrentState.CurrentTemperature);
        Assert.Equal("increase-cooling", result.Output.Decision.Action);
        Assert.Equal("temperature-rising", result.Output.MatchedRuleName);
    }

    [Fact]
    public async Task Build_ReturnsUpdatedStateWhenNoRuleMatches()
    {
        var agent = ModelBasedReflexAgentBuilder
            .Create<TemperaturePercept, TemperatureState, string>()
            .WithStateUpdater((previous, percept) => new TemperatureState(
                previous.CurrentTemperature,
                percept.Temperature,
                percept.TargetTemperature))
            .When("too-hot", state => state.CurrentTemperature > 30, "cool")
            .Build();

        var result = await agent.RunAsync(
            new ModelBasedReflexInput<TemperaturePercept, TemperatureState>(
                new TemperaturePercept(24, 22),
                new TemperatureState(22, 22, 22)));

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.NotNull(result.Output);
        Assert.Equal(24, result.Output.CurrentState.CurrentTemperature);
        Assert.False(result.Output.Decision.HasAction);
    }

    [Fact]
    public async Task Build_ConvertsStateUpdaterExceptionsToNormalizedFailures()
    {
        var agent = ModelBasedReflexAgentBuilder
            .Create<TemperaturePercept, TemperatureState, string>()
            .WithStateUpdater((_, _) => throw new InvalidOperationException("private failure"))
            .When("always", _ => true, "act")
            .Build();

        var result = await agent.RunAsync(
            new ModelBasedReflexInput<TemperaturePercept, TemperatureState>(
                new TemperaturePercept(20, 22),
                new TemperatureState(20, 20, 22)));

        Assert.Equal(AgentRunStatus.Failed, result.Status);
        Assert.Equal(AgentFailureCategory.InternalFailure, result.Failure?.Category);
        Assert.DoesNotContain("private failure", result.Failure?.Message ?? string.Empty);
    }

    [Fact]
    public void Build_RequiresAStateUpdater()
    {
        var builder = ModelBasedReflexAgentBuilder
            .Create<TemperaturePercept, TemperatureState, string>()
            .When("always", _ => true, "act");

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    private sealed record TemperaturePercept(int Temperature, int TargetTemperature);

    private sealed record TemperatureState(
        int PreviousTemperature,
        int CurrentTemperature,
        int TargetTemperature);
}
