using System.Collections.ObjectModel;

namespace AgentForge.Orchestration;

/// <summary>
/// Contains the ordered outcomes and condition decisions from one in-memory workflow run.
/// </summary>
public sealed class WorkflowRunOutput
{
    internal WorkflowRunOutput(
        string workflowId,
        IReadOnlyList<WorkflowStepResult> steps,
        IReadOnlyDictionary<string, bool> conditionResults)
    {
        WorkflowId = workflowId;
        Steps = Array.AsReadOnly(steps.ToArray());
        ConditionResults = new ReadOnlyDictionary<string, bool>(
            new Dictionary<string, bool>(conditionResults, StringComparer.Ordinal));
    }

    /// <summary>Gets the stable workflow identifier.</summary>
    public string WorkflowId { get; }

    /// <summary>Gets the ordered results for executed, skipped, or blocked steps.</summary>
    public IReadOnlyList<WorkflowStepResult> Steps { get; }

    /// <summary>Gets the condition values evaluated during the run.</summary>
    public IReadOnlyDictionary<string, bool> ConditionResults { get; }

    /// <summary>Gets the number of steps that completed successfully.</summary>
    public int CompletedStepCount =>
        Steps.Count(static step => step.Status == WorkflowStepStatus.Completed);

    /// <summary>
    /// Gets a previously produced workflow step output by its identifier.
    /// </summary>
    public TOutput GetOutput<TOutput>(string stepId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stepId);
        var step = Steps.FirstOrDefault(item => string.Equals(item.StepId, stepId, StringComparison.Ordinal));

        if (step is null || step.Status is not WorkflowStepStatus.Completed and not WorkflowStepStatus.PartiallyCompleted)
        {
            throw new KeyNotFoundException($"No completed output is available for workflow step '{stepId}'.");
        }

        if (step.Output is TOutput typed)
        {
            return typed;
        }

        if (step.Output is null && default(TOutput) is null)
        {
            return default!;
        }

        throw new InvalidCastException(
            $"Workflow step '{stepId}' output is not compatible with '{typeof(TOutput).FullName}'.");
    }
}
