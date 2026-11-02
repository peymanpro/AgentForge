namespace AgentForge.Models.Chat;

/// <summary>
/// Contains provider-neutral generation options with common, documented semantics.
/// </summary>
public sealed class ModelGenerationOptions
{
    /// <summary>
    /// Creates generation options.
    /// </summary>
    /// <param name="modelId">An optional provider-specific model or deployment identifier.</param>
    /// <param name="maxOutputTokens">An optional maximum number of output tokens.</param>
    /// <param name="temperature">An optional non-negative sampling temperature.</param>
    /// <param name="topP">An optional nucleus-sampling value in the range (0, 1].</param>
    /// <param name="stopSequences">Optional stop sequences.</param>
    public ModelGenerationOptions(
        string? modelId = null,
        int? maxOutputTokens = null,
        double? temperature = null,
        double? topP = null,
        IEnumerable<string>? stopSequences = null)
    {
        if (modelId is not null && string.IsNullOrWhiteSpace(modelId))
        {
            throw new ArgumentException("A model identifier cannot be empty or whitespace.", nameof(modelId));
        }

        if (maxOutputTokens is <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxOutputTokens),
                maxOutputTokens,
                "The output token limit must be greater than zero.");
        }

        if (temperature is { } value && (!double.IsFinite(value) || value < 0))
        {
            throw new ArgumentOutOfRangeException(
                nameof(temperature),
                temperature,
                "Temperature must be a finite, non-negative value.");
        }

        if (topP is { } probability && (!double.IsFinite(probability) || probability <= 0 || probability > 1))
        {
            throw new ArgumentOutOfRangeException(
                nameof(topP),
                topP,
                "Top-p must be a finite value greater than zero and at most one.");
        }

        var stops = stopSequences?.ToArray() ?? Array.Empty<string>();
        if (stops.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Stop sequences cannot contain null, empty, or whitespace values.", nameof(stopSequences));
        }

        ModelId = modelId;
        MaxOutputTokens = maxOutputTokens;
        Temperature = temperature;
        TopP = topP;
        StopSequences = Array.AsReadOnly(stops);
    }

    /// <summary>
    /// Gets the optional provider-specific model or deployment identifier.
    /// </summary>
    public string? ModelId { get; }

    /// <summary>
    /// Gets the optional maximum number of output tokens.
    /// </summary>
    public int? MaxOutputTokens { get; }

    /// <summary>
    /// Gets the optional non-negative sampling temperature.
    /// </summary>
    public double? Temperature { get; }

    /// <summary>
    /// Gets the optional nucleus-sampling value in the range (0, 1].
    /// </summary>
    public double? TopP { get; }

    /// <summary>
    /// Gets the immutable collection of stop sequences.
    /// </summary>
    public IReadOnlyList<string> StopSequences { get; }
}
