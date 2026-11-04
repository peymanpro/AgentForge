using AgentForge.Abstractions.Execution;
using AgentForge.Agents.FiniteState.Models;

namespace AgentForge.Agents.FiniteState.Builders;

/// <summary>Creates a fluent builder for an event-driven finite-state agent.</summary>
public static class FiniteStateAgentBuilder
{
    /// <summary>Starts a finite-state agent definition.</summary>
    public static FiniteStateAgentBuilder<TState, TEvent, TAction> Create<TState, TEvent, TAction>()
        where TState : notnull
        where TEvent : notnull
        where TAction : notnull => new();
}

/// <summary>Configures the ordered guards and transitions of a finite-state agent.</summary>
public sealed class FiniteStateAgentBuilder<TState, TEvent, TAction>
    where TState : notnull
    where TEvent : notnull
    where TAction : notnull
{
    private readonly List<StateTransition<TState, TEvent, TAction>> _transitions = [];
    private TimeProvider _timeProvider = TimeProvider.System;

    /// <summary>Adds an ordered guarded transition from the current state and event.</summary>
    public FiniteStateAgentBuilder<TState, TEvent, TAction> When(
        string name,
        Func<TState, TEvent, bool> guard,
        Func<TState, TEvent, TState> nextState,
        Func<TState, TEvent, TAction> actionFactory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(guard);
        ArgumentNullException.ThrowIfNull(nextState);
        ArgumentNullException.ThrowIfNull(actionFactory);
        if (_transitions.Any(transition => string.Equals(transition.Name, name, StringComparison.Ordinal)))
            throw new ArgumentException($"A transition named '{name}' is already registered.", nameof(name));

        _transitions.Add(new StateTransition<TState, TEvent, TAction>(
            name, guard, nextState, actionFactory));
        return this;
    }

    /// <summary>Configures the clock used for normalized run timestamps.</summary>
    public FiniteStateAgentBuilder<TState, TEvent, TAction> WithTimeProvider(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        _timeProvider = timeProvider;
        return this;
    }

    /// <summary>Builds the immutable finite-state execution handle.</summary>
    public AgentHandle<FiniteStateAgentInput<TState, TEvent>, FiniteStateAgentOutput<TState, TAction>> Build()
    {
        if (_transitions.Count == 0)
            throw new InvalidOperationException("At least one state transition must be registered before building the agent.");

        return new AgentHandle<FiniteStateAgentInput<TState, TEvent>, FiniteStateAgentOutput<TState, TAction>>(
            new FiniteStateAgent<TState, TEvent, TAction>(_transitions.ToArray(), _timeProvider));
    }
}