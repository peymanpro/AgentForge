using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Reflection.Evaluation;

namespace AgentForge.Agents.Reflection;

/// <summary>
/// Records one candidate and its evaluation outcome, if evaluation completed.
/// </summary>
public sealed record ReflectionAttempt(
    int AttemptNumber,
    string Candidate,
    ReflectionEvaluation? Evaluation,
    AgentFailure? Failure);
