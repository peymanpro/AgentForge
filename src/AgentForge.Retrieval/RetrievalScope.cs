using System.Collections.ObjectModel;

namespace AgentForge.Retrieval;

/// <summary>
/// Describes the data scope that an application policy has authorized for one retrieval operation.
/// </summary>
public sealed class RetrievalScope
{
    /// <summary>
    /// Creates an authorized retrieval scope.
    /// </summary>
    /// <param name="tenantId">The tenant boundary to enforce.</param>
    /// <param name="allowedDocumentIds">A document allowlist. Null means all documents within the authorized tenant; an empty list means none.</param>
    /// <param name="metadataFilters">Additional mandatory metadata filters.</param>
    public RetrievalScope(
        string tenantId,
        IEnumerable<string>? allowedDocumentIds = null,
        IReadOnlyDictionary<string, string>? metadataFilters = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        var documentIds = allowedDocumentIds?.ToArray();
        if (documentIds?.Any(string.IsNullOrWhiteSpace) == true)
        {
            throw new ArgumentException("Document identifiers cannot be empty.", nameof(allowedDocumentIds));
        }

        var filters = new Dictionary<string, string>(StringComparer.Ordinal);
        if (metadataFilters is not null)
        {
            foreach (var filter in metadataFilters)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(filter.Key);
                ArgumentNullException.ThrowIfNull(filter.Value);
                filters.Add(filter.Key, filter.Value);
            }
        }

        TenantId = tenantId;
        AllowedDocumentIds = documentIds is null
            ? null
            : Array.AsReadOnly(documentIds.Distinct(StringComparer.Ordinal).ToArray());
        MetadataFilters = new ReadOnlyDictionary<string, string>(filters);
    }

    /// <summary>Gets the authorized tenant boundary.</summary>
    public string TenantId { get; }

    /// <summary>Gets the optional document allowlist.</summary>
    public IReadOnlyList<string>? AllowedDocumentIds { get; }

    /// <summary>Gets immutable mandatory metadata filters.</summary>
    public IReadOnlyDictionary<string, string> MetadataFilters { get; }
}
