using AgentForge.Abstractions.Execution;
using AgentForge.Agents.RuleBased.Models;

namespace AgentForge.Agents.RuleBased;

internal sealed record ProductionRule<TMemory, TAction>(
    string Name,
    Func<TMemory, bool> Condition,
    Func<TMemory, TAction> ActionFactory,
    Func<TMemory, TMemory> Apply)
    where TMemory : notnull
    where TAction : notnull;

internal sealed class RuleBasedAgent<TMemory, TAction>(
    IReadOnlyList<ProductionRule<TMemory, TAction>> rules,
    int maxFirings,
    TimeProvider timeProvider)
    : IAgent<RuleBasedAgentInput<TMemory>, RuleBasedAgentOutput<TMemory, TAction>>
    where TMemory : notnull
    where TAction : notnull
{
    public Task<AgentRunResult<RuleBasedAgentOutput<TMemory, TAction>>> RunAsync(
        RuleBasedAgentInput<TMemory> input,
        AgentRunContext context,
        AgentRunOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        var startedAt = timeProvider.GetUtcNow();
        var memory = input.WorkingMemory;
        var firedRules = new List<string>();
        var actions = new List<TAction>();
        var firingLimit = Math.Min(maxFirings, options.MaxSteps ?? maxFirings);

        try
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var applicableRule = rules.FirstOrDefault(rule => rule.Condition(memory));

                if (applicableRule is null)
                {
                    var output = CreateOutput(RuleBasedTerminationReason.RulesExhausted);
                    return Task.FromResult(AgentRunResult<RuleBasedAgentOutput<TMemory, TAction>>.Completed(
                        output, startedAt, timeProvider.GetUtcNow()));
                }

                if (firedRules.Count >= firingLimit)
                {
                    var output = CreateOutput(RuleBasedTerminationReason.MaxFiringsReached);
                    return Task.FromResult(AgentRunResult<RuleBasedAgentOutput<TMemory, TAction>>.Stopped(
                        AgentRunStatus.BudgetExhausted,
                        new AgentFailure(
                            AgentFailureCategory.BudgetExhausted,
                            "rule_based.max_firings_reached",
                            "The production-system firing limit was reached while an applicable rule remained."),
                        startedAt,
                        timeProvider.GetUtcNow(),
                        output));
                }

                actions.Add(applicableRule.ActionFactory(memory));
                memory = applicableRule.Apply(memory);
                firedRules.Add(applicableRule.Name);
            }
        }
        catch (OperationCanceledException)
        {
            return Task.FromResult(AgentRunResult<RuleBasedAgentOutput<TMemory, TAction>>.Stopped(
                AgentRunStatus.Cancelled,
                new AgentFailure(
                    AgentFailureCategory.Cancelled,
                    "rule_based.cancelled",
                    "The production-system run was cancelled."),
                startedAt,
                timeProvider.GetUtcNow(),
                CreateOutput(RuleBasedTerminationReason.Cancelled)));
        }
        catch (Exception)
        {
            return Task.FromResult(AgentRunResult<RuleBasedAgentOutput<TMemory, TAction>>.Stopped(
                AgentRunStatus.Failed,
                new AgentFailure(
                    AgentFailureCategory.InternalFailure,
                    "rule_based.execution_failed",
                    "A production rule failed during evaluation or execution."),
                startedAt,
                timeProvider.GetUtcNow(),
                CreateOutput(RuleBasedTerminationReason.ExecutionFailed)));
        }

        RuleBasedAgentOutput<TMemory, TAction> CreateOutput(RuleBasedTerminationReason reason) =>
            new(memory, firedRules.AsReadOnly(), actions.AsReadOnly(), reason);
    }
}