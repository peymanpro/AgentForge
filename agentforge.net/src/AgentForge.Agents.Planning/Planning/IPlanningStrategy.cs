namespace AgentForge.Agents.Planning.Planning;

/// <summary>
/// Generates a structured plan candidate without executing any plan steps.
/// </summary>
public interface IPlanningStrategy
{
    /// <summary>
    /// Generates a plan candidate for the supplied task and constraints.
    /// </summary>
    /// <param name="request">The task and allowed planning constraints.</param>
    /// <param name="cancellationToken">A token used to request cooperative cancellation.</param>
    /// <returns>A structured plan candidate.</returns>
    Task<ExecutionPlan> GenerateAsync(
        PlanningRequest request,
        CancellationToken cancellationToken = default);
}
