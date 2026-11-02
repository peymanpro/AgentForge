namespace AgentForge.Agents.Classical;

/// <summary>
/// Describes the action selected by a classical decision policy, or the absence of an applicable action.
/// </summary>
/// <typeparam name="TAction">The action type.</typeparam>
public sealed record ClassicalDecision<TAction> where TAction : notnull
{
    private ClassicalDecision(bool hasAction, TAction? action, string explanation)
    {
        if (hasAction && action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (!hasAction && action is not null)
        {
            throw new ArgumentException("A decision without an action cannot contain an action value.", nameof(action));
        }

        if (string.IsNullOrWhiteSpace(explanation))
        {
            throw new ArgumentException("An explanation is required.", nameof(explanation));
        }

        HasAction = hasAction;
        Action = action;
        Explanation = explanation;
    }

    /// <summary>
    /// Gets a value indicating whether an action was selected.
    /// </summary>
    public bool HasAction { get; }

    /// <summary>
    /// Gets the selected action, or the default value when no action was selected.
    /// Check <see cref="HasAction"/> before reading this property.
    /// </summary>
    public TAction? Action { get; }

    /// <summary>
    /// Gets the human-readable reason for the decision.
    /// </summary>
    public string Explanation { get; }

    /// <summary>
    /// Creates a decision that selects an action.
    /// </summary>
    /// <param name="action">The selected action.</param>
    /// <param name="explanation">The reason the action was selected.</param>
    /// <returns>A decision containing the selected action.</returns>
    public static ClassicalDecision<TAction> Select(TAction action, string explanation)
    {
        ArgumentNullException.ThrowIfNull(action);
        return new ClassicalDecision<TAction>(true, action, explanation);
    }

    /// <summary>
    /// Creates a valid decision for which no configured action applies.
    /// </summary>
    /// <param name="explanation">The reason no action was selected.</param>
    /// <returns>A decision without an action.</returns>
    public static ClassicalDecision<TAction> NoAction(string explanation)
    {
        return new ClassicalDecision<TAction>(false, default, explanation);
    }
}
