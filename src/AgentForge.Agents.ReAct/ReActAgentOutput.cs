using AgentForge.Models.Chat;
using AgentForge.Tools;

namespace AgentForge.Agents.ReAct;

/// <summary>
/// Contains the final response and inspectable execution summary from one ReAct run.
/// </summary>
public sealed class ReActAgentOutput
{
    internal ReActAgentOutput(
        string? finalResponse,
        ModelFinishReason? finishReason,
        int modelTurnCount,
        int toolInvocationCount,
        IReadOnlyList<ToolExecutionResult> toolExecutions,
        IReadOnlyList<ModelToolCall> pendingToolCalls)
    {
        FinalResponse = finalResponse;
        FinishReason = finishReason;
        ModelTurnCount = modelTurnCount;
        ToolInvocationCount = toolInvocationCount;
        ToolExecutions = Array.AsReadOnly(toolExecutions.ToArray());
        PendingToolCalls = Array.AsReadOnly(pendingToolCalls.ToArray());
    }

    /// <summary>
    /// Gets the final assistant response, when the model produced one.
    /// </summary>
    public string? FinalResponse { get; }

    /// <summary>
    /// Gets the model finish reason for the final response or most recent decision.
    /// </summary>
    public ModelFinishReason? FinishReason { get; }

    /// <summary>
    /// Gets the number of model requests performed during the run.
    /// </summary>
    public int ModelTurnCount { get; }

    /// <summary>
    /// Gets the number of tool invocation attempts performed during the run.
    /// </summary>
    public int ToolInvocationCount { get; }

    /// <summary>
    /// Gets the immutable outcomes of tool invocation attempts.
    /// </summary>
    public IReadOnlyList<ToolExecutionResult> ToolExecutions { get; }

    /// <summary>
    /// Gets tool-call proposals that were not executed because a run limit was reached.
    /// </summary>
    public IReadOnlyList<ModelToolCall> PendingToolCalls { get; }
}
