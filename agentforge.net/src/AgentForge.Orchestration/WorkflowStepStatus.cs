namespace AgentForge.Orchestration;

/// <summary>
/// Describes the terminal outcome of one workflow step.
/// </summary>
public enum WorkflowStepStatus
{
    /// <summary>The step completed successfully.</summary>
    Completed,

    /// <summary>The step produced useful partial output but did not fully complete.</summary>
    PartiallyCompleted,

    /// <summary>The step failed.</summary>
    Failed,

    /// <summary>The step was not selected by its declared condition.</summary>
    Skipped,

    /// <summary>The step was not started because an earlier step failed or stopped the workflow.</summary>
    Blocked,

    /// <summary>The active step was cancelled.</summary>
    Cancelled,

    /// <summary>The active step exceeded its time limit.</summary>
    TimedOut,

    /// <summary>The step or workflow reached a configured execution limit.</summary>
    BudgetExhausted,

    /// <summary>The step input or configuration was rejected.</summary>
    Rejected
}
