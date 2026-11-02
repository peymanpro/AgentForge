using System.Text.Json;
using AgentForge.Abstractions.Execution;

namespace AgentForge.Agents.Planning.Execution;

/// <summary>
/// Represents the terminal outcome of one plan step.
/// </summary>
public sealed class PlanStepResult
{
    /// <summary>
    /// Creates a terminal outcome for one plan step.
    /// </summary>
    /// <param name="stepId">The stable plan step identifier.</param>
    /// <param name="status">The terminal status of the step.</param>
    /// <param name="startedAt">The timestamp at which step execution began.</param>
    /// <param name="completedAt">The timestamp at which the step reached its terminal state.</param>
    /// <param name="output">The JSON output produced by a completed step, when available.</param>
    /// <param name="failure">The normalized reason when the step did not complete.</param>
    public PlanStepResult(
        string stepId,
        PlanStepStatus status,
        DateTimeOffset startedAt,
        DateTimeOffset completedAt,
        JsonElement? output = null,
        AgentFailure? failure = null)
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

        if (status == PlanStepStatus.Completed && (output is null || failure is not null))
        {
            throw new ArgumentException("A completed step requires an output and cannot contain a failure.");
        }

        if (status != PlanStepStatus.Completed && failure is null)
        {
            throw new ArgumentNullException(nameof(failure), "A non-completed step must explain its outcome.");
        }

        failure?.Validate();

        StepId = stepId;
        Status = status;
        StartedAt = startedAt;
        CompletedAt = completedAt;
        Output = output?.Clone();
        Failure = failure;
    }

    /// <summary>Gets the step identifier.</summary>
    public string StepId { get; }

    /// <summary>Gets the terminal step status.</summary>
    public PlanStepStatus Status { get; }

    /// <summary>Gets the step start timestamp.</summary>
    public DateTimeOffset StartedAt { get; }

    /// <summary>Gets the step completion timestamp.</summary>
    public DateTimeOffset CompletedAt { get; }

    /// <summary>Gets the step output, when completed.</summary>
    public JsonElement? Output { get; }

    /// <summary>Gets the normalized failure or skip reason, when not completed.</summary>
    public AgentFailure? Failure { get; }
}
