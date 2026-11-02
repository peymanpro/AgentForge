using AgentForge.Models.Chat;

namespace AgentForge.Agents.ReAct;

/// <summary>
/// Contains configuration and finite default limits for a ReAct agent.
/// </summary>
public sealed class ReActAgentOptions
{
    /// <summary>
    /// Creates ReAct agent options.
    /// </summary>
    /// <param name="systemPrompt">Optional application-controlled system instructions.</param>
    /// <param name="generationOptions">Provider-neutral options applied to each model turn.</param>
    /// <param name="maxModelTurns">The default maximum number of model requests in one run.</param>
    /// <param name="maxToolInvocations">The default maximum number of tool invocation attempts in one run.</param>
    /// <param name="timeout">The default overall execution timeout.</param>
    /// <param name="maxToolObservationCharacters">The maximum serialized tool observation size sent back to the model.</param>
    public ReActAgentOptions(
        string? systemPrompt = null,
        ModelGenerationOptions? generationOptions = null,
        int maxModelTurns = 8,
        int maxToolInvocations = 12,
        TimeSpan? timeout = null,
        int maxToolObservationCharacters = 16_384)
    {
        if (systemPrompt is not null && string.IsNullOrWhiteSpace(systemPrompt))
        {
            throw new ArgumentException("A configured system prompt cannot be empty or whitespace.", nameof(systemPrompt));
        }

        if (maxModelTurns <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxModelTurns), maxModelTurns, "The model-turn limit must be positive.");
        }

        if (maxToolInvocations <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxToolInvocations),
                maxToolInvocations,
                "The tool-invocation limit must be positive.");
        }

        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(45);
        if (effectiveTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(timeout), effectiveTimeout, "The timeout must be positive.");
        }

        if (maxToolObservationCharacters <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxToolObservationCharacters),
                maxToolObservationCharacters,
                "The observation character limit must be positive.");
        }

        SystemPrompt = systemPrompt;
        GenerationOptions = generationOptions ?? new ModelGenerationOptions();
        MaxModelTurns = maxModelTurns;
        MaxToolInvocations = maxToolInvocations;
        Timeout = effectiveTimeout;
        MaxToolObservationCharacters = maxToolObservationCharacters;
    }

    /// <summary>
    /// Gets the optional application-controlled system prompt.
    /// </summary>
    public string? SystemPrompt { get; }

    /// <summary>
    /// Gets the immutable generation options used for each model turn.
    /// </summary>
    public ModelGenerationOptions GenerationOptions { get; }

    /// <summary>
    /// Gets the default maximum number of model requests in one run.
    /// </summary>
    public int MaxModelTurns { get; }

    /// <summary>
    /// Gets the default maximum number of tool invocation attempts in one run.
    /// </summary>
    public int MaxToolInvocations { get; }

    /// <summary>
    /// Gets the default overall execution timeout.
    /// </summary>
    public TimeSpan Timeout { get; }

    /// <summary>
    /// Gets the maximum serialized tool observation size sent to the model.
    /// </summary>
    public int MaxToolObservationCharacters { get; }
}
