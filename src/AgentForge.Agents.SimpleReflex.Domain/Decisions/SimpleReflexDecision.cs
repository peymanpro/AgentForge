namespace AgentForge.Agents.SimpleReflex.Domain.Decisions;

/// <summary>
/// Represents the outcome of evaluating a simple-reflex policy.
/// </summary>
/// <typeparam name="TAction">The action type.</typeparam>
public sealed record SimpleReflexDecision<TAction> where TAction : notnull
{
    private SimpleReflexDecision(bool hasAction, TAction? action, string explanation)
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
    /// Gets whether a rule selected an action.
    /// </summary>
    public bool HasAction { get; }

    /// <summary>
    /// Gets the selected action. Check <see cref="HasAction"/> before reading this property.
    /// </summary>
    public TAction? Action { get; }

    /// <summary>
    /// Gets the explanation for the decision.
    /// </summary>
    public string Explanation { get; }

    /// <summary>
    /// Creates a decision that proposes an action.
    /// </summary>
    public static SimpleReflexDecision<TAction> Select(TAction action, string explanation)
    {
        ArgumentNullException.ThrowIfNull(action);
        return new SimpleReflexDecision<TAction>(true, action, explanation);
    }

    /// <summary>
    /// Creates a valid decision when no rule applies.
    /// </summary>
    public static SimpleReflexDecision<TAction> NoAction(string explanation)
    {
        return new SimpleReflexDecision<TAction>(false, default, explanation);
    }
}
