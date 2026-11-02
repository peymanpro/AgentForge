using System.Collections.ObjectModel;

namespace AgentForge.Retrieval;

/// <summary>
/// Represents one indexed document chunk with source provenance and tenant ownership.
/// </summary>
public sealed class DocumentChunk
{
    /// <summary>
    /// Creates a document chunk.
    /// </summary>
    public DocumentChunk(
        string chunkId,
        string documentId,
        string tenantId,
        string content,
        string? title = null,
        string? sourceUri = null,
        int? chunkIndex = null,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(chunkId);
        ArgumentException.ThrowIfNullOrWhiteSpace(documentId);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);
        ArgumentNullException.ThrowIfNull(content);

        if (chunkIndex is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(chunkIndex));
        }

        var copied = new Dictionary<string, string>(StringComparer.Ordinal);
        if (metadata is not null)
        {
            foreach (var item in metadata)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(item.Key);
                ArgumentNullException.ThrowIfNull(item.Value);
                copied.Add(item.Key, item.Value);
            }
        }

        ChunkId = chunkId;
        DocumentId = documentId;
        TenantId = tenantId;
        Content = content;
        Title = title;
        SourceUri = sourceUri;
        ChunkIndex = chunkIndex;
        Metadata = new ReadOnlyDictionary<string, string>(copied);
    }

    /// <summary>Gets the stable chunk identifier.</summary>
    public string ChunkId { get; }

    /// <summary>Gets the parent document identifier.</summary>
    public string DocumentId { get; }

    /// <summary>Gets the tenant that owns this document chunk.</summary>
    public string TenantId { get; }

    /// <summary>Gets the chunk content.</summary>
    public string Content { get; }

    /// <summary>Gets the optional document title.</summary>
    public string? Title { get; }

    /// <summary>Gets the optional source URI.</summary>
    public string? SourceUri { get; }

    /// <summary>Gets the optional zero-based chunk index.</summary>
    public int? ChunkIndex { get; }

    /// <summary>Gets immutable application metadata.</summary>
    public IReadOnlyDictionary<string, string> Metadata { get; }
}
