using AgentForge.Agents.Learning.Domain.Models;
namespace AgentForge.Agents.Learning.Domain.Policies;
/// <summary>Selects an action using available candidates and learned estimates.</summary>
/// <typeparam name="TAction">The action identity type.</typeparam>
public interface ILearningPolicy<TAction> where TAction : notnull
{
    /// <summary>Selects one candidate action.</summary>
    /// <param name="candidates">Ordered action estimates for this decision.</param>
    /// <param name="totalObservations">Total observations in the supplied learning state.</param>
    /// <returns>The selected action and whether exploration was used.</returns>
    (TAction Action, bool WasExploration) Select(IReadOnlyList<ActionEstimate<TAction>> candidates, long totalObservations);
}