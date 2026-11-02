using AgentForge.Abstractions.Execution;
using AgentForge.Agents.GoalBased.Builders;
using AgentForge.Agents.GoalBased.Application.UseCases;
using AgentForge.Agents.GoalBased.Domain.Models;
using Xunit;

namespace AgentForge.Agents.GoalBased.Tests;

public sealed class GoalBasedAgentTests
{
    [Fact]
    public async Task FindsShortestPlanWithBreadthFirstSearch()
    {
        var agent = CreateAgent(new Dictionary<string, GoalBasedTransition<string, string>[]>
        {
            ["Home"] = [new("Long route", "A"), new("Short route", "B")],
            ["A"] = [new("Continue long route", "C")],
            ["C"] = [new("Arrive", "Goal")],
            ["B"] = [new("Arrive directly", "Goal")],
            ["Goal"] = []
        });

        var result = await agent.RunAsync(new("Home", "Goal"));

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.Equal(GoalBasedSearchStatus.GoalReached, result.Output?.Status);
        Assert.Equal(new[] { "Short route", "Arrive directly" }, result.Output?.Plan?.Actions);
        Assert.Equal(new[] { "Home", "B", "Goal" }, result.Output?.Plan?.States);
        Assert.Equal(2, result.Output?.Plan?.Depth);
    }

    [Fact]
    public async Task RecognizesWhenInitialStateAlreadySatisfiesGoal()
    {
        var agent = CreateAgent(new Dictionary<string, GoalBasedTransition<string, string>[]>());

        var result = await agent.RunAsync(new("Goal", "Goal"));

        Assert.Equal(GoalBasedSearchStatus.GoalReached, result.Output?.Status);
        Assert.Empty(result.Output!.Plan!.Actions);
        Assert.Equal(new[] { "Goal" }, result.Output.Plan.States);
    }

    [Fact]
    public async Task ReportsExhaustedUnreachableStateSpace()
    {
        var agent = CreateAgent(new Dictionary<string, GoalBasedTransition<string, string>[]>
        {
            ["Start"] = [new("Move", "DeadEnd")],
            ["DeadEnd"] = []
        });

        var result = await agent.RunAsync(new("Start", "Missing"));

        Assert.Equal(GoalBasedSearchStatus.GoalNotReachable, result.Output?.Status);
        Assert.Null(result.Output?.Plan);
    }

    [Fact]
    public async Task ReportsDepthLimitInsteadOfClaimingGoalIsUnreachable()
    {
        var agent = CreateAgent(
            new Dictionary<string, GoalBasedTransition<string, string>[]>
            {
                ["Start"] = [new("Move", "Goal")],
                ["Goal"] = []
            },
            maxDepth: 0);

        var result = await agent.RunAsync(new("Start", "Goal"));

        Assert.Equal(GoalBasedSearchStatus.SearchLimitReached, result.Output?.Status);
    }

    [Fact]
    public async Task ReportsExploredStateLimit()
    {
        var agent = CreateAgent(
            new Dictionary<string, GoalBasedTransition<string, string>[]>
            {
                ["Start"] = [new("A", "A"), new("B", "B")],
                ["A"] = [],
                ["B"] = []
            },
            maxExploredStates: 1);

        var result = await agent.RunAsync(new("Start", "Missing"));

        Assert.Equal(GoalBasedSearchStatus.SearchLimitReached, result.Output?.Status);
    }

    [Fact]
    public async Task UsesSuppliedStateComparer()
    {
        var routes = new Dictionary<string, GoalBasedTransition<string, string>[]>(StringComparer.OrdinalIgnoreCase)
        {
            ["start"] = [new("Finish", "GOAL")],
            ["goal"] = []
        };
        var agent = GoalBasedAgentBuilder.Create<string, string, string>()
            .WithSuccessors(state => routes[state])
            .WithGoalPredicate((state, goal) => StringComparer.OrdinalIgnoreCase.Equals(state, goal))
            .WithStateComparer(StringComparer.OrdinalIgnoreCase)
            .Build();

        var result = await agent.RunAsync(new("START", "goal"));

        Assert.Equal(GoalBasedSearchStatus.GoalReached, result.Output?.Status);
    }

    [Fact]
    public async Task CancellationBeforeSearchReturnsCancelled()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var agent = CreateAgent(new Dictionary<string, GoalBasedTransition<string, string>[]>());

        var result = await agent.RunAsync(new("Start", "Goal"), source.Token);

        Assert.Equal(AgentRunStatus.Cancelled, result.Status);
    }

    [Fact]
    public async Task NullSuccessorCollectionFailsSafely()
    {
        var agent = GoalBasedAgentBuilder.Create<string, string, string>()
            .WithSuccessors(_ => null!)
            .WithGoalPredicate((state, goal) => state == goal)
            .Build();

        var result = await agent.RunAsync(new("Start", "Goal"));

        Assert.Equal(AgentRunStatus.Failed, result.Status);
        Assert.Equal("goal_based.null_successors", result.Failure?.Code);
    }

    [Fact]
    public void BuilderRequiresBothSearchStrategies()
    {
        Assert.Throws<InvalidOperationException>(() =>
            GoalBasedAgentBuilder.Create<string, string, string>()
                .WithSuccessors(_ => Array.Empty<GoalBasedTransition<string, string>>())
                .Build());

        Assert.Throws<InvalidOperationException>(() =>
            GoalBasedAgentBuilder.Create<string, string, string>()
                .WithGoalPredicate((state, goal) => state == goal)
                .Build());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            GoalBasedAgentBuilder.Create<string, string, string>()
                .WithLimits(-1, 10));
    }

    private static AgentHandle<GoalBasedInput<string, string>, GoalBasedOutput<string, string>> CreateAgent(
        IReadOnlyDictionary<string, GoalBasedTransition<string, string>[]> routes,
        int maxDepth = 32,
        int maxExploredStates = 10_000) =>
        GoalBasedAgentBuilder.Create<string, string, string>()
            .WithSuccessors(state => routes.TryGetValue(state, out var transitions)
                ? transitions
                : Array.Empty<GoalBasedTransition<string, string>>())
            .WithGoalPredicate((state, goal) => state == goal)
            .WithLimits(maxDepth, maxExploredStates)
            .Build();
}
