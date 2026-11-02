namespace AgentForge.Agents.Reflection;

/// <summary>
/// Contains finite limits and refinement behavior options for a Reflection agent.
/// </summary>
public sealed class ReflectionAgentOptions
{
    /// <summary>
    /// Creates Reflection agent options.
    /// </summary>
    /// <param name="maxAttempts">Maximum number of candidate generation/evaluation attempts.</param>
    /// <param name="timeout">Overall generation and evaluation timeout.</param>
    /// <param name="stopOnNoImprovement">Whether to stop when an evaluated score does not improve.</param>
    /// <param name="minimumScoreImprovement">Minimum score increase required to count as improvement.</param>
    public ReflectionAgentOptions(
        int maxAttempts = 3,
        TimeSpan? timeout = null,
        bool stopOnNoImprovement = false,
        double minimumScoreImprovement = 0)
    {
        if (maxAttempts <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAttempts));
        }

        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(90);
        if (effectiveTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), effectiveTimeout, "Timeout must be greater than zero.");
        }

        if (!double.IsFinite(minimumScoreImprovement) || minimumScoreImprovement < 0 || minimumScoreImprovement > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(minimumScoreImprovement),
                minimumScoreImprovement,
                "Minimum score improvement must be between zero and one.");
        }

        MaxAttempts = maxAttempts;
        Timeout = effectiveTimeout;
        StopOnNoImprovement = stopOnNoImprovement;
        MinimumScoreImprovement = minimumScoreImprovement;
    }

    /// <summary>Gets the maximum candidate generation/evaluation attempts.</summary>
    public int MaxAttempts { get; }

    /// <summary>Gets the overall generation and evaluation timeout.</summary>
    public TimeSpan Timeout { get; }

    /// <summary>Gets whether the agent stops after a scored attempt fails to improve.</summary>
    public bool StopOnNoImprovement { get; }

    /// <summary>Gets the minimum score increase required to count as improvement.</summary>
    public double MinimumScoreImprovement { get; }
}
