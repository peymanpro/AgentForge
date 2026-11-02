namespace AgentForge.Retrieval;

/// <summary>
/// Retrieves data only after application authorization has produced an enforceable scope.
/// </summary>
public interface IAuthorizedRetriever
{
    /// <summary>Performs policy-authorized retrieval.</summary>
    Task<RetrievalResult> RetrieveAsync(
        RetrievalRequest request,
        RetrievalExecutionContext context,
        CancellationToken cancellationToken = default);
}
