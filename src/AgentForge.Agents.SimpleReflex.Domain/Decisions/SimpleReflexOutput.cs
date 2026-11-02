namespace AgentForge.Agents.SimpleReflex.Domain.Decisions;

/// <summary>
/// Contains a simple-reflex decision and the rule that produced it.
/// </summary>
/// <typeparam name="TAction">The action type.</typeparam>
public sealed record SimpleReflexOutput<TAction> where TAction : notnull
{
    /// <summary>
    /// Creates a simple-reflex output.
    /// </summary>
    public SimpleReflexOutput(SimpleReflexDecision<TAction> decision, string? matchedRuleName)
    {
        ArgumentNullException.ThrowIfNull(decision);
        Decision = decision;
        MatchedRuleName = matchedRuleName;
    }

    /// <summary>
    /// Gets the decision.
    /// </summary>
    public SimpleReflexDecision<TAction> Decision { get; }

    /// <summary>
    /// Gets the matched rule name, or null when no rule matched.
    /// </summary>
    public string? MatchedRuleName { get; }
}
