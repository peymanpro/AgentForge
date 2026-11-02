namespace AgentForge.Agents.Reflection.Generation;

/// <summary>
/// Contains the task, previous candidate, and evaluator feedback supplied to a generation strategy.
/// </summary>
public sealed class ReflectionGenerationRequest
{
    /// <summary>
    /// Creates a generation request.
    /// </summary>
    public ReflectionGenerationRequest(
        string task,
        int attemptNumber,
        int maxAttempts,
        string? previousCandidate,
        string? feedback)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(task);

        if (attemptNumber <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(attemptNumber));
        }

        if (maxAttempts <= 0 || attemptNumber > maxAttempts)
        {
            throw new ArgumentOutOfRangeException(nameof(maxAttempts));
        }

        Task = task;
        AttemptNumber = attemptNumber;
        MaxAttempts = maxAttempts;
        PreviousCandidate = previousCandidate;
        Feedback = feedback;
    }

    /// <summary>Gets the original application task.</summary>
    public string Task { get; }

    /// <summary>Gets the current one-based refinement attempt.</summary>
    public int AttemptNumber { get; }

    /// <summary>Gets the maximum permitted number of refinement attempts.</summary>
    public int MaxAttempts { get; }

    /// <summary>Gets the candidate from the prior attempt, when one exists.</summary>
    public string? PreviousCandidate { get; }

    /// <summary>Gets the evaluator feedback from the prior attempt, when one exists.</summary>
    public string? Feedback { get; }
}
