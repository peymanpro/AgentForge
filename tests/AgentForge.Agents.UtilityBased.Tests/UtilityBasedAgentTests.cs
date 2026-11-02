using AgentForge.Abstractions.Execution;
using AgentForge.Agents.UtilityBased.Builders;
using AgentForge.Agents.UtilityBased.Domain.Policies;
using Xunit;

namespace AgentForge.Agents.UtilityBased.Tests;

public sealed class UtilityBasedAgentTests
{
    [Fact]
    public async Task SelectsCandidateWithGreatestUtility()
    {
        var agent = CreateAgent(
            _ => ["slow", "balanced", "fast"],
            (_, action) => action switch { "slow" => 1, "balanced" => 5, _ => 3 });

        var result = await agent.RunAsync("task");

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.Equal("balanced", result.Output?.Decision.Action);
        Assert.Equal(5, result.Output?.Decision.Utility);
        Assert.Equal(3, result.Output?.Evaluations.Count);
    }

    [Fact]
    public async Task SupportsNegativeAndZeroUtilities()
    {
        var agent = CreateAgent(_ => ["negative", "zero"], (_, action) => action == "negative" ? -2 : 0);

        var result = await agent.RunAsync("task");

        Assert.Equal("zero", result.Output?.Decision.Action);
        Assert.Equal(0, result.Output?.Decision.Utility);
    }

    [Fact]
    public async Task FirstCandidateWinsByDefaultWhenMaximumIsTied()
    {
        var agent = CreateAgent(_ => ["first", "second"], (_, _) => 7);

        var result = await agent.RunAsync("task");

        Assert.Equal("first", result.Output?.Decision.Action);
    }

    [Fact]
    public async Task LastCandidatePolicySelectsLastMaximum()
    {
        var agent = CreateAgent(
            _ => ["first", "middle", "last"],
            (_, action) => action == "middle" ? 1 : 9,
            UtilityTieBreakPolicy.LastCandidate);

        var result = await agent.RunAsync("task");

        Assert.Equal("last", result.Output?.Decision.Action);
    }

    [Fact]
    public async Task RejectAmbiguousReturnsNoActionAndPreservesEvaluations()
    {
        var agent = CreateAgent(
            _ => ["first", "second"],
            (_, _) => 4,
            UtilityTieBreakPolicy.RejectAmbiguous);

        var result = await agent.RunAsync("task");

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.False(result.Output!.Decision.HasAction);
        Assert.Contains("share the maximum utility", result.Output.Decision.Explanation);
        Assert.Equal(2, result.Output.Evaluations.Count);
    }

    [Fact]
    public async Task EmptyCandidateSetReturnsSuccessfulNoAction()
    {
        var agent = CreateAgent(_ => Array.Empty<string>(), (_, _) => throw new InvalidOperationException());

        var result = await agent.RunAsync("task");

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.False(result.Output!.Decision.HasAction);
        Assert.Empty(result.Output.Evaluations);
    }

    [Fact]
    public async Task CandidateProviderRunsOnceAndEachCandidateIsEvaluatedOnceInOrder()
    {
        var providerCalls = 0;
        var evaluated = new List<string>();
        var agent = CreateAgent(
            _ =>
            {
                providerCalls++;
                return ["a", "b", "c"];
            },
            (_, action) =>
            {
                evaluated.Add(action);
                return action.Length;
            });

        await agent.RunAsync("task");

        Assert.Equal(1, providerCalls);
        Assert.Equal(new[] { "a", "b", "c" }, evaluated);
    }

    [Fact]
    public async Task NonFiniteUtilityFailsRun()
    {
        var agent = CreateAgent(_ => ["a"], (_, _) => double.NaN);

        var result = await agent.RunAsync("task");

        Assert.Equal(AgentRunStatus.Failed, result.Status);
        Assert.Equal("utility_based.non_finite_utility", result.Failure?.Code);
    }

    [Fact]
    public async Task EvaluatorExceptionIsNormalized()
    {
        var agent = CreateAgent(_ => ["a"], (_, _) => throw new InvalidOperationException("private detail"));

        var result = await agent.RunAsync("task");

        Assert.Equal(AgentRunStatus.Failed, result.Status);
        Assert.DoesNotContain("private detail", result.Failure?.Message);
    }

    [Fact]
    public async Task CancellationBeforeRunReturnsCancelled()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var agent = CreateAgent(_ => ["a"], (_, _) => 1);

        var result = await agent.RunAsync("task", source.Token);

        Assert.Equal(AgentRunStatus.Cancelled, result.Status);
    }

    [Fact]
    public void BuilderRequiresCandidateProviderAndUtilityEvaluator()
    {
        Assert.Throws<InvalidOperationException>(() =>
            UtilityBasedAgentBuilder.Create<string, string>()
                .WithUtility((_, _) => 1)
                .Build());

        Assert.Throws<InvalidOperationException>(() =>
            UtilityBasedAgentBuilder.Create<string, string>()
                .WithCandidates(_ => ["a"])
                .Build());
    }

    [Fact]
    public void BuildReturnsCommonAgentHandle()
    {
        var agent = CreateAgent(_ => ["a"], (_, _) => 1);

        Assert.IsAssignableFrom<IAgent<string, AgentForge.Agents.UtilityBased.Application.UseCases.UtilityBasedOutput<string>>>(agent);
    }


    [Fact]
    public async Task NullCandidateCollectionFailsRun()
    {
        var agent = CreateAgent(_ => null!, (_, _) => 1);

        var result = await agent.RunAsync("task");

        Assert.Equal(AgentRunStatus.Failed, result.Status);
        Assert.Equal("utility_based.null_candidates", result.Failure?.Code);
    }

    [Fact]
    public async Task NullCandidateEntryFailsRun()
    {
        var agent = CreateAgent(_ => new string[] { null! }, (_, _) => 1);

        var result = await agent.RunAsync("task");

        Assert.Equal(AgentRunStatus.Failed, result.Status);
        Assert.Equal("utility_based.null_candidate", result.Failure?.Code);
    }

    [Fact]
    public async Task CandidateProviderExceptionIsNormalized()
    {
        var agent = CreateAgent(_ => throw new InvalidOperationException("private detail"), (_, _) => 1);

        var result = await agent.RunAsync("task");

        Assert.Equal(AgentRunStatus.Failed, result.Status);
        Assert.Equal("utility_based.evaluation_failed", result.Failure?.Code);
        Assert.DoesNotContain("private detail", result.Failure?.Message);
    }

    [Fact]
    public async Task InvalidRunOptionsAreRejected()
    {
        var agent = CreateAgent(_ => ["a"], (_, _) => 1);

        var result = await agent.RunAsync("task", new AgentRunOptions { MaxSteps = 0 });

        Assert.Equal(AgentRunStatus.Rejected, result.Status);
        Assert.Equal(AgentFailureCategory.InvalidInput, result.Failure?.Category);
    }

    [Fact]
    public async Task CancellationBetweenCandidateEvaluationsStopsRun()
    {
        using var source = new CancellationTokenSource();
        var agent = CreateAgent(
            _ => ["first", "second"],
            (_, action) =>
            {
                if (action == "first")
                {
                    source.Cancel();
                }

                return 1;
            });

        var result = await agent.RunAsync("task", source.Token);

        Assert.Equal(AgentRunStatus.Cancelled, result.Status);
    }

    [Fact]
    public async Task ConcurrentRunsKeepTheirOutputsIndependent()
    {
        var agent = CreateAgent(
            _ => ["one", "two"],
            (_, action) => action == "one" ? 1 : 2);

        var results = await Task.WhenAll(
            agent.RunAsync("first"),
            agent.RunAsync("second"));

        Assert.All(results, result => Assert.Equal(AgentRunStatus.Completed, result.Status));
        Assert.Equal("two", results[0].Output?.Decision.Action);
        Assert.Equal("two", results[1].Output?.Decision.Action);
        Assert.NotSame(results[0].Output, results[1].Output);
    }

    private static AgentHandle<string, AgentForge.Agents.UtilityBased.Application.UseCases.UtilityBasedOutput<string>> CreateAgent(
        Func<string, IEnumerable<string>> candidates,
        Func<string, string, double> utility,
        UtilityTieBreakPolicy policy = UtilityTieBreakPolicy.FirstCandidate)
    {
        return UtilityBasedAgentBuilder
            .Create<string, string>()
            .WithCandidates(candidates)
            .WithUtility(utility)
            .WithTieBreakPolicy(policy)
            .Build();
    }
}