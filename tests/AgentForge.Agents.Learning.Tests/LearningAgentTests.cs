using AgentForge.Agents.Learning.Builders;
using AgentForge.Agents.Learning.Domain.Models;
namespace AgentForge.Agents.Learning.Tests;
public sealed class LearningAgentTests
{
    [Fact]
    public async Task SelectsUnobservedActionsBeforeExploitation()
    {
        var agent = LearningAgentBuilder.Create<string>().WithEpsilonGreedy(epsilon: 0).Build();
        var state = new LearningState<string>([new("A", new ActionStatistics(3, 0.4))]);
        var result = await agent.RunAsync(new LearningAgentInput<string>(["A", "B"], state));
        Assert.Equal("B", result.Output!.SelectedAction);
        Assert.True(result.Output.WasExploration);
    }
    [Fact]
    public async Task ExploitsActionWithHighestAverageReward()
    {
        var agent = LearningAgentBuilder.Create<string>().WithEpsilonGreedy(0, () => 0.9).Build();
        var state = new LearningState<string>([new("A", new ActionStatistics(5, 0.3)), new("B", new ActionStatistics(5, 0.8))]);
        var result = await agent.RunAsync(new LearningAgentInput<string>(["A", "B"], state));
        Assert.Equal("B", result.Output!.SelectedAction);
        Assert.False(result.Output.WasExploration);
        Assert.Equal(10, result.Output.TotalObservations);
    }
    [Fact]
    public async Task ExplorationCanSelectANonGreedyAction()
    {
        var agent = LearningAgentBuilder.Create<string>().WithEpsilonGreedy(1, () => 0, _ => 1).Build();
        var state = new LearningState<string>([new("A", new ActionStatistics(5, 0.9)), new("B", new ActionStatistics(5, 0.1))]);
        var result = await agent.RunAsync(new LearningAgentInput<string>(["A", "B"], state));
        Assert.Equal("B", result.Output!.SelectedAction);
        Assert.True(result.Output.WasExploration);
    }
    [Fact]
    public void ObserveReturnsNewImmutableStateAndUpdatesRunningMean()
    {
        var original = new LearningState<string>();
        var updated = original.Observe("A", 1).Observe("A", 0.5);
        Assert.Empty(original.Statistics);
        Assert.Equal(2, updated.GetStatistics("A").ObservationCount);
        Assert.Equal(0.75, updated.GetStatistics("A").AverageReward, 10);
    }
    [Fact]
    public void RejectsNonFiniteRewards()
    {
        var state = new LearningState<string>();
        Assert.Throws<ArgumentOutOfRangeException>(() => state.Observe("A", double.NaN));
        Assert.Throws<ArgumentOutOfRangeException>(() => state.Observe("A", double.PositiveInfinity));
    }
    [Fact]
    public async Task LearnsToPreferConsistentlyBetterActionAcrossOneThousandRounds()
    {
        var agent = LearningAgentBuilder.Create<string>().WithEpsilonGreedy(0.1).Build();
        var state = new LearningState<string>();
        var preferredSelections = 0;
        for (var round = 0; round < 1_000; round++)
        {
            var result = await agent.RunAsync(new LearningAgentInput<string>(["preferred", "weak"], state));
            var action = result.Output!.SelectedAction;
            if (action == "preferred") preferredSelections++;
            state = state.Observe(action, action == "preferred" ? 1 : 0.1);
        }
        Assert.True(preferredSelections > 850, $"Preferred action selected {preferredSelections} times.");
        Assert.True(state.GetStatistics("preferred").AverageReward > state.GetStatistics("weak").AverageReward);
    }
    [Fact]
    public void RejectsInvalidEpsilon()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => LearningAgentBuilder.Create<string>().WithEpsilonGreedy(1.1));
    }
}