namespace AgentForge.Agents.GoalBased.Domain.Models;

/// <summary>The initial state and target condition for a goal-directed search.</summary>
/// <param name="InitialState">The state from which search begins.</param>
/// <param name="Goal">The target goal value evaluated by the goal predicate.</param>
public sealed record GoalBasedInput<TState, TGoal>(TState InitialState, TGoal Goal)
    where TState : notnull
    where TGoal : notnull;

/// <summary>A state transition proposed by the application's action model.</summary>
/// <param name="Action">The action that would produce the next state.</param>
/// <param name="NextState">The state reached after the proposed action.</param>
public sealed record GoalBasedTransition<TState, TAction>(TAction Action, TState NextState)
    where TState : notnull
    where TAction : notnull;

/// <summary>The terminal outcome of a bounded goal-directed search.</summary>
public enum GoalBasedSearchStatus
{
    /// <summary>A goal-satisfying state was discovered.</summary>
    GoalReached,
    /// <summary>The reachable state space was exhausted without finding a goal.</summary>
    GoalNotReachable,
    /// <summary>A configured search bound prevented a complete search.</summary>
    SearchLimitReached
}

/// <summary>A sequence of proposed actions and states that reaches a goal.</summary>
/// <param name="Actions">The ordered actions from the initial state to the goal.</param>
/// <param name="States">The ordered state path, including initial and goal states.</param>
/// <param name="ExploredStateCount">The number of states removed from the search queue.</param>
/// <param name="Depth">The number of transitions in the plan.</param>
public sealed record GoalBasedPlan<TState, TAction>(
    IReadOnlyList<TAction> Actions,
    IReadOnlyList<TState> States,
    int ExploredStateCount,
    int Depth)
    where TState : notnull
    where TAction : notnull;
