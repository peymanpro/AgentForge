namespace AgentForge.Agents.Learning.Domain.Models;
/// <summary>An immutable snapshot of learned action statistics.</summary>
/// <typeparam name="TAction">The action identity type.</typeparam>
public sealed class LearningState<TAction> where TAction : notnull
{
    private readonly IReadOnlyDictionary<TAction, ActionStatistics> _statistics;
    /// <summary>Creates a state snapshot from previously accumulated action statistics.</summary>
    /// <param name="statistics">Optional action statistics to seed the state.</param>
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
    /// <summary>Gets the total number of reward observations across all actions.</summary>
    public long TotalObservations => _statistics.Values.Sum(item => item.ObservationCount);
    /// <summary>Gets a read-only view of the per-action statistics.</summary>
    public IReadOnlyDictionary<TAction, ActionStatistics> Statistics => _statistics;
    /// <summary>Gets the statistics for an action, or zero observations when it is unseen.</summary>
    /// <param name="action">Action to inspect.</param>
    public ActionStatistics GetStatistics(TAction action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return _statistics.TryGetValue(action, out var value) ? value : new ActionStatistics(0, 0);
    }
    /// <summary>Returns a new state with one reward observation applied.</summary>
    /// <param name="action">Action whose outcome was evaluated.</param>
    /// <param name="reward">Finite reward assigned by the host-side critic.</param>
    public LearningState<TAction> Observe(TAction action, double reward)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (!double.IsFinite(reward)) throw new ArgumentOutOfRangeException(nameof(reward));
        var next = new Dictionary<TAction, ActionStatistics>(_statistics) { [action] = GetStatistics(action).Add(reward) };
        return new LearningState<TAction>(next);
    }
}