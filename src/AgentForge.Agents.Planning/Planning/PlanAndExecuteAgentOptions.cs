namespace AgentForge.Agents.Planning.Planning;

/// <summary>
/// Contains finite default limits for a Plan-and-Execute agent.
/// </summary>
public sealed class PlanAndExecuteAgentOptions
{
    /// <summary>
    /// Creates planning agent options.
    /// </summary>
    /// <param name="maxPlanSteps">Maximum number of steps accepted in a generated plan.</param>
    /// <param name="timeout">Overall plan generation and execution timeout.</param>
    public PlanAndExecuteAgentOptions(int maxPlanSteps = 12, TimeSpan? timeout = null)
    {
        if (maxPlanSteps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPlanSteps));
        }

        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(90);
        if (effectiveTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), effectiveTimeout, "Timeout must be greater than zero.");
        }

        MaxPlanSteps = maxPlanSteps;
        Timeout = effectiveTimeout;
    }

    /// <summary>Gets the maximum number of steps accepted in one generated plan.</summary>
    public int MaxPlanSteps { get; }

    /// <summary>Gets the overall plan generation and execution timeout.</summary>
    public TimeSpan Timeout { get; }
}
