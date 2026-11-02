namespace AgentForge.Agents.Reflection;

/// <summary>
/// Contains the best candidate and the complete observable history of a Reflection run.
/// </summary>
public sealed class ReflectionAgentOutput
{
    internal ReflectionAgentOutput(
        string? bestCandidate,
        double? bestScore,
        bool isAccepted,
        IReadOnlyList<ReflectionAttempt> attempts)
    {
        BestCandidate = bestCandidate;
        BestScore = bestScore;
        IsAccepted = isAccepted;
        Attempts = Array.AsReadOnly(attempts.ToArray());
    }

    /// <summary>Gets the best known candidate, when generation produced one.</summary>
    public string? BestCandidate { get; }

    /// <summary>Gets the score of the best known candidate, when supplied by the evaluator.</summary>
    public double? BestScore { get; }

    /// <summary>Gets whether the best candidate satisfied the evaluator's acceptance criteria.</summary>
    public bool IsAccepted { get; }

    /// <summary>Gets the immutable, ordered candidate and evaluation history.</summary>
    public IReadOnlyList<ReflectionAttempt> Attempts { get; }

    /// <summary>Gets the number of candidates generated during the run.</summary>
    public int AttemptCount => Attempts.Count;
}
