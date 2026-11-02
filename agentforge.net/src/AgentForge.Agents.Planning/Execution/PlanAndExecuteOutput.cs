using AgentForge.Agents.Planning.Planning;

namespace AgentForge.Agents.Planning.Execution;

/// <summary>
/// Contains the validated plan and its ordered step outcomes.
/// </summary>
public sealed class PlanAndExecuteOutput
{
    internal PlanAndExecuteOutput(ExecutionPlan? plan, IReadOnlyList<PlanStepResult> steps)
    {
        Plan = plan;
        Steps = Array.AsReadOnly(steps.ToArray());
    }

    /// <summary>Gets the validated plan, or null when planning did not produce one.</summary>
    public ExecutionPlan? Plan { get; }

    /// <summary>Gets the ordered outcomes for steps that were attempted or not started.</summary>
    public IReadOnlyList<PlanStepResult> Steps { get; }

    /// <summary>Gets the number of successfully completed steps.</summary>
    public int CompletedStepCount => Steps.Count(static step => step.Status == PlanStepStatus.Completed);
}
