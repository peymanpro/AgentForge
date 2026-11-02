namespace AgentForge.Abstractions.Execution;

/// <summary>
/// Receives non-sensitive execution events emitted by AgentForge agents and workflows.
/// </summary>
public interface IAgentEventSink
{
    /// <summary>
    /// Publishes one structured execution event.
    /// </summary>
    /// <param name="agentEvent">The event to publish.</param>
    /// <param name="cancellationToken">A token used to request cooperative cancellation.</param>
    Task PublishAsync(
        AgentRunEvent agentEvent,
        CancellationToken cancellationToken = default);
}
