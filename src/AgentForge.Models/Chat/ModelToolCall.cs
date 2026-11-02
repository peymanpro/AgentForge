using System.Text.Json;

namespace AgentForge.Models.Chat;

/// <summary>
/// Represents a normalized tool-call proposal returned by a model.
/// </summary>
public sealed class ModelToolCall
{
    private readonly JsonElement _arguments;

    /// <summary>
    /// Creates a normalized tool-call proposal.
    /// </summary>
    /// <param name="id">The provider-independent correlation identifier for this tool call.</param>
    /// <param name="name">The requested tool name.</param>
    /// <param name="arguments">The proposed JSON object arguments.</param>
    public ModelToolCall(string id, string name, JsonElement arguments)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (arguments.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException("Tool-call arguments must be a JSON object.", nameof(arguments));
        }

        Id = id;
        Name = name;
        _arguments = arguments.Clone();
    }

    /// <summary>
    /// Gets the tool-call correlation identifier.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets the requested tool name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets an owned copy of the proposed arguments.
    /// </summary>
    public JsonElement Arguments => _arguments;
}
