namespace AgentForge.Models.Chat;

/// <summary>
/// Represents one normalized message exchanged during a model interaction.
/// </summary>
public sealed class ChatMessage
{
    private ChatMessage(
        ChatMessageRole role,
        string? content,
        string? toolCallId,
        IReadOnlyList<ModelToolCall> toolCalls)
    {
        Role = role;
        Content = content;
        ToolCallId = toolCallId;
        ToolCalls = toolCalls;
    }

    /// <summary>
    /// Gets the semantic role of the message.
    /// </summary>
    public ChatMessageRole Role { get; }

    /// <summary>
    /// Gets the textual content, when present.
    /// </summary>
    public string? Content { get; }

    /// <summary>
    /// Gets the associated tool-call identifier for a tool-result message.
    /// </summary>
    public string? ToolCallId { get; }

    /// <summary>
    /// Gets the tool-call proposals attached to an assistant message.
    /// </summary>
    public IReadOnlyList<ModelToolCall> ToolCalls { get; }

    /// <summary>
    /// Creates a system instruction message.
    /// </summary>
    public static ChatMessage System(string content)
    {
        return WithContent(ChatMessageRole.System, content);
    }

    /// <summary>
    /// Creates a developer instruction message.
    /// </summary>
    public static ChatMessage Developer(string content)
    {
        return WithContent(ChatMessageRole.Developer, content);
    }

    /// <summary>
    /// Creates a user message.
    /// </summary>
    public static ChatMessage User(string content)
    {
        return WithContent(ChatMessageRole.User, content);
    }

    /// <summary>
    /// Creates an assistant message containing a final or intermediate text response.
    /// </summary>
    public static ChatMessage Assistant(string content)
    {
        return WithContent(ChatMessageRole.Assistant, content);
    }

    /// <summary>
    /// Creates an assistant message containing one or more tool-call proposals.
    /// </summary>
    /// <param name="toolCalls">The tool-call proposals returned by the model.</param>
    /// <param name="content">Optional assistant content accompanying the proposals.</param>
    /// <returns>An immutable assistant message.</returns>
    public static ChatMessage AssistantToolCalls(
        IEnumerable<ModelToolCall> toolCalls,
        string? content = null)
    {
        ArgumentNullException.ThrowIfNull(toolCalls);

        var materialized = toolCalls.ToArray();
        if (materialized.Length == 0)
        {
            throw new ArgumentException("At least one tool-call proposal is required.", nameof(toolCalls));
        }

        if (materialized.Any(static call => call is null))
        {
            throw new ArgumentException("Tool-call proposals cannot contain null values.", nameof(toolCalls));
        }

        var duplicateId = materialized
            .GroupBy(static call => call.Id, StringComparer.Ordinal)
            .Any(static group => group.Count() > 1);

        if (duplicateId)
        {
            throw new ArgumentException("Tool-call identifiers must be unique within a message.", nameof(toolCalls));
        }

        return new ChatMessage(
            ChatMessageRole.Assistant,
            content,
            null,
            Array.AsReadOnly(materialized));
    }

    /// <summary>
    /// Creates a tool-result message correlated with a prior assistant tool-call proposal.
    /// </summary>
    public static ChatMessage ToolResult(string toolCallId, string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolCallId);
        ArgumentNullException.ThrowIfNull(content);

        return new ChatMessage(
            ChatMessageRole.Tool,
            content,
            toolCallId,
            Array.Empty<ModelToolCall>());
    }

    private static ChatMessage WithContent(ChatMessageRole role, string content)
    {
        ArgumentNullException.ThrowIfNull(content);

        return new ChatMessage(role, content, null, Array.Empty<ModelToolCall>());
    }
}
