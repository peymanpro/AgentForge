namespace AgentForge.Abstractions.Execution;

/// <summary>
/// Publishes execution events while isolating observer failures from agent business outcomes.
/// </summary>
public static class AgentRunEventPublisher
{
    /// <summary>
    /// Publishes an event when a sink is configured for the run.
    /// </summary>
    /// <param name="options">The run options containing the optional event sink.</param>
    /// <param name="context">The run identity and correlation context.</param>
    /// <param name="eventName">The stable event name.</param>
    /// <param name="attributes">Optional non-sensitive metadata.</param>
    /// <param name="timeProvider">The clock used to timestamp the event.</param>
    /// <param name="cancellationToken">The run cancellation token.</param>
    /// <returns>A task that completes after publication or an isolated observer failure.</returns>
    public static async ValueTask PublishAsync(
        AgentRunOptions options,
        AgentRunContext context,
        string eventName,
        IReadOnlyDictionary<string, string>? attributes = null,
        TimeProvider? timeProvider = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(context);

        var sink = options.EventSink;
        if (sink is null)
        {
            return;
        }

        var clock = timeProvider ?? TimeProvider.System;
        var agentEvent = new AgentRunEvent(
            context.RunId,
            context.ParentRunId,
            eventName,
            clock.GetUtcNow(),
            attributes);

        try
        {
            await sink.PublishAsync(agentEvent, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            // Observability must not silently change the agent's business outcome.
            // Sink implementations should report their own delivery failures safely.
        }
    }
}
