using AgentForge.Retrieval;
using Xunit;

namespace AgentForge.Retrieval.Tests;

public sealed class InMemoryKeywordRetrieverTests
{
    [Fact]
    public async Task RetrieveAsync_EnforcesTenantBoundary()
    {
        var retriever = new InMemoryKeywordRetriever(
        [
            CreateChunk("chunk-a", "doc-a", "tenant-a", "approved policy guidance"),
            CreateChunk("chunk-b", "doc-b", "tenant-b", "approved policy guidance")
        ]);

        var result = await retriever.RetrieveAsync(
            new RetrievalRequest("approved policy"),
            new RetrievalScope("tenant-a"));

        Assert.Equal(RetrievalStatus.Success, result.Status);
        Assert.Single(result.Items);
        Assert.Equal("tenant-a", result.Items[0].Chunk.TenantId);
        Assert.Equal("doc-a", result.Items[0].Chunk.DocumentId);
    }

    [Fact]
    public async Task RetrieveAsync_EnforcesDocumentAllowlist()
    {
        var retriever = new InMemoryKeywordRetriever(
        [
            CreateChunk("chunk-a", "doc-a", "tenant-a", "security policy"),
            CreateChunk("chunk-b", "doc-b", "tenant-a", "security policy")
        ]);

        var result = await retriever.RetrieveAsync(
            new RetrievalRequest("security policy"),
            new RetrievalScope("tenant-a", ["doc-b"]));

        Assert.Single(result.Items);
        Assert.Equal("doc-b", result.Items[0].Chunk.DocumentId);
    }

    [Fact]
    public async Task RetrieveAsync_EmptyDocumentAllowlistReturnsNoResults()
    {
        var retriever = new InMemoryKeywordRetriever(
        [
            CreateChunk("chunk-a", "doc-a", "tenant-a", "security policy")
        ]);

        var result = await retriever.RetrieveAsync(
            new RetrievalRequest("security policy"),
            new RetrievalScope("tenant-a", Array.Empty<string>()));

        Assert.Equal(RetrievalStatus.NoResults, result.Status);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task RetrieveAsync_CombinesRequestAndMandatoryMetadataFilters()
    {
        var retriever = new InMemoryKeywordRetriever(
        [
            CreateChunk("approved-finance", "doc-a", "tenant-a", "monthly budget policy",
                new Dictionary<string, string> { ["category"] = "policy", ["state"] = "approved" }),
            CreateChunk("draft-finance", "doc-b", "tenant-a", "monthly budget policy",
                new Dictionary<string, string> { ["category"] = "policy", ["state"] = "draft" }),
            CreateChunk("approved-ops", "doc-c", "tenant-a", "monthly budget policy",
                new Dictionary<string, string> { ["category"] = "procedure", ["state"] = "approved" })
        ]);

        var result = await retriever.RetrieveAsync(
            new RetrievalRequest(
                "monthly budget policy",
                filters: new Dictionary<string, string> { ["category"] = "policy" }),
            new RetrievalScope(
                "tenant-a",
                metadataFilters: new Dictionary<string, string> { ["state"] = "approved" }));

        Assert.Single(result.Items);
        Assert.Equal("approved-finance", result.Items[0].Chunk.ChunkId);
    }

    [Fact]
    public async Task RetrieveAsync_ReturnsDeterministicRankingAndHonorsLimit()
    {
        var retriever = new InMemoryKeywordRetriever(
        [
            CreateChunk("chunk-c", "doc-c", "tenant-a", "alpha beta"),
            CreateChunk("chunk-a", "doc-a", "tenant-a", "alpha beta"),
            CreateChunk("chunk-b", "doc-b", "tenant-a", "alpha beta")
        ]);

        var result = await retriever.RetrieveAsync(
            new RetrievalRequest("alpha beta", maxResults: 2),
            new RetrievalScope("tenant-a"));

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(new[] { "doc-a", "doc-b" }, result.Items.Select(item => item.Chunk.DocumentId));
        Assert.All(result.Items, item => Assert.Equal(1.0, item.Score));
        Assert.All(result.Items, item => Assert.Equal(
            "normalized-unique-query-term-overlap",
            item.ScoreSemantics));
    }

    [Fact]
    public void RetrievalResult_SuccessRejectsNullItems()
    {
        Assert.Throws<ArgumentException>(() =>
            RetrievalResult.Success([null!]));
    }

    private static DocumentChunk CreateChunk(
        string chunkId,
        string documentId,
        string tenantId,
        string content,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        return new DocumentChunk(
            chunkId,
            documentId,
            tenantId,
            content,
            title: "Test document",
            sourceUri: $"https://example.test/{documentId}",
            metadata: metadata);
    }
}
