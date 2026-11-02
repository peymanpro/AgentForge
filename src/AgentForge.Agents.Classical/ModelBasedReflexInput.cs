namespace AgentForge.Agents.Classical;

/// <summary>
/// Supplies a percept and the agent's previous explicit internal state.
/// </summary>
/// <typeparam name="TPercept">The current percept type.</typeparam>
/// <typeparam name="TState">The internal state type.</typeparam>
public sealed record ModelBasedReflexInput<TPercept, TState>
    where TPercept : notnull
    where TState : notnull
{
    /// <summary>
    /// Creates a model-based reflex input.
    /// </summary>
    /// <param name="percept">The latest observation from the environment.</param>
    /// <param name="previousState">The state produced by the preceding run, or the caller's initial state.</param>
    public ModelBasedReflexInput(TPercept percept, TState previousState)
    {
        ArgumentNullException.ThrowIfNull(percept);
        ArgumentNullException.ThrowIfNull(previousState);
        Percept = percept;
        PreviousState = previousState;
    }

    /// <summary>
    /// Gets the current percept.
    /// </summary>
    public TPercept Percept { get; }

    /// <summary>
    /// Gets the state carried forward from the previous run.
    /// </summary>
    public TState PreviousState { get; }
}
