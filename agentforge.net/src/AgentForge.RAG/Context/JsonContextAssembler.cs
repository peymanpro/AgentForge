using System.Text.Json;
using AgentForge.Retrieval;

namespace AgentForge.RAG.Context;

/// <summary>
/// Serializes each retrieved chunk as a separate JSON reference and enforces a hard character budget.
/// </summary>
public sealed class JsonContextAssembler : IContextAssembler
{
    /// <inheritdoc />
    public AssembledContext Assemble(
        IReadOnlyList<RetrievedChunk> chunks,
        int maxCharacters)
    {
        ArgumentNullException.ThrowIfNull(chunks);

        if (maxCharacters <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCharacters));
        }

        if (chunks.Any(static chunk => chunk is null))
        {
            throw new ArgumentException("Retrieved chunks cannot contain null values.", nameof(chunks));
        }

        var blocks = new List<(RetrievedChunk Chunk, string Text)>();
        foreach (var item in chunks)
        {
            var block = JsonSerializer.Serialize(new
            {
                chunk_id = item.Chunk.ChunkId,
                document_id = item.Chunk.DocumentId,
                title = item.Chunk.Title,
                source_uri = item.Chunk.SourceUri,
                chunk_index = item.Chunk.ChunkIndex,
                score = item.Score,
                score_semantics = item.ScoreSemantics,
                content = item.Chunk.Content
            });

            blocks.Add((item, block));
        }

        var included = new List<RetrievedChunk>();
        var omitted = new List<string>();
        var context = new System.Text.StringBuilder();

        foreach (var block in blocks)
        {
            var separatorLength = context.Length == 0 ? 0 : Environment.NewLine.Length;
            var requiredLength = separatorLength + block.Text.Length;

            if (context.Length + requiredLength > maxCharacters)
            {
                omitted.Add(block.Chunk.Chunk.ChunkId);
                continue;
            }

            if (separatorLength > 0)
            {
                context.AppendLine();
            }

            context.Append(block.Text);
            included.Add(block.Chunk);
        }

        return new AssembledContext(context.ToString(), included, omitted);
    }
}
