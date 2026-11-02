using AgentForge.Agents.Learning.Application.UseCases;
using AgentForge.Agents.Learning.Domain.Models;
using AgentForge.Agents.Learning.Builders;
using Xunit;

namespace AgentForge.Agents.Learning.Tests;

public sealed class ContextualLearningAgentTests
{
    [Fact]
    public async Task LinUcbSelectsActionsAccordingToCurrentContext()
    {
        var state = new ContextualLearningState<string>(featureCount: 2, regularization: 1)
            .Observe("A", [1, 0], 1)
            .Observe("B", [0, 1], 1);
        var agent = ContextualLearningAgentBuilder.Create<string>()
            .WithExplorationCoefficient(1)
            .Build();

        var firstContext = await agent.RunAsync(new ContextualLearningAgentInput<string>(["A", "B"], [1, 0], state));
        var secondContext = await agent.RunAsync(new ContextualLearningAgentInput<string>(["A", "B"], [0, 1], state));

        Assert.Equal("A", firstContext.Output!.SelectedAction);
        Assert.Equal("B", secondContext.Output!.SelectedAction);
    }

    [Fact]
    public void ObserveCreatesNewStateAndUpdatesOnlySelectedAction()
    {
        var initial = new ContextualLearningState<string>(2);
        var updated = initial.Observe("A", [1, 2], 0.5);

        Assert.Equal(0, initial.TotalObservations);
        Assert.Equal(1, updated.TotalObservations);
        Assert.Equal(0, initial.GetObservationCount("A"));
        Assert.Equal(1, updated.GetObservationCount("A"));
        Assert.Equal(0, updated.GetObservationCount("B"));
        Assert.Equal(2, updated.GetStatistics("A").DesignMatrix[0, 0]);
        Assert.Equal(2, updated.GetStatistics("A").DesignMatrix[0, 1]);
        Assert.Equal(5, updated.GetStatistics("A").DesignMatrix[1, 1]);
        Assert.Equal(0.5, updated.GetStatistics("A").RewardVector[0]);
        Assert.Equal(1.0, updated.GetStatistics("A").RewardVector[1]);
    }

    [Fact]
    public void UnseenActionReceivesUncertaintyBonus()
    {
        var state = new ContextualLearningState<string>(1).Observe("known", [1], 0.1);
        var input = new ContextualLearningAgentInput<string>(["known", "unseen"], [1], state);
        var result = new SelectContextualLearningAction<string>(explorationCoefficient: 2).Execute(input);

        Assert.Equal("unseen", result.SelectedAction);
        Assert.True(result.ExplorationBonus > 0);
        Assert.True(result.UpperConfidenceScore > result.EstimatedReward);
    }

    [Fact]
    public void RejectsInvalidContextAndRewards()
    {
        var state = new ContextualLearningState<string>(2);
        Assert.Throws<ArgumentException>(() => new ContextualLearningAgentInput<string>(["A"], [1], state));
        Assert.Throws<ArgumentException>(() => new ContextualLearningAgentInput<string>(["A"], [1, double.NaN], state));
        Assert.Throws<ArgumentException>(() => state.Observe("A", [1], 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => state.Observe("A", [1, 2], double.PositiveInfinity));
    }

    [Fact]
    public void RejectsInvalidRegularizationAndExplorationCoefficient()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ContextualLearningState<string>(2, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SelectContextualLearningAction<string>(-0.1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SelectContextualLearningAction<string>(double.NaN));
    }

    [Fact]
    public void RejectsNonPositiveDefinitePersistedStatistics()
    {
        Assert.Throws<ArgumentException>(() => new ContextualActionStatistics(
            new double[,] { { 1, 2 }, { 2, 1 } }, [0, 0]));
    }

    [Fact]
    public void DefensiveCopiesPreventMutationOfStatistics()
    {
        var matrix = new double[,] { { 1 } };
        var vector = new[] { 2.0 };
        var statistics = new ContextualActionStatistics(matrix, vector);
        matrix[0, 0] = 99;
        vector[0] = 99;
        var returned = statistics.DesignMatrix;
        returned[0, 0] = 50;

        Assert.Equal(1, statistics.DesignMatrix[0, 0]);
        Assert.Equal(2, statistics.RewardVector[0]);
    }
}
