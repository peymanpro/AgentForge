using AgentForge.Abstractions.Execution;
using AgentForge.Agents.RuleBased.Models;

namespace AgentForge.Agents.RuleBased.Builders;

/// <summary>Creates a fluent builder for a rule-based production-system agent.</summary>
public static class RuleBasedAgentBuilder
{
    /// <summary>Starts a rule-based agent definition for the supplied memory and action types.</summary>
    public static RuleBasedAgentBuilder<TMemory, TAction> Create<TMemory, TAction>()
        where TMemory : notnull
        where TAction : notnull => new();
}

/// <summary>Configures ordered production rules and bounded forward chaining.</summary>
public sealed class RuleBasedAgentBuilder<TMemory, TAction>
    where TMemory : notnull
    where TAction : notnull
{
    private readonly List<ProductionRule<TMemory, TAction>> _rules = [];
    private int _maxFirings = 32;
    private TimeProvider _timeProvider = TimeProvider.System;

    /// <summary>Adds a production rule. The first applicable rule fires, then evaluation restarts at the first rule.</summary>
    public RuleBasedAgentBuilder<TMemory, TAction> When(
        string name,
        Func<TMemory, bool> condition,
        Func<TMemory, TAction> actionFactory,
        Func<TMemory, TMemory> apply)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(condition);
        ArgumentNullException.ThrowIfNull(actionFactory);
        ArgumentNullException.ThrowIfNull(apply);
        if (_rules.Any(rule => string.Equals(rule.Name, name, StringComparison.Ordinal)))
            throw new ArgumentException($"A rule named '{name}' is already registered.", nameof(name));

        _rules.Add(new ProductionRule<TMemory, TAction>(name, condition, actionFactory, apply));
        return this;
    }

    /// <summary>Sets the maximum number of rule firings in one run.</summary>
    public RuleBasedAgentBuilder<TMemory, TAction> WithMaxFirings(int value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
        _maxFirings = value;
        return this;
    }

    /// <summary>Configures the clock used for normalized run timestamps.</summary>
    public RuleBasedAgentBuilder<TMemory, TAction> WithTimeProvider(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        _timeProvider = timeProvider;
        return this;
    }

    /// <summary>Builds the immutable production-system execution handle.</summary>
    public AgentHandle<RuleBasedAgentInput<TMemory>, RuleBasedAgentOutput<TMemory, TAction>> Build()
    {
        if (_rules.Count == 0)
            throw new InvalidOperationException("At least one production rule must be registered before building the agent.");

        return new AgentHandle<RuleBasedAgentInput<TMemory>, RuleBasedAgentOutput<TMemory, TAction>>(
            new RuleBasedAgent<TMemory, TAction>(_rules.ToArray(), _maxFirings, _timeProvider));
    }
}