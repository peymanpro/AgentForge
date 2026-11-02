namespace AgentForge.Agents.UtilityBased.Domain.Evaluation;

/// <summary>
/// Records the utility assigned to one candidate action.
/// </summary>
/// <typeparam name="TAction">The candidate action type.</typeparam>
public sealed record UtilityEvaluation<TAction> where TAction : notnull
{
    /// <summary>
    /// Creates a utility evaluation.
    /// </summary>
    public UtilityEvaluation(TAction action, double utility)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (!double.IsFinite(utility))
        {
            throw new ArgumentOutOfRangeException(nameof(utility), utility, "Utility must be finite.");
        }

        Action = action;
        Utility = utility;
    }

    /// <summary>Gets the evaluated candidate action.</summary>
    public TAction Action { get; }

    /// <summary>Gets its finite utility score.</summary>
    public double Utility { get; }
}