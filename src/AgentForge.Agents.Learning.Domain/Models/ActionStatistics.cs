namespace AgentForge.Agents.Learning.Domain.Models;
/// <summary>Aggregated reward observations for one action.</summary>
public sealed record ActionStatistics
{
    /// <summary>Creates validated action statistics.</summary>
    /// <param name="observationCount">Number of observed outcomes.</param>
    /// <param name="averageReward">Mean reward across observed outcomes.</param>
    public ActionStatistics(long observationCount, double averageReward)
    {
        if (observationCount < 0) throw new ArgumentOutOfRangeException(nameof(observationCount));
        if (!double.IsFinite(averageReward)) throw new ArgumentOutOfRangeException(nameof(averageReward));
        ObservationCount = observationCount;
        AverageReward = observationCount == 0 ? 0 : averageReward;
    }
    /// <summary>Gets the number of observations.</summary>
    public long ObservationCount { get; }
    /// <summary>Gets the running mean reward.</summary>
    public double AverageReward { get; }
    /// <summary>Returns statistics with one additional reward observation.</summary>
    /// <param name="reward">Finite reward for the latest outcome.</param>
    public ActionStatistics Add(double reward)
    {
        if (!double.IsFinite(reward)) throw new ArgumentOutOfRangeException(nameof(reward));
        var count = checked(ObservationCount + 1);
        return new ActionStatistics(count, AverageReward + ((reward - AverageReward) / count));
    }
}