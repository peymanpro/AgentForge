namespace AgentForge.Agents.Learning.Domain.Models;
/// <summary>Aggregated reward observations for one action.</summary>
public sealed record ActionStatistics
{
    public ActionStatistics(long observationCount, double averageReward)
    {
        if (observationCount < 0) throw new ArgumentOutOfRangeException(nameof(observationCount));
        if (!double.IsFinite(averageReward)) throw new ArgumentOutOfRangeException(nameof(averageReward));
        ObservationCount = observationCount;
        AverageReward = observationCount == 0 ? 0 : averageReward;
    }
    public long ObservationCount { get; }
    public double AverageReward { get; }
    public ActionStatistics Add(double reward)
    {
        if (!double.IsFinite(reward)) throw new ArgumentOutOfRangeException(nameof(reward));
        var count = checked(ObservationCount + 1);
        return new ActionStatistics(count, AverageReward + ((reward - AverageReward) / count));
    }
}