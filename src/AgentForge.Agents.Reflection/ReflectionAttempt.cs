using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Reflection.Evaluation;

namespace AgentForge.Agents.Reflection;

/// <summary>
/// Records one candidate and its evaluation outcome, if evaluation completed.
/// </summary>
/// <param name="AttemptNumber">The one-based generation/evaluation attempt number.</param>
/// <param name="Candidate">The generated candidate content.</param>
/// <param name="Evaluation">The evaluation outcome, when evaluation completed.</param>
/// <param name="Failure">The normalized evaluation failure, when applicable.</param>
public sealed record ReflectionAttempt(
    int AttemptNumber,
    string Candidate,
    ReflectionEvaluation? Evaluation,
    AgentFailure? Failure);
