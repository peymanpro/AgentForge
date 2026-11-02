using System.Text.Json;
using AgentForge.Abstractions.Execution;

namespace AgentForge.Tools;

/// <summary>
/// Contains the information an application policy needs to authorize a tool invocation.
/// </summary>
public sealed class ToolAuthorizationRequest
{
    /// <summary>
    /// Creates an authorization request.
    /// </summary>
    /// <param name="tool">The declared tool being requested.</param>
    /// <param name="arguments">The validated arguments for the requested operation.</param>
    /// <param name="runContext">The current agent run context.</param>
    public ToolAuthorizationRequest(
        ToolDefinition tool,
        JsonElement arguments,
        AgentRunContext runContext)
    {
        ArgumentNullException.ThrowIfNull(tool);
        ArgumentNullException.ThrowIfNull(runContext);

        Tool = tool;
        Arguments = arguments.Clone();
        RunContext = runContext;
    }

    /// <summary>
    /// Gets the declared tool.
    /// </summary>
    public ToolDefinition Tool { get; }

    /// <summary>
    /// Gets an owned copy of the validated arguments.
    /// </summary>
    public JsonElement Arguments { get; }

    /// <summary>
    /// Gets the run context for correlation or application policy.
    /// </summary>
    public AgentRunContext RunContext { get; }
}
