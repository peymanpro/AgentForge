using System.Text.Json;

namespace AgentForge.Agents.Planning.Execution;

/// <summary>
/// Executes a single declared plan step of one registered type.
/// </summary>
public interface IPlanStepHandler
{
    /// <summary>Gets the stable step type handled by this implementation.</summary>
    string StepType { get; }

    /// <summary>
    /// Executes the supplied step using only its explicitly mapped prior outputs.
    /// </summary>
    /// <param name="context">The step execution context.</param>
    /// <param name="cancellationToken">A token used to request cooperative cancellation.</param>
    /// <returns>The step output as a JSON value.</returns>
    Task<JsonElement> ExecuteAsync(
        PlanStepExecutionContext context,
        CancellationToken cancellationToken = default);
}
