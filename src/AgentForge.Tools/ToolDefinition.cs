using System.Text.Json;

namespace AgentForge.Tools;

/// <summary>
/// Describes a tool that can be exposed to an agent decision strategy.
/// </summary>
public sealed class ToolDefinition
{
    private readonly JsonElement _inputSchema;

    /// <summary>
    /// Creates a tool definition.
    /// </summary>
    /// <param name="name">A stable tool name containing letters, digits, underscores, hyphens, or periods.</param>
    /// <param name="description">A concise description intended for developers and model decision strategies.</param>
    /// <param name="inputSchema">A JSON object describing the tool's input shape.</param>
    public ToolDefinition(string name, string description, JsonElement inputSchema)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        if (name.Length > 64 || !name.All(static character =>
                char.IsAsciiLetterOrDigit(character) || character is '_' or '-' or '.'))
        {
            throw new ArgumentException(
                "A tool name must be at most 64 characters and contain only ASCII letters, digits, underscores, hyphens, or periods.",
                nameof(name));
        }

        if (inputSchema.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException("The input schema must be a JSON object.", nameof(inputSchema));
        }

        Name = name;
        Description = description;
        _inputSchema = inputSchema.Clone();
    }

    /// <summary>
    /// Gets the stable tool name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the description supplied to developers and model decision strategies.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets an owned copy of the JSON input schema.
    /// </summary>
    public JsonElement InputSchema => _inputSchema;
}
