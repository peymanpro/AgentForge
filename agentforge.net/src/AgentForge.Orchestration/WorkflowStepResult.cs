using AgentForge.Abstractions.Execution;

namespace AgentForge.Orchestration;

/// <summary>
/// Represents the observable outcome of one workflow step.
/// </summary>
public sealed class WorkflowStepResult
{
    internal WorkflowStepResult(
        string stepId,
        WorkflowStepStatus status,
        DateTimeOffset startedAt,
        DateTimeOffset completedAt,
        object? output = null,
        AgentFailure? failure = null,
        Guid? childRunId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stepId);

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        if (completedAt < startedAt)
        {
            throw new ArgumentOutOfRangeException(nameof(completedAt));
        }

        if (status == WorkflowStepStatus.Completed && failure is not null)
        {
            throw new ArgumentException("A completed step cannot contain a failure.", nameof(failure));
        }

        if (status is not WorkflowStepStatus.Completed and not WorkflowStepStatus.Skipped && failure is null)
        {
            throw new ArgumentNullException(nameof(failure), "A non-completed step must explain its outcome.");
        }

        failure?.Validate();

        StepId = stepId;
        Status = status;
        StartedAt = startedAt;
        CompletedAt = completedAt;
        Output = output;
        Failure = failure;
        ChildRunId = childRunId;
    }

    /// <summary>Gets the stable workflow step identifier.</summary>
    public string StepId { get; }

    /// <summary>Gets the terminal status of the step.</summary>
    public WorkflowStepStatus Status { get; }

    /// <summary>Gets the step start timestamp.</summary>
    public DateTimeOffset StartedAt { get; }

    /// <summary>Gets the step completion timestamp.</summary>
    public DateTimeOffset CompletedAt { get; }

    /// <summary>Gets the output, including partial output when the step did not fully complete.</summary>
    public object? Output { get; }

    /// <summary>Gets the normalized failure or skip reason, when applicable.</summary>
    public AgentFailure? Failure { get; }

    /// <summary>Gets the child agent run identifier, when the step invoked an agent.</summary>
    public Guid? ChildRunId { get; }
}
