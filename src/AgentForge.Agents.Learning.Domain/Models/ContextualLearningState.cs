namespace AgentForge.Agents.Learning.Domain.Models;

/// <summary>Immutable sufficient-statistics snapshot for a disjoint linear contextual bandit.</summary>
/// <typeparam name="TAction">The action identity type.</typeparam>
public sealed class ContextualLearningState<TAction> where TAction : notnull
{
    private readonly IReadOnlyDictionary<TAction, ContextualActionStatistics> _statistics;

    /// <summary>Creates an empty state with a fixed feature dimension and positive ridge regularization.</summary>
    public ContextualLearningState(int featureCount, double regularization = 1.0,
        IEnumerable<KeyValuePair<TAction, ContextualActionStatistics>>? statistics = null)
    {
        if (featureCount <= 0) throw new ArgumentOutOfRangeException(nameof(featureCount));
        if (!double.IsFinite(regularization) || regularization <= 0)
            throw new ArgumentOutOfRangeException(nameof(regularization));

        FeatureCount = featureCount;
        Regularization = regularization;
        var copy = new Dictionary<TAction, ContextualActionStatistics>();
        if (statistics is not null)
        {
            foreach (var entry in statistics)
            {
                ArgumentNullException.ThrowIfNull(entry.Key);
                ArgumentNullException.ThrowIfNull(entry.Value);
                if (entry.Value.FeatureCount != featureCount)
                    throw new ArgumentException("All action statistics must use the configured feature count.", nameof(statistics));
                if (!copy.TryAdd(entry.Key, entry.Value))
                    throw new ArgumentException("Duplicate action key.", nameof(statistics));
            }
        }
        _statistics = new System.Collections.ObjectModel.ReadOnlyDictionary<TAction, ContextualActionStatistics>(copy);
    }

    /// <summary>Gets the fixed number of features expected in each context.</summary>
    public int FeatureCount { get; }

    /// <summary>Gets the positive ridge regularization used for unseen actions.</summary>
    public double Regularization { get; }

    /// <summary>Gets the number of observed rewards across all actions.</summary>
    public long TotalObservations => _statistics.Values.Sum(item => item.ObservationCount);

    /// <summary>Gets a read-only map of observed action statistics.</summary>
    public IReadOnlyDictionary<TAction, ContextualActionStatistics> Statistics => _statistics;

    /// <summary>Gets existing statistics or the regularized prior for an unseen action.</summary>
    public ContextualActionStatistics GetStatistics(TAction action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (_statistics.TryGetValue(action, out var value)) return value;
        var matrix = new double[FeatureCount, FeatureCount];
        for (var index = 0; index < FeatureCount; index++) matrix[index, index] = Regularization;
        return new ContextualActionStatistics(matrix, new double[FeatureCount]);
    }

    /// <summary>Gets the number of observations for an action.</summary>
    public long GetObservationCount(TAction action) =>
        _statistics.TryGetValue(action, out var value) ? value.ObservationCount : 0;

    /// <summary>Returns a new state after applying one finite reward observation.</summary>
    public ContextualLearningState<TAction> Observe(TAction action, IEnumerable<double> context, double reward)
    {
        ArgumentNullException.ThrowIfNull(action);
        ArgumentNullException.ThrowIfNull(context);
        if (!double.IsFinite(reward)) throw new ArgumentOutOfRangeException(nameof(reward));
        var features = context.ToArray();
        if (features.Length != FeatureCount || features.Any(value => !double.IsFinite(value)))
            throw new ArgumentException("Context must contain exactly the configured number of finite features.", nameof(context));

        var current = GetStatistics(action);
        var matrix = current.CopyDesignMatrix();
        var vector = current.CopyRewardVector();
        for (var row = 0; row < FeatureCount; row++)
        {
            vector[row] += reward * features[row];
            for (var column = 0; column < FeatureCount; column++)
                matrix[row, column] += features[row] * features[column];
        }
        if (vector.Any(value => !double.IsFinite(value)) ||
            matrix.Cast<double>().Any(value => !double.IsFinite(value)))
            throw new ArithmeticException("The observation caused non-finite learning statistics.");

        var next = new Dictionary<TAction, ContextualActionStatistics>(_statistics)
        {
            [action] = new ContextualActionStatistics(matrix, vector, checked(current.ObservationCount + 1))
        };
        return new ContextualLearningState<TAction>(FeatureCount, Regularization, next);
    }
}
