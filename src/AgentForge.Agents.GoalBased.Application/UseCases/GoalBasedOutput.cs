using AgentForge.Agents.GoalBased.Domain.Models;

namespace AgentForge.Agents.GoalBased.Application.UseCases;

/// <summary>The result of a bounded goal-directed search.</summary>
/// <param name="Status">The terminal search outcome.</param>
/// <param name="Plan">The goal-reaching plan, or null when no plan was found.</param>
/// <param name="Explanation">A concise explanation of the search outcome.</param>
public sealed record GoalBasedOutput<TState, TAction>(
    GoalBasedSearchStatus Status,
    GoalBasedPlan<TState, TAction>? Plan,
    string Explanation)
    where TState : notnull
    where TAction : notnull;
