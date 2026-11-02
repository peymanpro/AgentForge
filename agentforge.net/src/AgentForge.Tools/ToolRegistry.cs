using System.Collections.ObjectModel;

namespace AgentForge.Tools;

/// <summary>
/// Provides an immutable, explicitly scoped collection of tools.
/// </summary>
public sealed class ToolRegistry
{
    private readonly IReadOnlyDictionary<string, ITool> _tools;
    private readonly IReadOnlyList<ToolDefinition> _definitions;

    /// <summary>
    /// Creates a registry from the tools available to one application or execution scope.
    /// </summary>
    /// <param name="tools">The tools to register.</param>
    public ToolRegistry(IEnumerable<ITool> tools)
    {
        ArgumentNullException.ThrowIfNull(tools);

        var map = new Dictionary<string, ITool>(StringComparer.Ordinal);
        foreach (var tool in tools)
        {
            ArgumentNullException.ThrowIfNull(tool);

            var definition = tool.Definition
                ?? throw new ArgumentException("A registered tool must provide a definition.", nameof(tools));

            if (!map.TryAdd(definition.Name, tool))
            {
                throw new ArgumentException(
                    $"A tool named '{definition.Name}' is already registered.",
                    nameof(tools));
            }
        }

        _tools = new ReadOnlyDictionary<string, ITool>(map);
        _definitions = Array.AsReadOnly(map.Values.Select(static tool => tool.Definition).ToArray());
    }

    /// <summary>
    /// Gets the definitions of the tools registered in this scope.
    /// </summary>
    public IReadOnlyList<ToolDefinition> Definitions => _definitions;

    /// <summary>
    /// Attempts to resolve a tool by its exact, case-sensitive name.
    /// </summary>
    /// <param name="name">The requested tool name.</param>
    /// <param name="tool">The resolved tool, when found.</param>
    /// <returns>True when the tool is present in this registry.</returns>
    public bool TryGet(string name, out ITool? tool)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return _tools.TryGetValue(name, out tool);
    }
}
