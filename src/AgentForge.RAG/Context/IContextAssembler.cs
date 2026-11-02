using AgentForge.Retrieval;

namespace AgentForge.RAG.Context;

/// <summary>
/// Converts retrieved chunks into a bounded, provenance-preserving context block.
/// </summary>
public interface IContextAssembler
{
    /// <summary>Assembles reference context up to the supplied character budget.</summary>
    AssembledContext Assemble(
        IReadOnlyList<RetrievedChunk> chunks,
        int maxCharacters);
}
