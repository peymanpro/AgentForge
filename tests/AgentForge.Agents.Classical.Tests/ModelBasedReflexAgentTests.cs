using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Classical;

namespace AgentForge.Agents.Classical.Tests;

public sealed class ModelBasedReflexAgentTests
{
    [Fact]
    public async Task RunAsync_UpdatesStateBeforeEvaluatingRules()
    {
        var rules = new[]
        {
            new ReflexRule<WorldState, string>(
                "rising-temperature",
                state => state.CurrentTemperature > state.PreviousTemperature,
                _ => "increase-cooling"),
            new ReflexRule<WorldState, string>(
                "falling-temperature",
                state => state.CurrentTemperature < state.PreviousTemperature,
                _ => "reduce-cooling")
        };

        var agent = new ModelBasedReflexAgent<TemperaturePercept, WorldState, string>(
            stateUpdater: (previous, percept) => new WorldState(
                previous.CurrentTemperature,
                percept.Temperature),
            rules);

        var result = await agent.RunAsync(
            new ModelBasedReflexInput<TemperaturePercept, WorldState>(
                new TemperaturePercept(24),
                new WorldState(21, 21)),
            AgentRunContext.Create(),
            new AgentRunOptions());

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.NotNull(result.Output);
        Assert.Equal(24, result.Output.CurrentState.CurrentTemperature);
        Assert.True(result.Output.Decision.HasAction);
        Assert.Equal("increase-cooling", result.Output.Decision.Action);
        Assert.Equal("rising-temperature", result.Output.AppliedRuleName);
    }

    [Fact]
    public async Task RunAsync_PreservesUpdatedStateWhenNoRuleMatches()
    {
        var agent = new ModelBasedReflexAgent<TemperaturePercept, WorldState, string>(
            stateUpdater: (previous, percept) => new WorldState(previous.CurrentTemperature, percept.Temperature),
            [
                new ReflexRule<WorldState, string>("hot", state => state.CurrentTemperature > 30, _ => "cool")
            ]);

        var result = await agent.RunAsync(
            new ModelBasedReflexInput<TemperaturePercept, WorldState>(
                new TemperaturePercept(24),
                new WorldState(22, 22)),
            AgentRunContext.Create(),
            new AgentRunOptions());

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.NotNull(result.Output);
        Assert.Equal(24, result.Output.CurrentState.CurrentTemperature);
        Assert.False(result.Output.Decision.HasAction);
    }

    [Fact]
    public async Task RunAsync_ConvertsStateUpdateExceptionsToInternalFailure()
    {
        var agent = new ModelBasedReflexAgent<TemperaturePercept, WorldState, string>(
            stateUpdater: (_, _) => throw new InvalidOperationException("sensitive detail"),
            [
                new ReflexRule<WorldState, string>("always", _ => true, _ => "act")
            ]);

        var result = await agent.RunAsync(
            new ModelBasedReflexInput<TemperaturePercept, WorldState>(
                new TemperaturePercept(20),
                new WorldState(20, 20)),
            AgentRunContext.Create(),
            new AgentRunOptions());

        Assert.Equal(AgentRunStatus.Failed, result.Status);
        Assert.Equal(AgentFailureCategory.InternalFailure, result.Failure?.Category);
        Assert.DoesNotContain("sensitive detail", result.Failure?.Message ?? string.Empty);
    }

    private sealed record TemperaturePercept(int Temperature);

    private sealed record WorldState(int PreviousTemperature, int CurrentTemperature);
}
