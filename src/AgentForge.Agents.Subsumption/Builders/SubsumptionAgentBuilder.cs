using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Subsumption.Models;

namespace AgentForge.Agents.Subsumption.Builders;

/// <summary>Creates a fluent builder for a layered Subsumption agent.</summary>
public static class SubsumptionAgentBuilder
{
    /// <summary>Starts a Subsumption agent definition for the supplied percept and action types.</summary>
    public static SubsumptionAgentBuilder<TPercept, TAction> Create<TPercept, TAction>()
        where TPercept : notnull
        where TAction : notnull => new();
}

/// <summary>Configures priority-ordered reactive behavior layers.</summary>
public sealed class SubsumptionAgentBuilder<TPercept, TAction>
    where TPercept : notnull
    where TAction : notnull
{
    private readonly List<ReactiveLayer<TPercept, TAction>> _layers = [];
    private TimeProvider _timeProvider = TimeProvider.System;

    /// <summary>Adds a behavior layer. Higher priorities are evaluated first; the first active layer suppresses lower layers.</summary>
    public SubsumptionAgentBuilder<TPercept, TAction> WithLayer(
        string name,
        int priority,
        Func<TPercept, bool> isActive,
        Func<TPercept, TAction> actionFactory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(isActive);
        ArgumentNullException.ThrowIfNull(actionFactory);
        if (_layers.Any(layer => string.Equals(layer.Name, name, StringComparison.Ordinal)))
            throw new ArgumentException($"A layer named '{name}' is already registered.", nameof(name));

        _layers.Add(new ReactiveLayer<TPercept, TAction>(name, priority, isActive, actionFactory));
        return this;
    }

    /// <summary>Configures the clock used for normalized run timestamps.</summary>
    public SubsumptionAgentBuilder<TPercept, TAction> WithTimeProvider(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        _timeProvider = timeProvider;
        return this;
    }

    /// <summary>Builds the immutable priority-arbitrated reactive agent handle.</summary>
    public AgentHandle<SubsumptionAgentInput<TPercept>, SubsumptionAgentOutput<TAction>> Build()
    {
        if (_layers.Count == 0)
            throw new InvalidOperationException("At least one behavior layer must be registered before building the agent.");

        var orderedLayers = _layers
            .OrderByDescending(layer => layer.Priority)
            .ToArray();

        return new AgentHandle<SubsumptionAgentInput<TPercept>, SubsumptionAgentOutput<TAction>>(
            new SubsumptionAgent<TPercept, TAction>(orderedLayers, _timeProvider));
    }
}