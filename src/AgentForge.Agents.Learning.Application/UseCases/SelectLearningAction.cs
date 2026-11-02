using AgentForge.Agents.Learning.Domain.Models;
using AgentForge.Agents.Learning.Domain.Policies;
namespace AgentForge.Agents.Learning.Application.UseCases;
/// <summary>Builds reward estimates and delegates selection to a learning policy.</summary>
public sealed class SelectLearningAction<TAction> where TAction : notnull
{
    private readonly ILearningPolicy<TAction> _policy;
    public SelectLearningAction(ILearningPolicy<TAction> policy) => _policy = policy ?? throw new ArgumentNullException(nameof(policy));
    public LearningAgentOutput<TAction> Execute(LearningAgentInput<TAction> input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var estimates = input.Candidates.Select(action =>
        {
            var stats = input.State.GetStatistics(action);
            return new ActionEstimate<TAction>(action, stats.ObservationCount, stats.AverageReward);
        }).ToArray();
        var selection = _policy.Select(estimates, input.State.TotalObservations);
        var selected = estimates.First(item => EqualityComparer<TAction>.Default.Equals(item.Action, selection.Action));
        return new LearningAgentOutput<TAction>(selection.Action, selection.WasExploration, input.State.TotalObservations, selected.AverageReward);
    }
}