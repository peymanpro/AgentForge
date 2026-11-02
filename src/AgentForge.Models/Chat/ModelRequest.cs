namespace AgentForge.Models.Chat;

/// <summary>
/// Represents a normalized request for one model generation operation.
/// </summary>
public sealed class ModelRequest
{
    /// <summary>
    /// Creates a model request with an immutable message collection.
    /// </summary>
    /// <param name="messages">The ordered conversation messages.</param>
    /// <param name="options">Optional provider-neutral generation options.</param>
    public ModelRequest(
        IEnumerable<ChatMessage> messages,
        ModelGenerationOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(messages);

        var materialized = messages.ToArray();
        if (materialized.Length == 0)
        {
            throw new ArgumentException("A model request must contain at least one message.", nameof(messages));
        }

        if (materialized.Any(static message => message is null))
        {
            throw new ArgumentException("A model request cannot contain null messages.", nameof(messages));
        }

        Messages = Array.AsReadOnly(materialized);
        Options = options ?? new ModelGenerationOptions();
    }

    /// <summary>
    /// Gets the ordered conversation messages.
    /// </summary>
    public IReadOnlyList<ChatMessage> Messages { get; }

    /// <summary>
    /// Gets the generation options.
    /// </summary>
    public ModelGenerationOptions Options { get; }
}
