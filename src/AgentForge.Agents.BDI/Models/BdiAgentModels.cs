namespace AgentForge.Agents.BDI.Models;

/// <summary>Supplies the BDI state and new observation for one deliberation cycle.</summary>
public sealed record BdiAgentInput<TBelief, TDesire, TIntention, TObservation>(
    IReadOnlyList<TBelief> Beliefs,
    IReadOnlyList<TDesire> AvailableDesires,
    IReadOnlyList<TIntention> CurrentIntentions,
    TObservation Observation)
    where TBelief : notnull
    where TDesire : notnull
    where TIntention : notnull
    where TObservation : notnull;

/// <summary>Represents an optional action proposed by the BDI decision cycle.</summary>
public sealed record BdiAction<TAction>(bool HasAction, TAction? Action)
    where TAction : notnull
{
    /// <summary>Gets a no-action result.</summary>
    public static BdiAction<TAction> None { get; } = new(false, default);

    /// <summary>Creates an action proposal.</summary>
    public static BdiAction<TAction> From(TAction action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return new BdiAction<TAction>(true, action);
    }
}

/// <summary>Returns the revised beliefs, selected desires, committed intentions, and proposed action.</summary>
public sealed record BdiAgentOutput<TBelief, TDesire, TIntention, TAction>(
    IReadOnlyList<TBelief> Beliefs,
    IReadOnlyList<TDesire> EligibleDesires,
    IReadOnlyList<TIntention> Intentions,
    BdiAction<TAction> ProposedAction)
    where TBelief : notnull
    where TDesire : notnull
    where TIntention : notnull
    where TAction : notnull;