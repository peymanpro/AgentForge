namespace AgentForge.Agents.Reflection.Evaluation;

/// <summary>
/// Assesses a candidate against application-defined acceptance criteria.
/// </summary>
public interface IReflectionEvaluator
{
    /// <summary>
    /// Evaluates one candidate and returns an explicit acceptance decision and feedback.
    /// </summary>
    Task<ReflectionEvaluation> EvaluateAsync(
        ReflectionEvaluationRequest request,
        CancellationToken cancellationToken = default);
}
