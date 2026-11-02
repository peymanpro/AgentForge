namespace AgentForge.Agents.Reflection.Generation;

/// <summary>
/// Produces an initial candidate or revises a candidate using explicit evaluator feedback.
/// </summary>
public interface IReflectionGenerationStrategy
{
    /// <summary>
    /// Generates a candidate for the supplied task and refinement context.
    /// </summary>
    Task<string> GenerateAsync(
        ReflectionGenerationRequest request,
        CancellationToken cancellationToken = default);
}
