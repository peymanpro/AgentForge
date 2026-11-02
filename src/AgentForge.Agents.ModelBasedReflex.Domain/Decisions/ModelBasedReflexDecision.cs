namespace AgentForge.Agents.ModelBasedReflex.Domain.Decisions;

/// <summary>
/// Represents the action selected after updating a model-based agent's internal state.
/// </summary>
public sealed record ModelBasedReflexDecision<TAction> where TAction : notnull
{
    private ModelBasedReflexDecision(bool hasAction, TAction? action, string explanation)
    {
        if (hasAction && action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (!hasAction && action is not null)
        {
            throw new ArgumentException("A no-action decision cannot contain an action.", nameof(action));
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
    /// Gets whether an action was selected.
    /// </summary>
    public bool HasAction { get; }

    /// <summary>
    /// Gets the proposed action. Check <see cref="HasAction"/> before reading this property.
    /// </summary>
    public TAction? Action { get; }

    /// <summary>
    /// Gets the explanation for the decision.
    /// </summary>
    public string Explanation { get; }

    /// <summary>
    /// Creates a decision that proposes an action.
    /// </summary>
    public static ModelBasedReflexDecision<TAction> Select(TAction action, string explanation)
    {
        ArgumentNullException.ThrowIfNull(action);
        return new ModelBasedReflexDecision<TAction>(true, action, explanation);
    }

    /// <summary>
    /// Creates a valid decision when no rule applies to the updated state.
    /// </summary>
    public static ModelBasedReflexDecision<TAction> NoAction(string explanation)
    {
        return new ModelBasedReflexDecision<TAction>(false, default, explanation);
    }
}
