using AgentForge.Retrieval;

namespace AgentForge.RAG;

/// <summary>
/// Represents a user question and the application identity used to authorize retrieval.
/// </summary>
public sealed class RagQuery
{
    /// <summary>Creates a RAG query.</summary>
    public RagQuery(
        string question,
        RetrievalExecutionContext retrievalContext,
        IReadOnlyDictionary<string, string>? filters = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(question);
        ArgumentNullException.ThrowIfNull(retrievalContext);

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

        Question = question;
        RetrievalContext = retrievalContext;
        Filters = new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(copied);
    }

    /// <summary>Gets the user's question.</summary>
    public string Question { get; }

    /// <summary>Gets the application identity and run context for retrieval authorization.</summary>
    public RetrievalExecutionContext RetrievalContext { get; }

    /// <summary>Gets immutable application-supplied retrieval filters.</summary>
    public IReadOnlyDictionary<string, string> Filters { get; }
}
