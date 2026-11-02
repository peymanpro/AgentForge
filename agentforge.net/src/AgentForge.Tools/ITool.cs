using System.Text.Json;
using AgentForge.Abstractions.Execution;

namespace AgentForge.Tools;

/// <summary>
/// Describes an application operation that an agent may request.
/// </summary>
public interface ITool
{
    /// <summary>
    /// Gets the stable name, description, and input schema exposed for this tool.
    /// </summary>
    ToolDefinition Definition { get; }

    /// <summary>
    /// Validates arguments without performing the tool's application operation or side effects.
    /// </summary>
    /// <param name="arguments">The untrusted JSON arguments proposed by the model.</param>
    /// <returns>A validation result describing whether the arguments are acceptable.</returns>
    ToolValidationResult Validate(JsonElement arguments);

    /// <summary>
    /// Executes the tool after the executor has validated the arguments and obtained authorization.
    /// </summary>
    /// <param name="arguments">The validated JSON arguments.</param>
    /// <param name="runContext">The current agent run context.</param>
    /// <param name="cancellationToken">A token used to request cooperative cancellation.</param>
    /// <returns>A JSON value produced by the tool.</returns>
    Task<JsonElement> ExecuteAsync(
        JsonElement arguments,
        AgentRunContext runContext,
        CancellationToken cancellationToken = default);
}
