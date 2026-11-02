namespace AgentForge.Abstractions.Execution;

/// <summary>
/// Describes the terminal outcome of an agent run.
/// </summary>
public enum AgentRunStatus
{
    /// <summary>The agent reached its defined successful terminal condition.</summary>
    Completed,

    /// <summary>A useful output exists, but the requested operation did not fully complete.</summary>
    PartiallyCompleted,

    /// <summary>A non-recoverable failure occurred.</summary>
    Failed,

    /// <summary>The caller or parent execution requested cancellation.</summary>
    Cancelled,

    /// <summary>The run's time limit expired.</summary>
    TimedOut,

    /// <summary>A configured non-time resource limit was reached.</summary>
    BudgetExhausted,

    /// <summary>The input or configuration was rejected before meaningful execution.</summary>
    Rejected
}
