namespace AgentForge.Orchestration;

/// <summary>
/// Contains finite default limits for a workflow run.
/// </summary>
public sealed class WorkflowOptions
{
    /// <summary>
    /// Creates bounded workflow options.
    /// </summary>
    /// <param name="maxSteps">Maximum number of steps that may actually execute.</param>
    /// <param name="timeout">Maximum elapsed time for the complete workflow.</param>
    /// <param name="stepTimeout">Maximum elapsed time for one step.</param>
    public WorkflowOptions(
        int maxSteps = 20,
        TimeSpan? timeout = null,
        TimeSpan? stepTimeout = null)
    {
        if (maxSteps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxSteps));
        }

        var effectiveTimeout = timeout ?? TimeSpan.FromMinutes(3);
        var effectiveStepTimeout = stepTimeout ?? TimeSpan.FromSeconds(60);

        ValidateTimeout(effectiveTimeout, nameof(timeout));
        ValidateTimeout(effectiveStepTimeout, nameof(stepTimeout));

        MaxSteps = maxSteps;
        Timeout = effectiveTimeout;
        StepTimeout = effectiveStepTimeout;
    }

    /// <summary>Gets the maximum number of executed steps.</summary>
    public int MaxSteps { get; }

    /// <summary>Gets the maximum elapsed time for the complete workflow.</summary>
    public TimeSpan Timeout { get; }

    /// <summary>Gets the maximum elapsed time for an individual step.</summary>
    public TimeSpan StepTimeout { get; }

    private static void ValidateTimeout(TimeSpan value, string parameterName)
    {
        if (value <= TimeSpan.Zero || value > TimeSpan.FromMilliseconds(4_294_967_294d))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Timeout must be positive and within the supported timer range.");
        }
    }
}
