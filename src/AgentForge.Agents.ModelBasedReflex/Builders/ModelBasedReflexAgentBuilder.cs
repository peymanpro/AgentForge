using AgentForge.Abstractions.Execution;
using AgentForge.Agents.ModelBasedReflex.Application.UseCases;
using AgentForge.Agents.ModelBasedReflex.Domain.Decisions;
using AgentForge.Agents.ModelBasedReflex.Domain.Rules;

namespace AgentForge.Agents.ModelBasedReflex.Builders;

/// <summary>
/// Creates a configured Model-Based Reflex agent.
/// </summary>
public static class ModelBasedReflexAgentBuilder
{
    /// <summary>
    /// Starts a Model-Based Reflex agent definition.
    /// </summary>
    public static ModelBasedReflexAgentBuilder<TPercept, TState, TAction> Create<TPercept, TState, TAction>()
        where TPercept : notnull
        where TState : notnull
        where TAction : notnull
    {
        return new ModelBasedReflexAgentBuilder<TPercept, TState, TAction>();
    }
}

/// <summary>
/// Fluent configuration for a Model-Based Reflex agent.
/// </summary>
public sealed class ModelBasedReflexAgentBuilder<TPercept, TState, TAction>
    where TPercept : notnull
    where TState : notnull
    where TAction : notnull
{
    private readonly List<ModelBasedReflexRule<TState, TAction>> _rules = [];
    private Func<TState, TPercept, TState>? _stateUpdater;
    private TimeProvider? _timeProvider;

    /// <summary>
    /// Configures how the previous internal state is updated using the latest percept.
    /// </summary>
    public ModelBasedReflexAgentBuilder<TPercept, TState, TAction> WithStateUpdater(
        Func<TState, TPercept, TState> stateUpdater)
    {
        ArgumentNullException.ThrowIfNull(stateUpdater);
        _stateUpdater = stateUpdater;
        return this;
    }

    /// <summary>
    /// Adds an ordered rule evaluated against the updated internal state. The first match wins.
    /// </summary>
    public ModelBasedReflexAgentBuilder<TPercept, TState, TAction> When(
        string name,
        Func<TState, bool> condition,
        Func<TState, TAction> actionFactory)
    {
        _rules.Add(new ModelBasedReflexRule<TState, TAction>(name, condition, actionFactory));
        return this;
    }

    /// <summary>
    /// Adds a rule that proposes a constant action when its condition matches the updated state.
    /// </summary>
    public ModelBasedReflexAgentBuilder<TPercept, TState, TAction> When(
        string name,
        Func<TState, bool> condition,
        TAction action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return When(name, condition, _ => action);
    }

    /// <summary>
    /// Configures the clock used for run timestamps.
    /// </summary>
    public ModelBasedReflexAgentBuilder<TPercept, TState, TAction> WithTimeProvider(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        _timeProvider = timeProvider;
        return this;
    }

    /// <summary>
    /// Builds the immutable agent handle.
    /// </summary>
    public AgentHandle<ModelBasedReflexInput<TPercept, TState>, ModelBasedReflexOutput<TState, TAction>> Build()
    {
        if (_stateUpdater is null)
        {
            throw new InvalidOperationException("A state updater must be configured before building the agent.");
        }

        var agent = new ModelBasedReflexAgent<TPercept, TState, TAction>(
            _stateUpdater,
            _rules,
            _timeProvider);

        return new AgentHandle<ModelBasedReflexInput<TPercept, TState>, ModelBasedReflexOutput<TState, TAction>>(agent);
    }
}
