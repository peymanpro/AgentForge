using AgentForge.Abstractions.Execution;

namespace AgentForge.Retrieval;

/// <summary>
/// Supplies the application identity and run correlation needed by retrieval authorization.
/// </summary>
public sealed class RetrievalExecutionContext
{
    /// <summary>Creates a retrieval execution context.</summary>
    public RetrievalExecutionContext(
        string principalId,
        string tenantId,
        AgentRunContext runContext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(principalId);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentNullException.ThrowIfNull(runContext);

        PrincipalId = principalId;
        TenantId = tenantId;
        RunContext = runContext;
    }

    /// <summary>Gets the application principal identifier.</summary>
    public string PrincipalId { get; }

    /// <summary>Gets the tenant identifier requested by the application.</summary>
    public string TenantId { get; }

    /// <summary>Gets the current agent run context.</summary>
    public AgentRunContext RunContext { get; }
}
