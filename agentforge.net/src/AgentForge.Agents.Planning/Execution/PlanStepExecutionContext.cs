using System.Collections.ObjectModel;
using System.Text.Json;
using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Planning.Planning;

namespace AgentForge.Agents.Planning.Execution;

/// <summary>
/// Contains only the task, current step, run identity, and outputs explicitly referenced by the step.
/// </summary>
public sealed class PlanStepExecutionContext
{
    /// <summary>
    /// Creates an immutable step execution context.
    /// </summary>
    public PlanStepExecutionContext(
        string task,
        PlanStep step,
        AgentRunContext runContext,
        IReadOnlyDictionary<string, JsonElement> referencedOutputs)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(step);
        ArgumentNullException.ThrowIfNull(runContext);
        ArgumentNullException.ThrowIfNull(referencedOutputs);

        var copied = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var item in referencedOutputs)
        {
            copied.Add(item.Key, item.Value.Clone());
        }

        Task = task;
        Step = step;
        RunContext = runContext;
        ReferencedOutputs = new ReadOnlyDictionary<string, JsonElement>(copied);
    }

    /// <summary>Gets the original application task.</summary>
    public string Task { get; }

    /// <summary>Gets the current validated step.</summary>
    public PlanStep Step { get; }

    /// <summary>Gets the parent agent run context.</summary>
    public AgentRunContext RunContext { get; }

    /// <summary>Gets only the prior step outputs explicitly referenced by the current step.</summary>
    public IReadOnlyDictionary<string, JsonElement> ReferencedOutputs { get; }
}
