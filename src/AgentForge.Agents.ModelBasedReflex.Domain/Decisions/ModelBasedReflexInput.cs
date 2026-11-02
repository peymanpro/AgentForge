namespace AgentForge.Agents.ModelBasedReflex.Domain.Decisions;

/// <summary>
/// Supplies the latest percept and previously retained internal state to one model-based reflex decision.
/// </summary>
public sealed record ModelBasedReflexInput<TPercept, TState>
    where TPercept : notnull
    where TState : notnull
{
    /// <summary>
    /// Creates the input for one state update and decision step.
    /// </summary>
    public ModelBasedReflexInput(TPercept percept, TState previousState)
    {
        ArgumentNullException.ThrowIfNull(percept);
        ArgumentNullException.ThrowIfNull(previousState);
        Percept = percept;
        PreviousState = previousState;
    }

    /// <summary>
    /// Gets the latest observation from the environment.
    /// </summary>
    public TPercept Percept { get; }

    /// <summary>
    /// Gets the state carried forward from the preceding run or supplied by the host.
    /// </summary>
    public TState PreviousState { get; }
}
