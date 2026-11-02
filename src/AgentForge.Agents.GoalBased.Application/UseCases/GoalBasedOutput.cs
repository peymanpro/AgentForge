using AgentForge.Agents.GoalBased.Domain.Models;

namespace AgentForge.Agents.GoalBased.Application.UseCases;

/// <summary>The result of a bounded goal-directed search.</summary>
public sealed record GoalBasedOutput<TState, TAction>(
    GoalBasedSearchStatus Status,
    GoalBasedPlan<TState, TAction>? Plan,
    string Explanation)
    where TState : notnull
    where TAction : notnull;
