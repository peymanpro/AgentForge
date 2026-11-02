namespace AgentForge.Orchestration;

/// <summary>
/// Contains finite default limits for one in-memory workflow run.
/// </summary>
public sealed class AgentWorkflowOptions
{
    /// <summary>Creates workflow execution options.</summary>
    /// <param name="maxSteps">The maximum number of workflow steps that may actually execute.</param>
    /// <param name="timeout">The overall workflow timeout.</param>
    public AgentWorkflowOptions(int maxSteps = 32, TimeSpan? timeout = null)
    {
        if (maxSteps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSteps));
        }

        var effectiveTimeout = timeout ?? TimeSpan.FromMinutes(5);
        if (effectiveTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), effectiveTimeout, "Workflow timeout must be greater than zero.");
        }

        MaxSteps = maxSteps;
        Timeout = effectiveTimeout;
    }

    /// <summary>Gets the maximum number of workflow steps that may actually execute.</summary>
    public int MaxSteps { get; }

    /// <summary>Gets the overall workflow timeout.</summary>
    public TimeSpan Timeout { get; }
}
