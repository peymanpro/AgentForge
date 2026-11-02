namespace AgentForge.Agents.Learning.Domain.Models;
/// <summary>Candidate actions and current learned state for one decision.</summary>
public sealed class LearningAgentInput<TAction> where TAction : notnull
{
    public LearningAgentInput(IEnumerable<TAction> candidates, LearningState<TAction>? state = null)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        var copy = candidates.ToArray();
        if (copy.Length == 0) throw new ArgumentException("At least one candidate is required.", nameof(candidates));
        if (copy.Any(action => action is null)) throw new ArgumentException("Candidates cannot contain null.", nameof(candidates));
        Candidates = Array.AsReadOnly(copy);
        State = state ?? new LearningState<TAction>();
    }
    public IReadOnlyList<TAction> Candidates { get; }
    public LearningState<TAction> State { get; }
}