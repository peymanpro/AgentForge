using AgentForge.Agents.UtilityBased.Domain.Decisions;
using AgentForge.Agents.UtilityBased.Domain.Evaluation;

namespace AgentForge.Agents.UtilityBased.Application.UseCases;

/// <summary>
/// Contains the decision and the ordered evaluations produced by one Utility-Based agent run.
/// </summary>
/// <typeparam name="TAction">The candidate action type.</typeparam>
public sealed class UtilityBasedOutput<TAction> where TAction : notnull
{
    /// <summary>Creates an immutable output snapshot.</summary>
    public UtilityBasedOutput(
        UtilityBasedDecision<TAction> decision,
        IEnumerable<UtilityEvaluation<TAction>> evaluations)
    {
        ArgumentNullException.ThrowIfNull(decision);
        ArgumentNullException.ThrowIfNull(evaluations);

        var items = evaluations.ToArray();
        if (items.Any(static item => item is null))
        {
            throw new ArgumentException("The evaluation collection cannot contain null entries.", nameof(evaluations));
        }

        Decision = decision;
        Evaluations = Array.AsReadOnly(items);
    }

    /// <summary>Gets the selected action or no-action decision.</summary>
    public UtilityBasedDecision<TAction> Decision { get; }

    /// <summary>Gets every candidate evaluation in evaluation order.</summary>
    public IReadOnlyList<UtilityEvaluation<TAction>> Evaluations { get; }
}