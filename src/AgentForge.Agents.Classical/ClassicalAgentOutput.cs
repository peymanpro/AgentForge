namespace AgentForge.Agents.Classical;

/// <summary>
/// Contains the decision produced by a classical agent and the rule that selected it, when applicable.
/// </summary>
/// <typeparam name="TAction">The action type.</typeparam>
public sealed record ClassicalAgentOutput<TAction> where TAction : notnull
{
    /// <summary>
    /// Creates a classical-agent output.
    /// </summary>
    /// <param name="decision">The selected action or no-action decision.</param>
    /// <param name="appliedRuleName">The name of the rule that matched, or <see langword="null"/>.</param>
    public ClassicalAgentOutput(ClassicalDecision<TAction> decision, string? appliedRuleName)
    {
        ArgumentNullException.ThrowIfNull(decision);
        Decision = decision;
        AppliedRuleName = appliedRuleName;
    }

    /// <summary>
    /// Gets the action decision.
    /// </summary>
    public ClassicalDecision<TAction> Decision { get; }

    /// <summary>
    /// Gets the matched rule name, or <see langword="null"/> when no rule applied.
    /// </summary>
    public string? AppliedRuleName { get; }
}
