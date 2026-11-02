using AgentForge.Models.Chat;
using AgentForge.Tools;

namespace AgentForge.Models.Contracts;

/// <summary>
/// Adds native tool-calling support to the baseline chat model contract.
/// </summary>
/// <remarks>
/// Implementations of this interface propose tool calls in their normalized response.
/// They must not execute application tools. Tool validation, authorization, and execution
/// remain responsibilities of the AgentForge runtime and application.
/// </remarks>
public interface IToolCallingChatModel : IChatModel
{
    /// <summary>
    /// Generates an assistant response with the supplied tool definitions available for selection.
    /// </summary>
    /// <param name="request">The normalized model request.</param>
    /// <param name="tools">The tools scoped and made available by the calling application.</param>
    /// <param name="cancellationToken">A token used to request cooperative cancellation.</param>
    /// <returns>A normalized assistant response that may contain tool-call proposals.</returns>
    Task<ModelResponse> GenerateWithToolsAsync(
        ModelRequest request,
        IReadOnlyList<ToolDefinition> tools,
        CancellationToken cancellationToken = default);
}
