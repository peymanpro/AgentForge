using AgentForge.Models.Chat;

namespace AgentForge.RAG;

/// <summary>
/// Contains finite limits and generation settings for a RAG pipeline.
/// </summary>
public sealed class RagPipelineOptions
{
    /// <summary>Creates RAG pipeline options.</summary>
    public RagPipelineOptions(
        int maxResults = 6,
        int maxContextCharacters = 18_000,
        TimeSpan? timeout = null,
        string? applicationSystemPrompt = null,
        ModelGenerationOptions? generationOptions = null)
    {
        if (maxResults <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxResults));
        }

        if (maxContextCharacters <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxContextCharacters));
        }

        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(60);
        if (effectiveTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), effectiveTimeout, "Timeout must be greater than zero.");
        }

        if (applicationSystemPrompt is not null && string.IsNullOrWhiteSpace(applicationSystemPrompt))
        {
            throw new ArgumentException("A configured system prompt cannot be empty or whitespace.", nameof(applicationSystemPrompt));
        }

        MaxResults = maxResults;
        MaxContextCharacters = maxContextCharacters;
        Timeout = effectiveTimeout;
        ApplicationSystemPrompt = applicationSystemPrompt;
        GenerationOptions = generationOptions ?? new ModelGenerationOptions();
    }

    /// <summary>Gets the maximum number of chunks requested from the retriever.</summary>
    public int MaxResults { get; }

    /// <summary>Gets the maximum number of characters assembled into model context.</summary>
    public int MaxContextCharacters { get; }

    /// <summary>Gets the overall retrieval and generation timeout.</summary>
    public TimeSpan Timeout { get; }

    /// <summary>Gets the optional application system instructions appended to the built-in evidence policy.</summary>
    public string? ApplicationSystemPrompt { get; }

    /// <summary>Gets the provider-neutral generation options.</summary>
    public ModelGenerationOptions GenerationOptions { get; }
}
