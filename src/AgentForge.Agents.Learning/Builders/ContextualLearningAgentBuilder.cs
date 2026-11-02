using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Learning.Domain.Models;

namespace AgentForge.Agents.Learning.Builders;

/// <summary>Fluent construction entry point for a contextual LinUCB agent.</summary>
public static class ContextualLearningAgentBuilder
{
    /// <summary>Starts a builder for the specified action type.</summary>
    public static ContextualLearningAgentBuilder<TAction> Create<TAction>() where TAction : notnull => new();
}

/// <summary>Configures LinUCB exploration and execution clock.</summary>
public sealed class ContextualLearningAgentBuilder<TAction> where TAction : notnull
{
    private double _explorationCoefficient = 1.0;
    private TimeProvider? _clock;

    /// <summary>Sets the finite, non-negative LinUCB exploration coefficient.</summary>
    public ContextualLearningAgentBuilder<TAction> WithExplorationCoefficient(double value)
    {
        if (!double.IsFinite(value) || value < 0) throw new ArgumentOutOfRangeException(nameof(value));
        _explorationCoefficient = value;
        return this;
    }

    /// <summary>Configures the clock used for normalized run timestamps.</summary>
    public ContextualLearningAgentBuilder<TAction> WithTimeProvider(TimeProvider timeProvider)
    {
        _clock = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        return this;
    }

    /// <summary>Builds the AgentForge execution handle.</summary>
    public AgentHandle<ContextualLearningAgentInput<TAction>, ContextualLearningAgentOutput<TAction>> Build() =>
        new(new ContextualLearningAgent<TAction>(_explorationCoefficient, _clock));
}
