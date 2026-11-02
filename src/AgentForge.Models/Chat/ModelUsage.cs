namespace AgentForge.Models.Chat;

/// <summary>
/// Represents usage information reported by a model provider.
/// </summary>
public sealed class ModelUsage
{
    /// <summary>
    /// Creates usage information. Unknown values should be left unspecified.
    /// </summary>
    /// <param name="inputTokens">The number of input tokens, when reported.</param>
    /// <param name="outputTokens">The number of output tokens, when reported.</param>
    /// <param name="totalTokens">The total token count, when reported.</param>
    public ModelUsage(long? inputTokens = null, long? outputTokens = null, long? totalTokens = null)
    {
        ValidateNonNegative(inputTokens, nameof(inputTokens));
        ValidateNonNegative(outputTokens, nameof(outputTokens));
        ValidateNonNegative(totalTokens, nameof(totalTokens));

        InputTokens = inputTokens;
        OutputTokens = outputTokens;
        TotalTokens = totalTokens;
    }

    /// <summary>
    /// Gets the reported input token count, if available.
    /// </summary>
    public long? InputTokens { get; }

    /// <summary>
    /// Gets the reported output token count, if available.
    /// </summary>
    public long? OutputTokens { get; }

    /// <summary>
    /// Gets the reported total token count, if available.
    /// </summary>
    public long? TotalTokens { get; }

    private static void ValidateNonNegative(long? value, string parameterName)
    {
        if (value is < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Token usage cannot be negative.");
        }
    }
}
