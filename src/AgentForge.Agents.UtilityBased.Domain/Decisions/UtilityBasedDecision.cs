namespace AgentForge.Agents.UtilityBased.Domain.Decisions;

/// <summary>
/// Represents the selected action, or explains why no action was selected.
/// </summary>
/// <typeparam name="TAction">The action type.</typeparam>
public sealed record UtilityBasedDecision<TAction> where TAction : notnull
{
    private UtilityBasedDecision(bool hasAction, TAction? action, double? utility, string explanation)
    {
        if (hasAction && (action is null || utility is null || !double.IsFinite(utility.Value)))
        {
            throw new ArgumentException("A selected action requires a non-null action and a finite utility score.");
        }

        if (!hasAction && (action is not null || utility is not null))
        {
            throw new ArgumentException("A no-action decision cannot contain an action or utility score.");
        }

        if (string.IsNullOrWhiteSpace(explanation))
        {
            throw new ArgumentException("An explanation is required.", nameof(explanation));
        }

        HasAction = hasAction;
        Action = action;
        Utility = utility;
        Explanation = explanation;
    }

    /// <summary>Gets whether a candidate action was selected.</summary>
    public bool HasAction { get; }

    /// <summary>Gets the selected action. Check <see cref="HasAction"/> first.</summary>
    public TAction? Action { get; }

    /// <summary>Gets the selected action's utility score.</summary>
    public double? Utility { get; }

    /// <summary>Gets the explanation for the decision.</summary>
    public string Explanation { get; }

    /// <summary>Creates a decision containing the selected action and its utility.</summary>
    public static UtilityBasedDecision<TAction> Select(TAction action, double utility, string explanation)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (!double.IsFinite(utility))
        {
            throw new ArgumentOutOfRangeException(nameof(utility), utility, "Utility must be finite.");
        }

        return new UtilityBasedDecision<TAction>(true, action, utility, explanation);
    }

    /// <summary>Creates a successful decision in which no action is selected.</summary>
    public static UtilityBasedDecision<TAction> NoAction(string explanation)
    {
        return new UtilityBasedDecision<TAction>(false, default, null, explanation);
    }
}