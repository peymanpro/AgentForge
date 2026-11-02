using System.Text.Json;
using AgentForge.Abstractions.Execution;

namespace AgentForge.Tools;

/// <summary>
/// Represents the normalized outcome of one tool invocation attempt.
/// </summary>
public sealed class ToolExecutionResult
{
    private ToolExecutionResult(
        string toolCallId,
        ToolExecutionStatus status,
        JsonElement? output,
        AgentFailure? failure)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(toolCallId);

        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status), status, "The tool execution status is not defined.");
        }

        if (status == ToolExecutionStatus.Succeeded && failure is not null)
        {
            throw new ArgumentException("A successful tool result cannot contain a failure.", nameof(failure));
        }

        if (status != ToolExecutionStatus.Succeeded && failure is null)
        {
            throw new ArgumentNullException(nameof(failure), "A failed tool result must explain its outcome.");
        }

        failure?.Validate();

        ToolCallId = toolCallId;
        Status = status;
        Output = output?.Clone();
        Failure = failure;
    }

    /// <summary>
    /// Gets the invocation correlation identifier.
    /// </summary>
    public string ToolCallId { get; }

    /// <summary>
    /// Gets the normalized invocation status.
    /// </summary>
    public ToolExecutionStatus Status { get; }

    /// <summary>
    /// Gets the JSON output, when the invocation succeeded.
    /// </summary>
    public JsonElement? Output { get; }

    /// <summary>
    /// Gets the normalized failure or policy outcome, when the invocation did not succeed.
    /// </summary>
    public AgentFailure? Failure { get; }

    /// <summary>
    /// Creates a successful tool result.
    /// </summary>
    public static ToolExecutionResult Succeeded(string toolCallId, JsonElement output)
    {
        if (output.ValueKind == JsonValueKind.Undefined)
        {
            throw new ArgumentException("A tool output must be a defined JSON value.", nameof(output));
        }

        return new ToolExecutionResult(toolCallId, ToolExecutionStatus.Succeeded, output, null);
    }

    /// <summary>
    /// Creates a non-successful tool result.
    /// </summary>
    /// <param name="toolCallId">The invocation correlation identifier.</param>
    /// <param name="status">A status other than Succeeded.</param>
    /// <param name="failure">The normalized reason the invocation did not succeed.</param>
    /// <returns>A normalized non-successful result.</returns>
    public static ToolExecutionResult Unsuccessful(
        string toolCallId,
        ToolExecutionStatus status,
        AgentFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);

        if (status == ToolExecutionStatus.Succeeded)
        {
            throw new ArgumentException("Use Succeeded to create a successful tool result.", nameof(status));
        }

        return new ToolExecutionResult(toolCallId, status, null, failure);
    }
}
