namespace AgentForge.Abstractions.Execution;

/// <summary>
/// Represents the normalized outcome of one agent run.
/// </summary>
/// <typeparam name="TOutput">The output type produced by the agent.</typeparam>
public sealed class AgentRunResult<TOutput>
{
    private AgentRunResult(
        AgentRunStatus status,
        TOutput? output,
        AgentFailure? failure,
        DateTimeOffset startedAt,
        DateTimeOffset completedAt)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "The run status is not defined.");
        }

        if (completedAt < startedAt)
        {
            throw new ArgumentOutOfRangeException(nameof(completedAt), completedAt, "Completion cannot precede run start.");
        }

        if (status == AgentRunStatus.Completed && failure is not null)
        {
            throw new ArgumentException("A completed run cannot contain a failure.", nameof(failure));
        }

        if (status != AgentRunStatus.Completed && failure is null)
        {
            throw new ArgumentNullException(nameof(failure), "A non-completed run must explain its terminal outcome.");
        }

        failure?.Validate();

        Status = status;
        Output = output;
        Failure = failure;
        StartedAt = startedAt;
        CompletedAt = completedAt;
    }

    /// <summary>
    /// Gets the terminal run status.
    /// </summary>
    public AgentRunStatus Status { get; }

    /// <summary>
    /// Gets the output, including a partial output when the status is PartiallyCompleted.
    /// </summary>
    public TOutput? Output { get; }

    /// <summary>
    /// Gets the normalized failure or stopping reason, when present.
    /// </summary>
    public AgentFailure? Failure { get; }

    /// <summary>
    /// Gets the run start timestamp.
    /// </summary>
    public DateTimeOffset StartedAt { get; }

    /// <summary>
    /// Gets the run completion timestamp.
    /// </summary>
    public DateTimeOffset CompletedAt { get; }

    /// <summary>
    /// Gets the elapsed run duration.
    /// </summary>
    public TimeSpan Duration => CompletedAt - StartedAt;

    /// <summary>
    /// Creates a completed run result.
    /// </summary>
    public static AgentRunResult<TOutput> Completed(
        TOutput? output,
        DateTimeOffset startedAt,
        DateTimeOffset completedAt)
    {
        return new AgentRunResult<TOutput>(AgentRunStatus.Completed, output, null, startedAt, completedAt);
    }

    /// <summary>
    /// Creates a partially completed run result with a useful output and an explanation.
    /// </summary>
    public static AgentRunResult<TOutput> PartiallyCompleted(
        TOutput? output,
        AgentFailure failure,
        DateTimeOffset startedAt,
        DateTimeOffset completedAt)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new AgentRunResult<TOutput>(AgentRunStatus.PartiallyCompleted, output, failure, startedAt, completedAt);
    }

    /// <summary>
    /// Creates a stopped run result for a non-completed terminal status.
    /// </summary>
    /// <param name="status">A non-completed terminal status.</param>
    /// <param name="failure">The normalized reason for stopping.</param>
    /// <param name="startedAt">The run start timestamp.</param>
    /// <param name="completedAt">The run completion timestamp.</param>
    /// <param name="output">An optional partial output preserved when the run stops.</param>
    /// <returns>A result with the specified terminal status and optional partial output.</returns>
    /// <exception cref="ArgumentException">The status is Completed or PartiallyCompleted.</exception>
    public static AgentRunResult<TOutput> Stopped(
        AgentRunStatus status,
        AgentFailure failure,
        DateTimeOffset startedAt,
        DateTimeOffset completedAt,
        TOutput? output = default)
    {
        ArgumentNullException.ThrowIfNull(failure);

        if (status is AgentRunStatus.Completed or AgentRunStatus.PartiallyCompleted)
        {
            throw new ArgumentException("Use the dedicated factory for this status.", nameof(status));
        }

        return new AgentRunResult<TOutput>(status, output, failure, startedAt, completedAt);
    }
}
