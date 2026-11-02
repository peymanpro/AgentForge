namespace AgentForge.Abstractions.Execution;

/// <summary>
/// Represents an agent that executes an input and returns a normalized run result.
/// </summary>
/// <typeparam name="TInput">The input accepted by the agent.</typeparam>
/// <typeparam name="TOutput">The output produced by the agent.</typeparam>
public interface IAgent<in TInput, TOutput>
{
    /// <summary>
    /// Executes one bounded agent run.
    /// </summary>
    /// <param name="input">The task or data supplied to the agent.</param>
    /// <param name="context">The identity and correlation context for this run.</param>
    /// <param name="options">The caller-selected run limits.</param>
    /// <param name="cancellationToken">A token used to request cooperative cancellation.</param>
    /// <returns>A normalized result describing the terminal outcome.</returns>
    Task<AgentRunResult<TOutput>> RunAsync(
        TInput input,
        AgentRunContext context,
        AgentRunOptions options,
        CancellationToken cancellationToken = default);
}
