using AgentForge.Agents.Learning.Domain.Models;
using AgentForge.Agents.Learning.Domain.Policies;
namespace AgentForge.Agents.Learning.Policies;
/// <summary>Balances exploration and exploitation with epsilon-greedy selection.</summary>
/// <typeparam name="TAction">The action identity type.</typeparam>
public sealed class EpsilonGreedyLearningPolicy<TAction> : ILearningPolicy<TAction> where TAction : notnull
{
    private readonly double _epsilon;
    private readonly Func<double> _nextDouble;
    private readonly Func<int, int> _nextIndex;
    /// <summary>Creates an epsilon-greedy policy.</summary>
    /// <param name="epsilon">Exploration probability in the inclusive range zero to one.</param>
    /// <param name="nextDouble">Optional random source for exploration probability.</param>
    /// <param name="nextIndex">Optional random source for candidate index selection.</param>
    public EpsilonGreedyLearningPolicy(double epsilon = 0.1, Func<double>? nextDouble = null, Func<int, int>? nextIndex = null)
    {
        if (!double.IsFinite(epsilon) || epsilon < 0 || epsilon > 1) throw new ArgumentOutOfRangeException(nameof(epsilon));
        _epsilon = epsilon;
        _nextDouble = nextDouble ?? Random.Shared.NextDouble;
        _nextIndex = nextIndex ?? Random.Shared.Next;
    }
    /// <inheritdoc />
    public (TAction Action, bool WasExploration) Select(IReadOnlyList<ActionEstimate<TAction>> candidates, long totalObservations)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        if (candidates.Count == 0) throw new ArgumentException("At least one candidate is required.", nameof(candidates));
        var unseen = candidates.Where(item => item.ObservationCount == 0).ToArray();
        if (unseen.Length > 0) return (unseen[0].Action, true);
        if (_nextDouble() < _epsilon) return (candidates[_nextIndex(candidates.Count)].Action, true);
        var best = candidates[0];
        for (var i = 1; i < candidates.Count; i++)
            if (candidates[i].AverageReward > best.AverageReward) best = candidates[i];
        return (best.Action, false);
    }
}