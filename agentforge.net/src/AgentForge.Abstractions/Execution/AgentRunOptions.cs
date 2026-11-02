namespace AgentForge.Abstractions.Execution;

/// <summary>
/// Contains caller-selected limits for a single agent run.
/// </summary>
public sealed record AgentRunOptions
{
    /// <summary>
    /// Gets the maximum number of execution steps, when specified.
    /// </summary>
    public int? MaxSteps { get; init; }

    /// <summary>
    /// Gets the maximum number of model requests, when specified.
    /// </summary>
    public int? MaxModelRequests { get; init; }

    /// <summary>
    /// Gets the maximum number of tool invocations, when specified.
    /// </summary>
    public int? MaxToolInvocations { get; init; }

    /// <summary>
    /// Gets the maximum elapsed time for the complete run, when specified.
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    /// Validates that all configured limits are positive.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">A configured limit is zero or negative.</exception>
    public void Validate()
    {
        ValidatePositive(MaxSteps, nameof(MaxSteps));
        ValidatePositive(MaxModelRequests, nameof(MaxModelRequests));
        ValidatePositive(MaxToolInvocations, nameof(MaxToolInvocations));

        if (Timeout is { } timeout && timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(Timeout), timeout, "Timeout must be greater than zero.");
        }
    }

    private static void ValidatePositive(int? value, string parameterName)
    {
        if (value is <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "A configured limit must be greater than zero.");
        }
    }
}
