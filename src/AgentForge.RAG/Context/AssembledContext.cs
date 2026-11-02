using AgentForge.Retrieval;

namespace AgentForge.RAG.Context;

/// <summary>
/// Represents a bounded context string and the exact source chunks included or omitted.
/// </summary>
public sealed class AssembledContext
{
    internal AssembledContext(
        string text,
        IReadOnlyList<RetrievedChunk> includedChunks,
        IReadOnlyList<string> omittedChunkIds)
    {
        Text = text;
        IncludedChunks = Array.AsReadOnly(includedChunks.ToArray());
        OmittedChunkIds = Array.AsReadOnly(omittedChunkIds.ToArray());
    }

    /// <summary>Gets the serialized reference context.</summary>
    public string Text { get; }

    /// <summary>Gets the source chunks included in the context.</summary>
    public IReadOnlyList<RetrievedChunk> IncludedChunks { get; }

    /// <summary>Gets the identifiers of retrieved chunks omitted by the context budget.</summary>
    public IReadOnlyList<string> OmittedChunkIds { get; }
}
