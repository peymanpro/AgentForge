namespace AgentForge.Retrieval;

/// <summary>
/// Searches within a scope already authorized by the application.
/// </summary>
public interface IRetriever
{
    /// <summary>
    /// Retrieves a bounded set of chunks while enforcing the supplied authorized scope before returning content.
    /// </summary>
    Task<RetrievalResult> RetrieveAsync(
        RetrievalRequest request,
        RetrievalScope authorizedScope,
        CancellationToken cancellationToken = default);
}
