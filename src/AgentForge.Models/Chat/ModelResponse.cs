namespace AgentForge.Models.Chat;

/// <summary>
/// Represents a normalized model response and its reported finish condition.
/// </summary>
public sealed class ModelResponse
{
    /// <summary>
    /// Creates a normalized response.
    /// </summary>
    /// <param name="message">The assistant message returned by the model.</param>
    /// <param name="finishReason">The normalized reason the provider ended the response.</param>
    /// <param name="usage">Optional usage data reported by the provider.</param>
    public ModelResponse(
        ChatMessage message,
        ModelFinishReason finishReason,
        ModelUsage? usage = null)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!Enum.IsDefined(finishReason))
        {
            throw new ArgumentOutOfRangeException(
                nameof(finishReason),
                finishReason,
                "The finish reason is not defined.");
        }

        if (message.Role != ChatMessageRole.Assistant)
        {
            throw new ArgumentException("A model response must contain an assistant message.", nameof(message));
        }

        var hasToolCalls = message.ToolCalls.Count > 0;
        if (hasToolCalls != (finishReason == ModelFinishReason.ToolCalls))
        {
            throw new ArgumentException(
                "The ToolCalls finish reason must correspond to an assistant message containing tool-call proposals.",
                nameof(finishReason));
        }

        Message = message;
        FinishReason = finishReason;
        Usage = usage;
    }

    /// <summary>
    /// Gets the assistant message.
    /// </summary>
    public ChatMessage Message { get; }

    /// <summary>
    /// Gets the normalized finish reason.
    /// </summary>
    public ModelFinishReason FinishReason { get; }

    /// <summary>
    /// Gets the usage information reported by the provider, when available.
    /// </summary>
    public ModelUsage? Usage { get; }
}
