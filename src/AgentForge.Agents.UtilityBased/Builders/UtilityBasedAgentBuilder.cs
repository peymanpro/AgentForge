using AgentForge.Abstractions.Execution;
using AgentForge.Agents.UtilityBased.Application.UseCases;
using AgentForge.Agents.UtilityBased.Domain.Policies;

namespace AgentForge.Agents.UtilityBased.Builders;

/// <summary>Creates a configured Utility-Based agent.</summary>
public static class UtilityBasedAgentBuilder
{
    /// <summary>Starts a Utility-Based agent definition.</summary>
    public static UtilityBasedAgentBuilder<TPercept, TAction> Create<TPercept, TAction>()
        where TPercept : notnull
        where TAction : notnull
    {
        return new UtilityBasedAgentBuilder<TPercept, TAction>();
    }
}

/// <summary>Fluent configuration for candidate generation and utility evaluation.</summary>
public sealed class UtilityBasedAgentBuilder<TPercept, TAction>
    where TPercept : notnull
    where TAction : notnull
{
    private Func<TPercept, IEnumerable<TAction>>? _candidateProvider;
    private Func<TPercept, TAction, double>? _utilityEvaluator;
    private UtilityTieBreakPolicy _tieBreakPolicy = UtilityTieBreakPolicy.FirstCandidate;
    private TimeProvider? _timeProvider;

    /// <summary>Configures the candidate actions available for each percept.</summary>
    public UtilityBasedAgentBuilder<TPercept, TAction> WithCandidates(
        Func<TPercept, IEnumerable<TAction>> candidateProvider)
    {
        ArgumentNullException.ThrowIfNull(candidateProvider);
        _candidateProvider = candidateProvider;
        return this;
    }

    /// <summary>Configures the utility score assigned to each candidate action.</summary>
    public UtilityBasedAgentBuilder<TPercept, TAction> WithUtility(
        Func<TPercept, TAction, double> utilityEvaluator)
    {
        ArgumentNullException.ThrowIfNull(utilityEvaluator);
        _utilityEvaluator = utilityEvaluator;
        return this;
    }

    /// <summary>Configures how equal maximum utility scores are resolved.</summary>
    public UtilityBasedAgentBuilder<TPercept, TAction> WithTieBreakPolicy(
        UtilityTieBreakPolicy tieBreakPolicy)
    {
        if (!Enum.IsDefined(tieBreakPolicy))
        {
            throw new ArgumentOutOfRangeException(nameof(tieBreakPolicy), tieBreakPolicy, "The tie-break policy is not defined.");
        }

        _tieBreakPolicy = tieBreakPolicy;
        return this;
    }

    /// <summary>Configures the clock used for run timestamps, primarily for deterministic tests.</summary>
    public UtilityBasedAgentBuilder<TPercept, TAction> WithTimeProvider(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        _timeProvider = timeProvider;
        return this;
    }

    /// <summary>Builds the immutable agent handle.</summary>
    public AgentHandle<TPercept, UtilityBasedOutput<TAction>> Build()
    {
        if (_candidateProvider is null)
        {
            throw new InvalidOperationException("A candidate provider must be configured before building the agent.");
        }

        if (_utilityEvaluator is null)
        {
            throw new InvalidOperationException("A utility evaluator must be configured before building the agent.");
        }

        var agent = new UtilityBasedAgent<TPercept, TAction>(
            _candidateProvider,
            _utilityEvaluator,
            _tieBreakPolicy,
            _timeProvider);

        return new AgentHandle<TPercept, UtilityBasedOutput<TAction>>(agent);
    }
}