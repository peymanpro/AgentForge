namespace AgentForge.Agents.Learning.Domain.Models;

/// <summary>Candidate actions, context features, and learned state for one contextual decision.</summary>
/// <typeparam name="TAction">The action identity type.</typeparam>
public sealed class ContextualLearningAgentInput<TAction> where TAction : notnull
{
    /// <summary>Creates a contextual decision input.</summary>
    public ContextualLearningAgentInput(IEnumerable<TAction> candidates, IEnumerable<double> context,
        ContextualLearningState<TAction> state)
    {
        ArgumentNullException.ThrowIfNull(candidates);
        ArgumentNullException.ThrowIfNull(context);
        State = state ?? throw new ArgumentNullException(nameof(state));
        var actions = candidates.ToArray();
        if (actions.Length == 0) throw new ArgumentException("At least one candidate is required.", nameof(candidates));
        if (actions.Any(action => action is null)) throw new ArgumentException("Candidates cannot contain null.", nameof(candidates));
        if (actions.Distinct().Count() != actions.Length)
            throw new ArgumentException("Candidate actions must be unique.", nameof(candidates));
        var features = context.ToArray();
        if (features.Length != State.FeatureCount || features.Any(value => !double.IsFinite(value)))
            throw new ArgumentException("Context must contain exactly the configured number of finite features.", nameof(context));
        Candidates = Array.AsReadOnly(actions);
        Context = Array.AsReadOnly(features);
    }

    /// <summary>Gets the ordered candidate actions.</summary>
    public IReadOnlyList<TAction> Candidates { get; }

    /// <summary>Gets the immutable context feature vector.</summary>
    public IReadOnlyList<double> Context { get; }

    /// <summary>Gets the immutable state used for this decision.</summary>
    public ContextualLearningState<TAction> State { get; }
}
