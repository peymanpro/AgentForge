namespace AgentForge.Agents.Reflection.Evaluation;

/// <summary>
/// Contains the task and candidate that an evaluator must assess.
/// </summary>
public sealed class ReflectionEvaluationRequest
{
    /// <summary>Creates an evaluation request.</summary>
    public ReflectionEvaluationRequest(string task, string candidate, int attemptNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(task);
        ArgumentException.ThrowIfNullOrWhiteSpace(candidate);

        if (attemptNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(attemptNumber));
        }

        Task = task;
        Candidate = candidate;
        AttemptNumber = attemptNumber;
    }

    /// <summary>Gets the original application task.</summary>
    public string Task { get; }

    /// <summary>Gets the candidate to evaluate.</summary>
    public string Candidate { get; }

    /// <summary>Gets the one-based attempt number.</summary>
    public int AttemptNumber { get; }
}
