using AgentForge.Agents.Learning.Domain.Models;
namespace AgentForge.Agents.Learning.Domain.Policies;
/// <summary>Selects an action using available candidates and learned estimates.</summary>
public interface ILearningPolicy<TAction> where TAction : notnull
{
    (TAction Action, bool WasExploration) Select(IReadOnlyList<ActionEstimate<TAction>> candidates, long totalObservations);
}