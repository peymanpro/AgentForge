using System.Text.Json;
using AgentForge.Abstractions.Execution;

namespace AgentForge.Tools;

/// <summary>
/// Represents a model-proposed tool invocation.
/// </summary>
public sealed class ToolExecutionRequest
{
    /// <summary>
    /// Creates a tool execution request.
    /// </summary>
    /// <param name="toolCallId">A correlation identifier for this invocation.</param>
    /// <param name="toolName">The requested tool name.</param>
    /// <param name="arguments">The untrusted JSON arguments proposed by the model.</param>
    /// <param name="runContext">The current agent run context.</param>
    public ToolExecutionRequest(
        string toolCallId,
        string toolName,
        JsonElement arguments,
        AgentRunContext runContext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolCallId);
        ArgumentException.ThrowIfNullOrWhiteSpace(toolName);
        ArgumentNullException.ThrowIfNull(runContext);

        ToolCallId = toolCallId;
        ToolName = toolName;
        Arguments = arguments.Clone();
        RunContext = runContext;
    }

    /// <summary>
    /// Gets the invocation correlation identifier.
    /// </summary>
    public string ToolCallId { get; }

    /// <summary>
    /// Gets the requested tool name.
    /// </summary>
    public string ToolName { get; }

    /// <summary>
    /// Gets an owned copy of the untrusted arguments.
    /// </summary>
    public JsonElement Arguments { get; }

    /// <summary>
    /// Gets the current agent run context.
    /// </summary>
    public AgentRunContext RunContext { get; }
}
