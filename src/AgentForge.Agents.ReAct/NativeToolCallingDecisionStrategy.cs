using AgentForge.Models.Chat;
using AgentForge.Models.Contracts;
using AgentForge.Tools;

namespace AgentForge.Agents.ReAct;

/// <summary>
/// Implements ReAct decisions through a model provider's native tool-calling capability.
/// </summary>
public sealed class NativeToolCallingDecisionStrategy : IReactDecisionStrategy
{
    private readonly IToolCallingChatModel _model;

    /// <summary>
    /// Creates a native tool-calling decision strategy.
    /// </summary>
    /// <param name="model">The provider adapter exposing native tool calling.</param>
    public NativeToolCallingDecisionStrategy(IToolCallingChatModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        _model = model;
    }

    /// <inheritdoc />
    public Task<ModelResponse> DecideAsync(
        ModelRequest request,
        IReadOnlyList<ToolDefinition> availableTools,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(availableTools);

        return _model.GenerateWithToolsAsync(request, availableTools, cancellationToken);
    }
}
