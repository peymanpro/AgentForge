namespace AgentForge.Agents.Planning.Planning;

/// <summary>
/// Represents a structured sequential plan candidate.
/// </summary>
public sealed class ExecutionPlan
{
    /// <summary>
    /// Creates a plan candidate. The plan validator checks semantic constraints before execution.
    /// </summary>
    /// <param name="objective">The overall plan objective.</param>
    /// <param name="steps">The ordered steps in the plan.</param>
    public ExecutionPlan(string objective, IEnumerable<PlanStep> steps)
    {
        ArgumentNullException.ThrowIfNull(objective);
        ArgumentNullException.ThrowIfNull(steps);

        var materialized = steps.ToArray();
        if (materialized.Any(static step => step is null))
        {
            throw new ArgumentException("A plan cannot contain null steps.", nameof(steps));
        }

        Objective = objective;
        Steps = Array.AsReadOnly(materialized);
    }

    /// <summary>Gets the overall objective represented by this plan.</summary>
    public string Objective { get; }

    /// <summary>Gets the ordered steps in the plan.</summary>
    public IReadOnlyList<PlanStep> Steps { get; }
}
