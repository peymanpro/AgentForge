namespace AgentForge.Retrieval;

/// <summary>
/// Represents an explicit retrieval authorization decision.
/// </summary>
public sealed class RetrievalAuthorizationDecision
{
    private RetrievalAuthorizationDecision(bool isAllowed, RetrievalScope? scope, string? reason)
    {
        IsAllowed = isAllowed;
        Scope = scope;
        Reason = reason;
    }

    /// <summary>Gets whether the operation is allowed.</summary>
    public bool IsAllowed { get; }

    /// <summary>Gets the mandatory data scope, when authorized.</summary>
    public RetrievalScope? Scope { get; }

    /// <summary>Gets a safe denial reason, when denied.</summary>
    public string? Reason { get; }

    /// <summary>Creates an allowed decision with a mandatory tenant/document scope.</summary>
    public static RetrievalAuthorizationDecision Allowed(RetrievalScope scope)
    {
        ArgumentNullException.ThrowIfNull(scope);
        return new RetrievalAuthorizationDecision(true, scope, null);
    }

    /// <summary>Creates a denied decision.</summary>
    public static RetrievalAuthorizationDecision Denied(string reason = "The application policy denied this retrieval operation.")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        return new RetrievalAuthorizationDecision(false, null, reason);
    }
}
