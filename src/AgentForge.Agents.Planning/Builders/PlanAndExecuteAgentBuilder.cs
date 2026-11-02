using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Planning.Execution;
using AgentForge.Agents.Planning.Planning;

namespace AgentForge.Agents.Planning.Builders;

/// <summary>
/// Starts a strongly typed fluent configuration for a Plan-and-Execute agent.
/// </summary>
public static class PlanAndExecuteAgentBuilder
{
    /// <summary>Creates a builder for a Plan-and-Execute agent.</summary>
    public static PlanAndExecuteAgentBuilderConfiguration Create() => new();
}

/// <summary>
/// Configures plan generation, the allow-listed step handlers, and finite execution limits.
/// </summary>
public sealed class PlanAndExecuteAgentBuilderConfiguration
{
    private IPlanningStrategy? _planningStrategy;
    private readonly List<IPlanStepHandler> _handlers = [];
    private int _maxPlanSteps = 12;
    private TimeSpan _timeout = TimeSpan.FromSeconds(90);
    private PlanValidator? _validator;
    private TimeProvider? _timeProvider;

    /// <summary>Configures the strategy that generates structured plan candidates.</summary>
    public PlanAndExecuteAgentBuilderConfiguration WithPlanner(IPlanningStrategy planningStrategy)
    {
        ArgumentNullException.ThrowIfNull(planningStrategy);
        _planningStrategy = planningStrategy;
        return this;
    }

    /// <summary>Adds one explicitly permitted plan step handler.</summary>
    public PlanAndExecuteAgentBuilderConfiguration WithStepHandler(IPlanStepHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _handlers.Add(handler);
        return this;
    }

    /// <summary>Adds explicitly permitted plan step handlers.</summary>
    public PlanAndExecuteAgentBuilderConfiguration WithStepHandlers(IEnumerable<IPlanStepHandler> handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);
        foreach (var handler in handlers)
        {
            ArgumentNullException.ThrowIfNull(handler);
            _handlers.Add(handler);
        }

        return this;
    }

    /// <summary>Sets the maximum number of steps accepted in a generated plan.</summary>
    public PlanAndExecuteAgentBuilderConfiguration WithMaxPlanSteps(int value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
        _maxPlanSteps = value;
        return this;
    }

    /// <summary>Sets the overall plan generation and execution timeout.</summary>
    public PlanAndExecuteAgentBuilderConfiguration WithTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero || timeout.TotalMilliseconds > 4294967294d)
            throw new ArgumentOutOfRangeException(nameof(timeout));
        _timeout = timeout;
        return this;
    }

    /// <summary>Uses a custom validator for generated plan candidates.</summary>
    public PlanAndExecuteAgentBuilderConfiguration WithValidator(PlanValidator validator)
    {
        ArgumentNullException.ThrowIfNull(validator);
        _validator = validator;
        return this;
    }

    /// <summary>Configures the clock used for normalized run timestamps.</summary>
    public PlanAndExecuteAgentBuilderConfiguration WithTimeProvider(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        _timeProvider = timeProvider;
        return this;
    }

    /// <summary>Builds the immutable Plan-and-Execute agent handle.</summary>
    public AgentHandle<string, PlanAndExecuteOutput> Build()
    {
        if (_planningStrategy is null)
            throw new InvalidOperationException("A planning strategy must be configured before building the agent.");

        var options = new PlanAndExecuteAgentOptions(_maxPlanSteps, _timeout);
        var registry = new PlanStepHandlerRegistry(_handlers);
        var agent = new PlanAndExecuteAgent(
            _planningStrategy,
            registry,
            options,
            _validator,
            _timeProvider);

        return new AgentHandle<string, PlanAndExecuteOutput>(agent);
    }
}
