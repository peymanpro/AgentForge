namespace AgentForge.Agents.ModelBasedReflex.Domain.Decisions;

/// <summary>
/// Contains the state produced by the latest percept and the action selected from that state.
/// </summary>
public sealed record ModelBasedReflexOutput<TState, TAction>
    where TState : notnull
    where TAction : notnull
{
    /// <summary>
    /// Creates a model-based reflex output.
    /// </summary>
    public ModelBasedReflexOutput(
        TState currentState,
        ModelBasedReflexDecision<TAction> decision,
        string? matchedRuleName)
    {
        ArgumentNullException.ThrowIfNull(currentState);
        ArgumentNullException.ThrowIfNull(decision);
        CurrentState = currentState;
        Decision = decision;
        MatchedRuleName = matchedRuleName;
    }

    /// <summary>
    /// Gets the updated internal state. The host owns persistence and state lifetime.
    /// </summary>
    public TState CurrentState { get; }

    /// <summary>
    /// Gets the selected action or no-action decision.
    /// </summary>
    public ModelBasedReflexDecision<TAction> Decision { get; }

    /// <summary>
    /// Gets the matched rule name, or null when no rule matched.
    /// </summary>
    public string? MatchedRuleName { get; }
}
