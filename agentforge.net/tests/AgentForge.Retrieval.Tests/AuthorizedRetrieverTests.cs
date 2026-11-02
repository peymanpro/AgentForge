using AgentForge.Abstractions.Execution;
using AgentForge.Retrieval;
using Xunit;

namespace AgentForge.Retrieval.Tests;

public sealed class AuthorizedRetrieverTests
{
    [Fact]
    public async Task RetrieveAsync_DenialPreventsDataSourceAccess()
    {
        var source = new RecordingRetriever(RetrievalResult.NoResults());
        var service = new AuthorizedRetriever(source, new FixedPolicy(RetrievalAuthorizationDecision.Denied()));

        var result = await service.RetrieveAsync(
            CreateRequest(),
            CreateContext());

        Assert.Equal(RetrievalStatus.Denied, result.Status);
        Assert.Equal(0, source.CallCount);
    }

    [Fact]
    public async Task RetrieveAsync_TenantScopeMismatchPreventsDataSourceAccess()
    {
        var source = new RecordingRetriever(RetrievalResult.NoResults());
        var policy = new FixedPolicy(
            RetrievalAuthorizationDecision.Allowed(new RetrievalScope("tenant-b")));
        var service = new AuthorizedRetriever(source, policy);

        var result = await service.RetrieveAsync(
            CreateRequest(),
            CreateContext("tenant-a"));

        Assert.Equal(RetrievalStatus.Failed, result.Status);
        Assert.Equal("retrieval.tenant_scope_mismatch", result.Failure?.Code);
        Assert.Equal(0, source.CallCount);
    }

    [Fact]
    public async Task RetrieveAsync_PolicyFailureIsSanitizedAndFailsClosed()
    {
        var source = new RecordingRetriever(RetrievalResult.NoResults());
        var service = new AuthorizedRetriever(source, new ThrowingPolicy());

        var result = await service.RetrieveAsync(CreateRequest(), CreateContext());

        Assert.Equal(RetrievalStatus.Failed, result.Status);
        Assert.Equal("retrieval.authorization_failed", result.Failure?.Code);
        Assert.DoesNotContain("private policy host", result.Failure?.Message ?? string.Empty);
        Assert.Equal(0, source.CallCount);
    }

    [Fact]
    public async Task RetrieveAsync_ProviderFailureIsSanitized()
    {
        var source = new RecordingRetriever(
            RetrievalResult.NoResults(),
            throwOnCall: true);
        var service = new AuthorizedRetriever(
            source,
            new FixedPolicy(RetrievalAuthorizationDecision.Allowed(new RetrievalScope("tenant-a"))));

        var result = await service.RetrieveAsync(CreateRequest(), CreateContext());

        Assert.Equal(RetrievalStatus.Failed, result.Status);
        Assert.Equal("retrieval.provider_failed", result.Failure?.Code);
        Assert.DoesNotContain("private storage account", result.Failure?.Message ?? string.Empty);
    }

    [Fact]
    public async Task RetrieveAsync_CancellationAfterAuthorizationPreventsDataSourceAccess()
    {
        using var cancellation = new CancellationTokenSource();
        var source = new RecordingRetriever(RetrievalResult.NoResults());
        var policy = new CancellingPolicy(cancellation);
        var service = new AuthorizedRetriever(source, policy);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            service.RetrieveAsync(CreateRequest(), CreateContext(), cancellation.Token));

        Assert.Equal(0, source.CallCount);
    }

    private static RetrievalRequest CreateRequest() =>
        new("account policy");

    private static RetrievalExecutionContext CreateContext(string tenantId = "tenant-a") =>
        new("user-1", tenantId, AgentRunContext.Create());

    private sealed class FixedPolicy(RetrievalAuthorizationDecision decision) : IRetrievalAuthorizationPolicy
    {
        public Task<RetrievalAuthorizationDecision> AuthorizeAsync(
            RetrievalRequest request,
            RetrievalExecutionContext context,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(decision);
        }
    }

    private sealed class ThrowingPolicy : IRetrievalAuthorizationPolicy
    {
        public Task<RetrievalAuthorizationDecision> AuthorizeAsync(
            RetrievalRequest request,
            RetrievalExecutionContext context,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("private policy host details");
        }
    }

    private sealed class CancellingPolicy(CancellationTokenSource cancellation) : IRetrievalAuthorizationPolicy
    {
        public Task<RetrievalAuthorizationDecision> AuthorizeAsync(
            RetrievalRequest request,
            RetrievalExecutionContext context,
            CancellationToken cancellationToken = default)
        {
            cancellation.Cancel();
            return Task.FromResult(
                RetrievalAuthorizationDecision.Allowed(new RetrievalScope(context.TenantId)));
        }
    }

    private sealed class RecordingRetriever(
        RetrievalResult result,
        bool throwOnCall = false) : IRetriever
    {
        public int CallCount { get; private set; }

        public Task<RetrievalResult> RetrieveAsync(
            RetrievalRequest request,
            RetrievalScope authorizedScope,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;

            if (throwOnCall)
            {
                throw new InvalidOperationException("private storage account details");
            }

            return Task.FromResult(result);
        }
    }
}
