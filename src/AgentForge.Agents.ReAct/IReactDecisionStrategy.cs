using AgentForge.Models.Chat;
using AgentForge.Tools;

namespace AgentForge.Agents.ReAct;

/// <summary>
/// Converts a model interaction into a normalized final response or tool-call proposal.
/// </summary>
public interface IReactDecisionStrategy
{
    /// <summary>
    /// Obtains the next decision for a ReAct execution step.
    /// </summary>
    /// <param name="request">The conversation and generation options for this turn.</param>
    /// <param name="availableTools">The tools explicitly available to this execution.</param>
    /// <param name="cancellationToken">A token used to request cooperative cancellation.</param>
    /// <returns>A normalized model response.</returns>
    Task<ModelResponse> DecideAsync(
        ModelRequest request,
        IReadOnlyList<ToolDefinition> availableTools,
        CancellationToken cancellationToken = default);
}
