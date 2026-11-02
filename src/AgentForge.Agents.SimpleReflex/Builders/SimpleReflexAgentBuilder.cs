using AgentForge.Abstractions.Execution;
using AgentForge.Agents.SimpleReflex.Application.UseCases;
using AgentForge.Agents.SimpleReflex.Domain.Decisions;
using AgentForge.Agents.SimpleReflex.Domain.Rules;

namespace AgentForge.Agents.SimpleReflex.Builders;

/// <summary>
/// Creates a configured Simple Reflex agent using an ordered, fluent rule definition.
/// </summary>
public static class SimpleReflexAgentBuilder
{
    /// <summary>
    /// Starts a Simple Reflex agent definition.
    /// </summary>
    public static SimpleReflexAgentBuilder<TPercept, TAction> Create<TPercept, TAction>()
        where TPercept : notnull
        where TAction : notnull
    {
        return new SimpleReflexAgentBuilder<TPercept, TAction>();
    }
}

/// <summary>
/// Fluent configuration for a Simple Reflex agent.
/// </summary>
public sealed class SimpleReflexAgentBuilder<TPercept, TAction>
    where TPercept : notnull
    where TAction : notnull
{
    private readonly List<SimpleReflexRule<TPercept, TAction>> _rules = [];
    private TimeProvider? _timeProvider;

    /// <summary>
    /// Adds an ordered condition-action rule. The first matching rule wins.
    /// </summary>
    public SimpleReflexAgentBuilder<TPercept, TAction> When(
        string name,
        Func<TPercept, bool> condition,
        Func<TPercept, TAction> actionFactory)
    {
        _rules.Add(new SimpleReflexRule<TPercept, TAction>(name, condition, actionFactory));
        return this;
    }

    /// <summary>
    /// Adds a rule that proposes a constant action when its condition matches.
    /// </summary>
    public SimpleReflexAgentBuilder<TPercept, TAction> When(
        string name,
        Func<TPercept, bool> condition,
        TAction action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return When(name, condition, _ => action);
    }

    /// <summary>
    /// Configures the clock used for run timestamps. Primarily useful for deterministic hosts and tests.
    /// </summary>
    public SimpleReflexAgentBuilder<TPercept, TAction> WithTimeProvider(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        _timeProvider = timeProvider;
        return this;
    }

    /// <summary>
    /// Builds the immutable agent handle.
    /// </summary>
    public AgentHandle<TPercept, SimpleReflexOutput<TAction>> Build()
    {
        var agent = new SimpleReflexAgent<TPercept, TAction>(_rules, _timeProvider);
        return new AgentHandle<TPercept, SimpleReflexOutput<TAction>>(agent);
    }
}
