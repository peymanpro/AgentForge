namespace AgentForge.Retrieval;

/// <summary>
/// Represents a retrieved chunk with a score whose meaning is defined by the retriever.
/// </summary>
public sealed record RetrievedChunk
{
    /// <summary>Creates a retrieved chunk.</summary>
    public RetrievedChunk(DocumentChunk chunk, double score, string scoreSemantics)
    {
        ArgumentNullException.ThrowIfNull(chunk);
        ArgumentException.ThrowIfNullOrWhiteSpace(scoreSemantics);

        if (!double.IsFinite(score) || score < 0 || score > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(score), score, "The normalized retrieval score must be between zero and one.");
        }

        Chunk = chunk;
        Score = score;
        ScoreSemantics = scoreSemantics;
    }

    /// <summary>Gets the original chunk with provenance.</summary>
    public DocumentChunk Chunk { get; }

    /// <summary>Gets the normalized relevance score.</summary>
    public double Score { get; }

    /// <summary>Gets the documented meaning of the score.</summary>
    public string ScoreSemantics { get; }
}
