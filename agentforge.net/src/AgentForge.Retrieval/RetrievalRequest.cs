using System.Collections.ObjectModel;

namespace AgentForge.Retrieval;

/// <summary>
/// Represents a bounded retrieval request with application-supplied metadata filters.
/// </summary>
public sealed class RetrievalRequest
{
    /// <summary>Creates a retrieval request.</summary>
    public RetrievalRequest(
        string query,
        int maxResults = 6,
        IReadOnlyDictionary<string, string>? filters = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(query);

        if (maxResults <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxResults));
        }

        var copied = new Dictionary<string, string>(StringComparer.Ordinal);
        if (filters is not null)
        {
            foreach (var filter in filters)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(filter.Key);
                ArgumentNullException.ThrowIfNull(filter.Value);
                copied.Add(filter.Key, filter.Value);
            }
        }

        Query = query;
        MaxResults = maxResults;
        Filters = new ReadOnlyDictionary<string, string>(copied);
    }

    /// <summary>Gets the search query.</summary>
    public string Query { get; }

    /// <summary>Gets the maximum number of results requested.</summary>
    public int MaxResults { get; }

    /// <summary>Gets immutable metadata filters requested by the application.</summary>
    public IReadOnlyDictionary<string, string> Filters { get; }
}
