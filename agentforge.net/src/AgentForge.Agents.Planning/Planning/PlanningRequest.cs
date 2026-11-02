namespace AgentForge.Agents.Planning.Planning;

/// <summary>
/// Contains the task and constraints supplied to a planning strategy.
/// </summary>
public sealed class PlanningRequest
{
    /// <summary>
    /// Creates a planning request.
    /// </summary>
    /// <param name="task">The application task to decompose.</param>
    /// <param name="maxPlanSteps">The maximum permitted number of generated steps.</param>
    /// <param name="allowedStepTypes">The handler types available to the planner.</param>
    public PlanningRequest(string task, int maxPlanSteps, IEnumerable<string> allowedStepTypes)
    {
        ArgumentNullException.ThrowIfNull(task);
        ArgumentNullException.ThrowIfNull(allowedStepTypes);

        if (maxPlanSteps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPlanSteps));
        }

        Task = task;
        MaxPlanSteps = maxPlanSteps;
        AllowedStepTypes = Array.AsReadOnly(allowedStepTypes.Distinct(StringComparer.Ordinal).ToArray());
    }

    /// <summary>Gets the original application task.</summary>
    public string Task { get; }

    /// <summary>Gets the maximum number of permitted plan steps.</summary>
    public int MaxPlanSteps { get; }

    /// <summary>Gets the exact step handler types available to the planner.</summary>
    public IReadOnlyList<string> AllowedStepTypes { get; }
}
