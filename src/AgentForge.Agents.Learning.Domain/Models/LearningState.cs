namespace AgentForge.Agents.Learning.Domain.Models;
/// <summary>An immutable snapshot of learned action statistics.</summary>
public sealed class LearningState<TAction> where TAction : notnull
{
    private readonly IReadOnlyDictionary<TAction, ActionStatistics> _statistics;
    public LearningState(IEnumerable<KeyValuePair<TAction, ActionStatistics>>? statistics = null)
    {
        var copy = new Dictionary<TAction, ActionStatistics>();
        if (statistics is not null)
            foreach (var entry in statistics)
            {
                ArgumentNullException.ThrowIfNull(entry.Key);
                ArgumentNullException.ThrowIfNull(entry.Value);
                if (!copy.TryAdd(entry.Key, entry.Value))
                    throw new ArgumentException("Duplicate action key.", nameof(statistics));
            }
        _statistics = new System.Collections.ObjectModel.ReadOnlyDictionary<TAction, ActionStatistics>(copy);
    }
    public long TotalObservations => _statistics.Values.Sum(item => item.ObservationCount);
    public IReadOnlyDictionary<TAction, ActionStatistics> Statistics => _statistics;
    public ActionStatistics GetStatistics(TAction action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return _statistics.TryGetValue(action, out var value) ? value : new ActionStatistics(0, 0);
    }
    public LearningState<TAction> Observe(TAction action, double reward)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (!double.IsFinite(reward)) throw new ArgumentOutOfRangeException(nameof(reward));
        var next = new Dictionary<TAction, ActionStatistics>(_statistics) { [action] = GetStatistics(action).Add(reward) };
        return new LearningState<TAction>(next);
    }
}