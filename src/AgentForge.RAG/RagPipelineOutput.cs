using AgentForge.Models.Chat;
using AgentForge.Retrieval;

namespace AgentForge.RAG;

/// <summary>
/// Contains the generated answer and the provenance of the reference chunks supplied to the model.
/// </summary>
public sealed class RagPipelineOutput
{
    internal RagPipelineOutput(
        string? answer,
        RetrievalStatus retrievalStatus,
        IReadOnlyList<RetrievedChunk> retrievedChunks,
        IReadOnlyList<RetrievedChunk> includedChunks,
        IReadOnlyList<string> omittedChunkIds,
        ModelUsage? modelUsage)
    {
        Answer = answer;
        RetrievalStatus = retrievalStatus;
        RetrievedChunks = Array.AsReadOnly(retrievedChunks.ToArray());
        IncludedChunks = Array.AsReadOnly(includedChunks.ToArray());
        OmittedChunkIds = Array.AsReadOnly(omittedChunkIds.ToArray());
        ModelUsage = modelUsage;
    }

    /// <summary>Gets the generated answer, when generation was performed.</summary>
    public string? Answer { get; }

    /// <summary>Gets the normalized retrieval status.</summary>
    public RetrievalStatus RetrievalStatus { get; }

    /// <summary>Gets all chunks returned by the authorized retriever.</summary>
    public IReadOnlyList<RetrievedChunk> RetrievedChunks { get; }

    /// <summary>Gets the chunks actually supplied to the model.</summary>
    public IReadOnlyList<RetrievedChunk> IncludedChunks { get; }

    /// <summary>Gets the identifiers of retrieved chunks omitted by the context budget.</summary>
    public IReadOnlyList<string> OmittedChunkIds { get; }

    /// <summary>Gets the usage reported by the model provider, when available.</summary>
    public ModelUsage? ModelUsage { get; }

    /// <summary>Gets whether no evidence was supplied to the model.</summary>
    public bool NoEvidenceFound => IncludedChunks.Count == 0;
}
