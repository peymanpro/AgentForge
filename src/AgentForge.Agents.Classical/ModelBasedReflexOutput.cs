namespace AgentForge.Agents.Classical;

/// <summary>
/// Contains the updated internal state and the decision made from that state.
/// </summary>
/// <typeparam name="TState">The internal state type.</typeparam>
/// <typeparam name="TAction">The action type.</typeparam>
public sealed record ModelBasedReflexOutput<TState, TAction>
    where TState : notnull
    where TAction : notnull
{
    /// <summary>
    /// Creates a model-based reflex output.
    /// </summary>
    /// <param name="currentState">The updated state after applying the latest percept.</param>
    /// <param name="decision">The selected action or no-action decision.</param>
    /// <param name="appliedRuleName">The matched rule, if any.</param>
    public ModelBasedReflexOutput(
        TState currentState,
        ClassicalDecision<TAction> decision,
        string? appliedRuleName)
    {
        ArgumentNullException.ThrowIfNull(currentState);
        ArgumentNullException.ThrowIfNull(decision);
        CurrentState = currentState;
        Decision = decision;
        AppliedRuleName = appliedRuleName;
    }

    /// <summary>
    /// Gets the newly updated internal state.
    /// </summary>
    public TState CurrentState { get; }

    /// <summary>
    /// Gets the selected action or no-action decision.
    /// </summary>
    public ClassicalDecision<TAction> Decision { get; }

    /// <summary>
    /// Gets the matched rule name, or <see langword="null"/> when no rule applied.
    /// </summary>
    public string? AppliedRuleName { get; }
}
