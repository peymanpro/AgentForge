using AgentForge.Abstractions.Execution;
using AgentForge.Agents.GoalBased.Application.UseCases;
using AgentForge.Agents.GoalBased.Domain.Models;

namespace AgentForge.Agents.GoalBased.Builders;

/// <summary>Creates a configured Goal-Based agent.</summary>
public static class GoalBasedAgentBuilder
{
    public static GoalBasedAgentBuilder<TState, TGoal, TAction> Create<TState, TGoal, TAction>()
        where TState : notnull
        where TGoal : notnull
        where TAction : notnull => new();
}

/// <summary>Fluent configuration for bounded goal-directed state-space search.</summary>
public sealed class GoalBasedAgentBuilder<TState, TGoal, TAction>
    where TState : notnull
    where TGoal : notnull
    where TAction : notnull
{
    private Func<TState, IEnumerable<GoalBasedTransition<TState, TAction>>>? _successors;
    private Func<TState, TGoal, bool>? _isGoal;
    private int _maxDepth = 32;
    private int _maxExploredStates = 10_000;
    private IEqualityComparer<TState>? _stateComparer;
    private TimeProvider? _timeProvider;

    public GoalBasedAgentBuilder<TState, TGoal, TAction> WithSuccessors(
        Func<TState, IEnumerable<GoalBasedTransition<TState, TAction>>> successorProvider)
    {
        ArgumentNullException.ThrowIfNull(successorProvider);
        _successors = successorProvider;
        return this;
    }

    public GoalBasedAgentBuilder<TState, TGoal, TAction> WithGoalPredicate(
        Func<TState, TGoal, bool> goalPredicate)
    {
        ArgumentNullException.ThrowIfNull(goalPredicate);
        _isGoal = goalPredicate;
        return this;
    }

    public GoalBasedAgentBuilder<TState, TGoal, TAction> WithLimits(
        int maxDepth, int maxExploredStates)
    {
        _ = new GoalBasedAgentOptions(maxDepth, maxExploredStates);
        _maxDepth = maxDepth;
        _maxExploredStates = maxExploredStates;
        return this;
    }

    public GoalBasedAgentBuilder<TState, TGoal, TAction> WithStateComparer(
        IEqualityComparer<TState> stateComparer)
    {
        ArgumentNullException.ThrowIfNull(stateComparer);
        _stateComparer = stateComparer;
        return this;
    }

    public GoalBasedAgentBuilder<TState, TGoal, TAction> WithTimeProvider(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        _timeProvider = timeProvider;
        return this;
    }

    public AgentHandle<GoalBasedInput<TState, TGoal>, GoalBasedOutput<TState, TAction>> Build()
    {
        if (_successors is null)
            throw new InvalidOperationException("A successor provider must be configured before building the agent.");
        if (_isGoal is null)
            throw new InvalidOperationException("A goal predicate must be configured before building the agent.");

        var agent = new GoalBasedAgent<TState, TGoal, TAction>(
            _successors, _isGoal,
            new GoalBasedAgentOptions(_maxDepth, _maxExploredStates),
            _stateComparer, _timeProvider);
        return new AgentHandle<GoalBasedInput<TState, TGoal>, GoalBasedOutput<TState, TAction>>(agent);
    }
}
