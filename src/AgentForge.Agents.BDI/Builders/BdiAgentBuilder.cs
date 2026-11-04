using AgentForge.Abstractions.Execution;
using AgentForge.Agents.BDI.Models;

namespace AgentForge.Agents.BDI.Builders;

/// <summary>Creates a fluent builder for a Belief-Desire-Intention agent.</summary>
public static class BdiAgentBuilder
{
    /// <summary>Starts a typed BDI agent definition.</summary>
    public static BdiAgentBuilder<TBelief, TDesire, TIntention, TObservation, TAction>
        Create<TBelief, TDesire, TIntention, TObservation, TAction>()
        where TBelief : notnull
        where TDesire : notnull
        where TIntention : notnull
        where TObservation : notnull
        where TAction : notnull => new();
}

/// <summary>Configures belief revision, desire filtering, intention commitment, and action selection.</summary>
public sealed class BdiAgentBuilder<TBelief, TDesire, TIntention, TObservation, TAction>
    where TBelief : notnull
    where TDesire : notnull
    where TIntention : notnull
    where TObservation : notnull
    where TAction : notnull
{
    private Func<IReadOnlyList<TBelief>, TObservation, IReadOnlyList<TBelief>>? _beliefRevision;
    private Func<IReadOnlyList<TBelief>, IReadOnlyList<TDesire>, IReadOnlyList<TDesire>>? _desireSelection;
    private Func<TDesire, TIntention>? _intentionFactory;
    private Func<IReadOnlyList<TBelief>, TIntention, bool>? _commitmentPolicy;
    private Func<IReadOnlyList<TBelief>, IReadOnlyList<TIntention>, BdiAction<TAction>>? _actionSelection;
    private int _maxIntentions = 16;
    private TimeProvider _timeProvider = TimeProvider.System;

    /// <summary>Configures how the current observation updates the explicit belief set.</summary>
    public BdiAgentBuilder<TBelief, TDesire, TIntention, TObservation, TAction> WithBeliefRevision(
        Func<IReadOnlyList<TBelief>, TObservation, IReadOnlyList<TBelief>> revision)
    {
        ArgumentNullException.ThrowIfNull(revision);
        _beliefRevision = revision;
        return this;
    }

    /// <summary>Configures the policy that filters currently available desires against revised beliefs.</summary>
    public BdiAgentBuilder<TBelief, TDesire, TIntention, TObservation, TAction> WithDesireSelection(
        Func<IReadOnlyList<TBelief>, IReadOnlyList<TDesire>, IReadOnlyList<TDesire>> selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        _desireSelection = selection;
        return this;
    }

    /// <summary>Maps an eligible desire to an intention representation.</summary>
    public BdiAgentBuilder<TBelief, TDesire, TIntention, TObservation, TAction> WithIntentionFactory(
        Func<TDesire, TIntention> factory)
    {
        ArgumentNullException.ThrowIfNull(factory);
        _intentionFactory = factory;
        return this;
    }

    /// <summary>Decides whether an existing intention remains committed under the revised beliefs.</summary>
    public BdiAgentBuilder<TBelief, TDesire, TIntention, TObservation, TAction> WithCommitmentPolicy(
        Func<IReadOnlyList<TBelief>, TIntention, bool> policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        _commitmentPolicy = policy;
        return this;
    }

    /// <summary>Configures the action proposed from revised beliefs and the resulting intention set.</summary>
    public BdiAgentBuilder<TBelief, TDesire, TIntention, TObservation, TAction> WithActionSelection(
        Func<IReadOnlyList<TBelief>, IReadOnlyList<TIntention>, BdiAction<TAction>> selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        _actionSelection = selection;
        return this;
    }

    /// <summary>Sets the maximum number of retained and newly selected intentions.</summary>
    public BdiAgentBuilder<TBelief, TDesire, TIntention, TObservation, TAction> WithMaxIntentions(int value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
        _maxIntentions = value;
        return this;
    }

    /// <summary>Configures the clock used for normalized run timestamps.</summary>
    public BdiAgentBuilder<TBelief, TDesire, TIntention, TObservation, TAction> WithTimeProvider(
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        _timeProvider = timeProvider;
        return this;
    }

    /// <summary>Builds the immutable BDI agent execution handle.</summary>
    public AgentHandle<
        BdiAgentInput<TBelief, TDesire, TIntention, TObservation>,
        BdiAgentOutput<TBelief, TDesire, TIntention, TAction>> Build()
    {
        if (_beliefRevision is null)
            throw new InvalidOperationException("A belief revision strategy must be configured.");
        if (_desireSelection is null)
            throw new InvalidOperationException("A desire selection strategy must be configured.");
        if (_intentionFactory is null)
            throw new InvalidOperationException("An intention factory must be configured.");
        if (_commitmentPolicy is null)
            throw new InvalidOperationException("An intention commitment policy must be configured.");
        if (_actionSelection is null)
            throw new InvalidOperationException("An action selection strategy must be configured.");

        return new AgentHandle<
            BdiAgentInput<TBelief, TDesire, TIntention, TObservation>,
            BdiAgentOutput<TBelief, TDesire, TIntention, TAction>>(
                new BdiAgent<TBelief, TDesire, TIntention, TObservation, TAction>(
                    _beliefRevision,
                    _desireSelection,
                    _intentionFactory,
                    _commitmentPolicy,
                    _actionSelection,
                    _maxIntentions,
                    _timeProvider));
    }
}