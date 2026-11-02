namespace AgentForge.Tools;

/// <summary>
/// Represents an application's authorization decision for a tool invocation.
/// </summary>
public enum ToolAuthorizationDecision
{
    /// <summary>The invocation is authorized to proceed.</summary>
    Allowed,

    /// <summary>The invocation is denied and must not execute.</summary>
    Denied,

    /// <summary>An external approval step is required before execution.</summary>
    ApprovalRequired
}
