namespace AgentForge.Abstractions.Execution;

/// <summary>
/// Provides a concise application-facing entry point for executing a configured agent.
/// </summary>
/// <typeparam name="TInput">The input accepted by the agent.</typeparam>
/// <typeparam name="TOutput">The output produced by the agent.</typeparam>
/// <remarks>
/// The handle supplies a fresh run context and default run options for the common case.
/// Callers that need correlation, parent-child relationships, custom budgets, or event sinks
/// can use the overload that accepts an explicit context and options.
/// </remarks>
public sealed class AgentHandle<TInput, TOutput> : IAgent<TInput, TOutput>
{
    private readonly IAgent<TInput, TOutput> _agent;

    /// <summary>
    /// Wraps an agent implementation with the concise execution API.
    /// </summary>
    /// <param name="agent">The configured agent implementation.</param>
    public AgentHandle(IAgent<TInput, TOutput> agent)
    {
        ArgumentNullException.ThrowIfNull(agent);
        _agent = agent;
    }

    /// <summary>
    /// Executes the agent with default run options and a newly created run context.
    /// </summary>
    /// <param name="input">The task or percept supplied to the agent.</param>
    /// <param name="cancellationToken">A token used to request cooperative cancellation.</param>
    /// <returns>The normalized outcome of the run.</returns>
    public Task<AgentRunResult<TOutput>> RunAsync(
        TInput input,
        CancellationToken cancellationToken = default)
    {
        return RunAsync(input, new AgentRunOptions(), cancellationToken);
    }

    /// <summary>
    /// Executes the agent with caller-supplied run options and a newly created run context.
    /// </summary>
    /// <param name="input">The task or percept supplied to the agent.</param>
    /// <param name="options">The resource limits and optional event sink for this run.</param>
    /// <param name="cancellationToken">A token used to request cooperative cancellation.</param>
    /// <returns>The normalized outcome of the run.</returns>
    public Task<AgentRunResult<TOutput>> RunAsync(
        TInput input,
        AgentRunOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        return _agent.RunAsync(input, AgentRunContext.Create(), options, cancellationToken);
    }

    /// <inheritdoc />
    public Task<AgentRunResult<TOutput>> RunAsync(
        TInput input,
        AgentRunContext context,
        AgentRunOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(options);
        return _agent.RunAsync(input, context, options, cancellationToken);
    }
}
