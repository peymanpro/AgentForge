namespace AgentForge.Agents.GoalBased.Domain.Models;

/// <summary>The initial state and target condition for a goal-directed search.</summary>
public sealed record GoalBasedInput<TState, TGoal>(TState InitialState, TGoal Goal)
    where TState : notnull
    where TGoal : notnull;

/// <summary>A state transition proposed by the application's action model.</summary>
public sealed record GoalBasedTransition<TState, TAction>(TAction Action, TState NextState)
    where TState : notnull
    where TAction : notnull;

/// <summary>The terminal outcome of a bounded goal-directed search.</summary>
public enum GoalBasedSearchStatus
{
    GoalReached,
    GoalNotReachable,
    SearchLimitReached
}

/// <summary>A sequence of proposed actions and states that reaches a goal.</summary>
public sealed record GoalBasedPlan<TState, TAction>(
    IReadOnlyList<TAction> Actions,
    IReadOnlyList<TState> States,
    int ExploredStateCount,
    int Depth)
    where TState : notnull
    where TAction : notnull;
