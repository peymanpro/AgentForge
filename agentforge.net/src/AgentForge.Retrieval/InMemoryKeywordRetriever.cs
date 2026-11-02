using System.Text;

namespace AgentForge.Retrieval;

/// <summary>
/// Provides a deterministic in-memory keyword-overlap retriever for examples and tests.
/// </summary>
/// <remarks>
/// This implementation is a simple lexical baseline, not a vector search or semantic embedding retriever.
/// Its score is the proportion of unique query terms found in the title or content.
/// </remarks>
public sealed class InMemoryKeywordRetriever : IRetriever
{
    private const string ScoreMeaning = "normalized-unique-query-term-overlap";
    private readonly IReadOnlyList<DocumentChunk> _documents;

    /// <summary>Creates an in-memory retriever from a fixed document collection.</summary>
    public InMemoryKeywordRetriever(IEnumerable<DocumentChunk> documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        var materialized = documents.ToArray();
        if (materialized.Any(static document => document is null))
        {
            throw new ArgumentException("The document collection cannot contain null values.", nameof(documents));
        }

        if (materialized.Select(static document => document.ChunkId).Distinct(StringComparer.Ordinal).Count() != materialized.Length)
        {
            throw new ArgumentException("Chunk identifiers must be unique.", nameof(documents));
        }

        _documents = Array.AsReadOnly(materialized);
    }

    /// <inheritdoc />
    public Task<RetrievalResult> RetrieveAsync(
        RetrievalRequest request,
        RetrievalScope authorizedScope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(authorizedScope);
        cancellationToken.ThrowIfCancellationRequested();

        var queryTerms = Tokenize(request.Query);
        if (queryTerms.Count == 0)
        {
            return Task.FromResult(RetrievalResult.NoResults());
        }

        var allowedDocuments = authorizedScope.AllowedDocumentIds is null
            ? null
            : new HashSet<string>(authorizedScope.AllowedDocumentIds, StringComparer.Ordinal);

        var candidates = new List<RetrievedChunk>();

        foreach (var document in _documents)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!string.Equals(document.TenantId, authorizedScope.TenantId, StringComparison.Ordinal))
            {
                continue;
            }

            if (allowedDocuments is not null && !allowedDocuments.Contains(document.DocumentId))
            {
                continue;
            }

            if (!MatchesFilters(document, request.Filters) ||
                !MatchesFilters(document, authorizedScope.MetadataFilters))
            {
                continue;
            }

            var searchableText = $"{document.Title} {document.Content}";
            var documentTerms = Tokenize(searchableText);
            var matchedTerms = queryTerms.Count(documentTerms.Contains);
            if (matchedTerms == 0)
            {
                continue;
            }

            var score = (double)matchedTerms / queryTerms.Count;
            candidates.Add(new RetrievedChunk(document, score, ScoreMeaning));
        }

        var results = candidates
            .OrderByDescending(static item => item.Score)
            .ThenBy(static item => item.Chunk.DocumentId, StringComparer.Ordinal)
            .ThenBy(static item => item.Chunk.ChunkIndex)
            .ThenBy(static item => item.Chunk.ChunkId, StringComparer.Ordinal)
            .Take(request.MaxResults)
            .ToArray();

        return Task.FromResult(RetrievalResult.Success(results));
    }

    private static bool MatchesFilters(
        DocumentChunk document,
        IReadOnlyDictionary<string, string> filters)
    {
        foreach (var filter in filters)
        {
            if (!document.Metadata.TryGetValue(filter.Key, out var value) ||
                !string.Equals(value, filter.Value, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }

    private static HashSet<string> Tokenize(string text)
    {
        var builder = new StringBuilder(text.Length);
        foreach (var character in text)
        {
            builder.Append(char.IsLetterOrDigit(character) ? char.ToLowerInvariant(character) : ' ');
        }

        return builder.ToString()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.Ordinal);
    }
}
