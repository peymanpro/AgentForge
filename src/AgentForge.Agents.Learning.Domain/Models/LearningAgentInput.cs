namespace AgentForge.Agents.Learning.Domain.Models;
/// <summary>Candidate actions and current learned state for one decision.</summary>
/// <typeparam name="TAction">The action identity type.</typeparam>
public sealed class LearningAgentInput<TAction> where TAction : notnull
{
    /// <summary>Creates a decision input.</summary>
    /// <param name="candidates">Non-empty candidate action sequence.</param>
    /// <param name="state">Current learned state, or a new empty state.</param>
    public LearningAgentInput(IEnumerable<TAction> candidates, LearningState<TAction>? state = null)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        var copy = candidates.ToArray();
        if (copy.Length == 0) throw new ArgumentException("At least one candidate is required.", nameof(candidates));
        if (copy.Any(action => action is null)) throw new ArgumentException("Candidates cannot contain null.", nameof(candidates));
        Candidates = Array.AsReadOnly(copy);
        State = state ?? new LearningState<TAction>();
    }
    /// <summary>Gets the ordered candidate actions.</summary>
    public IReadOnlyList<TAction> Candidates { get; }
    /// <summary>Gets the immutable learned state used for this decision.</summary>
    public LearningState<TAction> State { get; }
}