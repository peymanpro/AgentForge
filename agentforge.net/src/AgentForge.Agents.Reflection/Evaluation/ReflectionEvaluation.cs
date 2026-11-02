namespace AgentForge.Agents.Reflection.Evaluation;

/// <summary>
/// Represents an explicit evaluation decision and actionable feedback for one candidate.
/// </summary>
public sealed class ReflectionEvaluation
{
    /// <summary>
    /// Creates an evaluation.
    /// </summary>
    /// <param name="isAccepted">Whether the candidate satisfies the evaluator's acceptance criteria.</param>
    /// <param name="feedback">Actionable feedback for a future revision. Required when rejected.</param>
    /// <param name="score">An optional normalized quality score from zero to one.</param>
    public ReflectionEvaluation(bool isAccepted, string feedback, double? score = null)
    {
        ArgumentNullException.ThrowIfNull(feedback);

        if (!isAccepted && string.IsNullOrWhiteSpace(feedback))
        {
            throw new ArgumentException("A rejected candidate requires actionable feedback.", nameof(feedback));
        }

        if (score is { } value && (!double.IsFinite(value) || value < 0 || value > 1))
        {
            throw new ArgumentOutOfRangeException(nameof(score), score, "An evaluation score must be between zero and one.");
        }

        IsAccepted = isAccepted;
        Feedback = feedback;
        Score = score;
    }

    /// <summary>Gets whether the candidate satisfied the configured acceptance criteria.</summary>
    public bool IsAccepted { get; }

    /// <summary>Gets actionable feedback for the next refinement attempt.</summary>
    public string Feedback { get; }

    /// <summary>Gets the optional normalized quality score.</summary>
    public double? Score { get; }
}
