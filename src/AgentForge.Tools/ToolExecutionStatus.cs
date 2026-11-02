namespace AgentForge.Tools;

/// <summary>
/// Describes the outcome of one tool invocation attempt.
/// </summary>
public enum ToolExecutionStatus
{
    /// <summary>The tool completed successfully.</summary>
    Succeeded,

    /// <summary>The requested tool is not registered in this execution scope.</summary>
    NotFound,

    /// <summary>The tool rejected the supplied arguments.</summary>
    InvalidArguments,

    /// <summary>The application authorization policy denied the invocation.</summary>
    Denied,

    /// <summary>The invocation requires an approval handler that is not part of this executor.</summary>
    ApprovalRequired,

    /// <summary>The policy, validator, or tool implementation failed unexpectedly.</summary>
    Failed
}
