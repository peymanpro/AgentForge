using AgentForge.Abstractions.Execution;
using AgentForge.Models.Chat;
using AgentForge.Models.Contracts;
using AgentForge.Tools;

namespace AgentForge.Agents.ReAct.Builders;

/// <summary>
/// Starts a strongly typed fluent configuration for a ReAct agent.
/// </summary>
public static class ReActAgentBuilder
{
    /// <summary>
    /// Creates a builder for a ReAct agent.
    /// </summary>
    public static ReActAgentBuilderConfiguration Create() => new();
}

/// <summary>
/// Configures the model decision strategy, authorized tool boundary, and finite ReAct limits.
/// </summary>
public sealed class ReActAgentBuilderConfiguration
{
    private IReactDecisionStrategy? _decisionStrategy;
    private readonly List<ITool> _tools = [];
    private IToolAuthorizationPolicy? _authorizationPolicy;
    private string? _systemPrompt;
    private ModelGenerationOptions? _generationOptions;
    private int _maxModelTurns = 8;
    private int _maxToolInvocations = 12;
    private TimeSpan _timeout = TimeSpan.FromSeconds(45);
    private int _maxToolObservationCharacters = 16_384;
    private TimeProvider? _timeProvider;

    /// <summary>
    /// Configures a native tool-calling chat model using the built-in decision strategy.
    /// </summary>
    public ReActAgentBuilderConfiguration WithModel(IToolCallingChatModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        _decisionStrategy = new NativeToolCallingDecisionStrategy(model);
        return this;
    }

    /// <summary>
    /// Configures the provider-neutral strategy used to obtain model decisions.
    /// </summary>
    public ReActAgentBuilderConfiguration WithDecisionStrategy(IReactDecisionStrategy decisionStrategy)
    {
        ArgumentNullException.ThrowIfNull(decisionStrategy);
        _decisionStrategy = decisionStrategy;
        return this;
    }

    /// <summary>
    /// Adds one application-approved tool to this agent's tool registry.
    /// </summary>
    public ReActAgentBuilderConfiguration WithTool(ITool tool)
    {
        ArgumentNullException.ThrowIfNull(tool);
        _tools.Add(tool);
        return this;
    }

    /// <summary>
    /// Adds application-approved tools to this agent's tool registry.
    /// </summary>
    public ReActAgentBuilderConfiguration WithTools(IEnumerable<ITool> tools)
    {
        ArgumentNullException.ThrowIfNull(tools);
        foreach (var tool in tools)
        {
            ArgumentNullException.ThrowIfNull(tool);
            _tools.Add(tool);
        }

        return this;
    }

    /// <summary>
    /// Configures the application-controlled authorization policy applied to every tool invocation.
    /// </summary>
    public ReActAgentBuilderConfiguration WithAuthorization(IToolAuthorizationPolicy authorizationPolicy)
    {
        ArgumentNullException.ThrowIfNull(authorizationPolicy);
        _authorizationPolicy = authorizationPolicy;
        return this;
    }

    /// <summary>
    /// Configures application-controlled system instructions.
    /// </summary>
    public ReActAgentBuilderConfiguration WithSystemPrompt(string systemPrompt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(systemPrompt);
        _systemPrompt = systemPrompt;
        return this;
    }

    /// <summary>
    /// Configures provider-neutral generation options used for each model turn.
    /// </summary>
    public ReActAgentBuilderConfiguration WithGenerationOptions(ModelGenerationOptions generationOptions)
    {
        ArgumentNullException.ThrowIfNull(generationOptions);
        _generationOptions = generationOptions;
        return this;
    }

    /// <summary>
    /// Configures the maximum number of model requests allowed in one run.
    /// </summary>
    public ReActAgentBuilderConfiguration WithMaxModelTurns(int value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
        _maxModelTurns = value;
        return this;
    }

    /// <summary>
    /// Configures the maximum number of tool invocation attempts allowed in one run.
    /// </summary>
    public ReActAgentBuilderConfiguration WithMaxToolInvocations(int value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
        _maxToolInvocations = value;
        return this;
    }

    /// <summary>
    /// Configures the overall timeout for one run.
    /// </summary>
    public ReActAgentBuilderConfiguration WithTimeout(TimeSpan timeout)
    {
        if (timeout <= TimeSpan.Zero || timeout.TotalMilliseconds > 4294967294d)
            throw new ArgumentOutOfRangeException(nameof(timeout));
        _timeout = timeout;
        return this;
    }

    /// <summary>
    /// Configures the maximum serialized tool observation size sent back to the model.
    /// </summary>
    public ReActAgentBuilderConfiguration WithMaxToolObservationCharacters(int value)
    {
        if (value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
        _maxToolObservationCharacters = value;
        return this;
    }

    /// <summary>
    /// Configures the clock used for normalized run timestamps.
    /// </summary>
    public ReActAgentBuilderConfiguration WithTimeProvider(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);
        _timeProvider = timeProvider;
        return this;
    }

    /// <summary>
    /// Builds the immutable ReAct agent handle after validating required dependencies.
    /// </summary>
    public AgentHandle<string, ReActAgentOutput> Build()
    {
        if (_decisionStrategy is null)
            throw new InvalidOperationException("A decision strategy must be configured before building the ReAct agent.");
        if (_authorizationPolicy is null)
            throw new InvalidOperationException("An authorization policy must be configured before building the ReAct agent.");

        var options = new ReActAgentOptions(
            _systemPrompt,
            _generationOptions,
            _maxModelTurns,
            _maxToolInvocations,
            _timeout,
            _maxToolObservationCharacters);

        var agent = new ReActAgent(
            _decisionStrategy,
            new ToolRegistry(_tools),
            _authorizationPolicy,
            options,
            _timeProvider);

        return new AgentHandle<string, ReActAgentOutput>(agent);
    }
}
