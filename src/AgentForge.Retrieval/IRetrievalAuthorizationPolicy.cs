namespace AgentForge.Retrieval;

/// <summary>
/// Determines the tenant and document scope that an application principal may retrieve.
/// </summary>
public interface IRetrievalAuthorizationPolicy
{
    /// <summary>
    /// Authorizes one retrieval request and returns the mandatory data scope.
    /// </summary>
    Task<RetrievalAuthorizationDecision> AuthorizeAsync(
        RetrievalRequest request,
        RetrievalExecutionContext context,
        CancellationToken cancellationToken = default);
}
