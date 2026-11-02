namespace AgentForge.Retrieval;

/// <summary>
/// Enforces application authorization before delegating a retrieval request to a data-source adapter.
/// </summary>
public sealed class AuthorizedRetriever : IAuthorizedRetriever
{
    private readonly IRetriever _retriever;
    private readonly IRetrievalAuthorizationPolicy _authorizationPolicy;

    /// <summary>Creates an authorized retriever.</summary>
    public AuthorizedRetriever(
        IRetriever retriever,
        IRetrievalAuthorizationPolicy authorizationPolicy)
    {
        ArgumentNullException.ThrowIfNull(retriever);
        ArgumentNullException.ThrowIfNull(authorizationPolicy);

        _retriever = retriever;
        _authorizationPolicy = authorizationPolicy;
    }

    /// <inheritdoc />
    public async Task<RetrievalResult> RetrieveAsync(
        RetrievalRequest request,
        RetrievalExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(context);
        cancellationToken.ThrowIfCancellationRequested();

        RetrievalAuthorizationDecision decision;
        try
        {
            decision = await _authorizationPolicy.AuthorizeAsync(
                request,
                context,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return RetrievalResult.Failed(
                "retrieval.authorization_failed",
                "The retrieval authorization policy failed.");
        }

        if (decision is null)
        {
            return RetrievalResult.Failed(
                "retrieval.invalid_authorization_decision",
                "The retrieval authorization policy returned no decision.");
        }

        cancellationToken.ThrowIfCancellationRequested();

        if (!decision.IsAllowed)
        {
            return RetrievalResult.Denied(decision.Reason ?? "The application policy denied this retrieval operation.");
        }

        if (decision.Scope is null)
        {
            return RetrievalResult.Failed(
                "retrieval.missing_authorized_scope",
                "The authorization policy allowed retrieval without providing a mandatory data scope.");
        }

        // The tenant in the policy scope must not be broader than the identity context supplied by the application.
        if (!string.Equals(decision.Scope.TenantId, context.TenantId, StringComparison.Ordinal))
        {
            return RetrievalResult.Failed(
                "retrieval.tenant_scope_mismatch",
                "The authorized retrieval scope does not match the requested tenant.");
        }

        try
        {
            return await _retriever.RetrieveAsync(
                request,
                decision.Scope,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return RetrievalResult.Failed(
                "retrieval.provider_failed",
                "The retrieval provider failed.");
        }
    }
}
