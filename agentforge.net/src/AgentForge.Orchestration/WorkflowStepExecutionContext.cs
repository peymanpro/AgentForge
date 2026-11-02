using System.Collections.ObjectModel;
using AgentForge.Abstractions.Execution;

namespace AgentForge.Orchestration;

/// <summary>
/// Provides a workflow input and only the prior outputs explicitly declared for the current step.
/// </summary>
public sealed class WorkflowStepExecutionContext<TWorkflowInput>
{
    internal WorkflowStepExecutionContext(
        TWorkflowInput workflowInput,
        AgentRunContext runContext,
        IReadOnlyDictionary<string, object?> referencedOutputs)
    {
        ArgumentNullException.ThrowIfNull(runContext);
        ArgumentNullException.ThrowIfNull(referencedOutputs);

        var copied = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var item in referencedOutputs)
        {
            copied.Add(item.Key, item.Value);
        }

        WorkflowInput = workflowInput;
        RunContext = runContext;
        ReferencedOutputs = new ReadOnlyDictionary<string, object?>(copied);
    }

    /// <summary>Gets the original workflow input.</summary>
    public TWorkflowInput WorkflowInput { get; }

    /// <summary>Gets the workflow run context.</summary>
    public AgentRunContext RunContext { get; }

    /// <summary>
    /// Gets the immutable set of outputs declared as inputs to this step.
    /// Outputs from earlier steps that were not explicitly mapped are not accessible.
    /// </summary>
    public IReadOnlyDictionary<string, object?> ReferencedOutputs { get; }

    /// <summary>
    /// Gets a prior step output using its declared step identifier.
    /// </summary>
    /// <typeparam name="TOutput">The expected output type.</typeparam>
    /// <param name="stepId">The identifier of a declared input reference.</param>
    /// <returns>The prior step output.</returns>
    /// <exception cref="KeyNotFoundException">The step output was not explicitly declared for this step.</exception>
    /// <exception cref="InvalidCastException">The output does not have the requested type.</exception>
    public TOutput GetOutput<TOutput>(string stepId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stepId);

        if (!ReferencedOutputs.TryGetValue(stepId, out var value))
        {
            throw new KeyNotFoundException(
                $"Step output '{stepId}' was not declared as an input to the current workflow step.");
        }

        if (value is TOutput typed)
        {
            return typed;
        }

        if (value is null && default(TOutput) is null)
        {
            return default!;
        }

        throw new InvalidCastException(
            $"Step output '{stepId}' is not compatible with the requested type '{typeof(TOutput).FullName}'.");
    }

    /// <summary>
    /// Attempts to obtain a prior step output using its declared step identifier.
    /// </summary>
    public bool TryGetOutput<TOutput>(string stepId, out TOutput? output)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stepId);

        if (ReferencedOutputs.TryGetValue(stepId, out var value) && value is TOutput typed)
        {
            output = typed;
            return true;
        }

        if (ReferencedOutputs.TryGetValue(stepId, out value) && value is null && default(TOutput) is null)
        {
            output = default;
            return true;
        }

        output = default;
        return false;
    }
}
