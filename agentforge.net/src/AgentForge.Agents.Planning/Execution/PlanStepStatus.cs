namespace AgentForge.Agents.Planning.Execution;

/// <summary>
/// Describes the observable outcome of one planned step.
/// </summary>
public enum PlanStepStatus
{
    /// <summary>The step completed successfully.</summary>
    Completed,

    /// <summary>The step failed during execution.</summary>
    Failed,

    /// <summary>The step was not started because an earlier step failed.</summary>
    Blocked,

    /// <summary>The step was not started because the execution budget was exhausted.</summary>
    Skipped,

    /// <summary>The step was interrupted by run cancellation or timeout.</summary>
    Cancelled
}
