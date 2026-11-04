using AgentForge.Abstractions.Execution;
using AgentForge.Agents.RuleBased.Builders;
using AgentForge.Agents.RuleBased.Models;
using Xunit;

namespace AgentForge.Agents.RuleBased.Tests;

public sealed class RuleBasedAgentTests
{
    [Fact]
    public async Task FiresFirstApplicableRuleUntilWorkingMemoryIsStable()
    {
        var agent = RuleBasedAgentBuilder.Create<Counter, string>()
            .When(
                "increment",
                memory => memory.Value < 2,
                _ => "incremented",
                memory => memory with { Value = memory.Value + 1 })
            .Build();

        var result = await agent.RunAsync(new RuleBasedAgentInput<Counter>(new Counter(0)));

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.Equal(2, result.Output!.FinalWorkingMemory.Value);
        Assert.Equal(new[] { "increment", "increment" }, result.Output.FiredRules);
        Assert.Equal(new[] { "incremented", "incremented" }, result.Output.Actions);
        Assert.Equal(RuleBasedTerminationReason.RulesExhausted, result.Output.TerminationReason);
    }

    [Fact]
    public async Task ReturnsPartialTraceWhenFiringBudgetIsReached()
    {
        var agent = RuleBasedAgentBuilder.Create<Counter, string>()
            .When(
                "increment",
                _ => true,
                _ => "incremented",
                memory => memory with { Value = memory.Value + 1 })
            .WithMaxFirings(2)
            .Build();

        var result = await agent.RunAsync(new RuleBasedAgentInput<Counter>(new Counter(0)));

        Assert.Equal(AgentRunStatus.BudgetExhausted, result.Status);
        Assert.Equal("rule_based.max_firings_reached", result.Failure?.Code);
        Assert.Equal(2, result.Output!.FinalWorkingMemory.Value);
        Assert.Equal(2, result.Output.FiredRules.Count);
        Assert.Equal(RuleBasedTerminationReason.MaxFiringsReached, result.Output.TerminationReason);
    }

    [Fact]
    public async Task SharedMaxStepsCanFurtherRestrictTheFiringLimit()
    {
        var agent = RuleBasedAgentBuilder.Create<Counter, string>()
            .When("increment", _ => true, _ => "incremented", memory => memory with { Value = memory.Value + 1 })
            .WithMaxFirings(10)
            .Build();

        var result = await agent.RunAsync(
            new RuleBasedAgentInput<Counter>(new Counter(0)),
            new AgentRunOptions { MaxSteps = 1 });

        Assert.Equal(AgentRunStatus.BudgetExhausted, result.Status);
        Assert.Equal(1, result.Output!.FinalWorkingMemory.Value);
    }

    [Fact]
    public async Task RuleFailurePreservesPartialTraceAndFailureReason()
    {
        var agent = RuleBasedAgentBuilder.Create<Counter, string>()
            .When("throws", _ => true, _ => throw new InvalidOperationException("private details"), memory => memory)
            .Build();

        var result = await agent.RunAsync(new RuleBasedAgentInput<Counter>(new Counter(0)));

        Assert.Equal(AgentRunStatus.Failed, result.Status);
        Assert.Equal("rule_based.execution_failed", result.Failure?.Code);
        Assert.Equal(RuleBasedTerminationReason.ExecutionFailed, result.Output?.TerminationReason);
    }

    [Fact]
    public void BuilderRequiresAtLeastOneRuleAndUniqueNames()
    {
        Assert.Throws<InvalidOperationException>(() =>
            RuleBasedAgentBuilder.Create<Counter, string>().Build());

        var builder = RuleBasedAgentBuilder.Create<Counter, string>()
            .When("same", _ => false, _ => "action", memory => memory);

        Assert.Throws<ArgumentException>(() =>
            builder.When("same", _ => false, _ => "other", memory => memory));
    }

    [Fact]
    public async Task CancellationBeforeExecutionReturnsCancelled()
    {
        using var source = new CancellationTokenSource();
        source.Cancel();
        var agent = RuleBasedAgentBuilder.Create<Counter, string>()
            .When("never", _ => true, _ => "action", memory => memory)
            .Build();

        var result = await agent.RunAsync(new RuleBasedAgentInput<Counter>(new Counter(0)), source.Token);

        Assert.Equal(AgentRunStatus.Cancelled, result.Status);
    }

    private sealed record Counter(int Value);
}