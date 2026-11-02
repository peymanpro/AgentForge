using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Reflection.Evaluation;
using AgentForge.Agents.Reflection.Generation;

namespace AgentForge.Agents.Reflection.Builders;

/// <summary>Starts a strongly typed fluent configuration for a Reflection agent.</summary>
public static class ReflectionAgentBuilder
{
    /// <summary>Creates a builder for a Reflection agent.</summary>
    public static ReflectionAgentBuilderConfiguration Create() => new();
}

/// <summary>Configures candidate generation, evaluation, and bounded refinement behavior.</summary>
public sealed class ReflectionAgentBuilderConfiguration
{
    private IReflectionGenerationStrategy? _generationStrategy;
    private IReflectionEvaluator? _evaluator;
    private int _maxAttempts = 3;
    private TimeSpan _timeout = TimeSpan.FromSeconds(90);
    private bool _stopOnNoImprovement;
    private double _minimumScoreImprovement;
    private TimeProvider? _timeProvider;

    /// <summary>Configures the strategy that generates and refines candidates.</summary>
    public ReflectionAgentBuilderConfiguration WithGenerator(IReflectionGenerationStrategy generationStrategy)
    {
        ArgumentNullException.ThrowIfNull(generationStrategy);
        _generationStrategy = generationStrategy;
        return this;
    }

    /// <summary>Configures the independent evaluator used to assess each candidate.</summary>
    public ReflectionAgentBuilderConfiguration WithEvaluator(IReflectionEvaluator evaluator)
    {
        ArgumentNullException.ThrowIfNull(evaluator);
        _evaluator = evaluator;
        return this;
    }

    /// <summary>Sets the maximum number of candidate generation/evaluation attempts.</summary>
    public ReflectionAgentBuilderConfiguration WithMaxAttempts(int value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
        _maxAttempts = value;
        return this;
    }

    /// <summary>Sets the overall generation and evaluation timeout.</summary>
    public ReflectionAgentBuilderConfiguration WithTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero || timeout.TotalMilliseconds > 4294967294d)
            throw new ArgumentOutOfRangeException(nameof(timeout));
        _timeout = timeout;
        return this;
    }

    /// <summary>Stops refinement when an evaluated candidate does not improve on the best score.</summary>
    public ReflectionAgentBuilderConfiguration WithStopOnNoImprovement(bool enabled = true)
    {
        _stopOnNoImprovement = enabled;
        return this;
    }

    /// <summary>Sets the minimum normalized score increase required to count as improvement.</summary>
    public ReflectionAgentBuilderConfiguration WithMinimumScoreImprovement(double value)
    {
        if (!double.IsFinite(value) || value < 0 || value > 1)
            throw new ArgumentOutOfRangeException(nameof(value));
        _minimumScoreImprovement = value;
        return this;
    }

    /// <summary>Configures the clock used for normalized run timestamps.</summary>
    public ReflectionAgentBuilderConfiguration WithTimeProvider(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        _timeProvider = timeProvider;
        return this;
    }

    /// <summary>Builds the immutable Reflection agent handle.</summary>
    public AgentHandle<string, ReflectionAgentOutput> Build()
    {
        if (_generationStrategy is null)
            throw new InvalidOperationException("A generation strategy must be configured before building the Reflection agent.");
        if (_evaluator is null)
            throw new InvalidOperationException("An evaluator must be configured before building the Reflection agent.");

        var options = new ReflectionAgentOptions(
            _maxAttempts,
            _timeout,
            _stopOnNoImprovement,
            _minimumScoreImprovement);

        var agent = new ReflectionAgent(_generationStrategy, _evaluator, options, _timeProvider);
        return new AgentHandle<string, ReflectionAgentOutput>(agent);
    }
}
