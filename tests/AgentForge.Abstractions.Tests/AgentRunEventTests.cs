using AgentForge.Abstractions.Execution;
using Xunit;

namespace AgentForge.Abstractions.Tests;

public sealed class AgentRunEventTests
{
    [Fact]
    public void Constructor_CopiesEventAttributes()
    {
        var attributes = new Dictionary<string, string>
        {
            ["status"] = "started"
        };

        var agentEvent = new AgentRunEvent(
            Guid.NewGuid(),
            null,
            "run.started",
            DateTimeOffset.UtcNow,
            attributes);

        attributes["status"] = "changed";

        Assert.Equal("started", agentEvent.Attributes["status"]);
    }

    [Fact]
    public void Constructor_RejectsInvalidEventName()
    {
        Assert.Throws<ArgumentException>(() =>
            new AgentRunEvent(Guid.NewGuid(), null, "run started", DateTimeOffset.UtcNow));
    }

    [Fact]
    public async Task Publisher_UsesRunCorrelationAndConfiguredClock()
    {
        var sink = new RecordingSink();
        var options = new AgentRunOptions { EventSink = sink };
        var parent = AgentRunContext.Create();
        var child = parent.CreateChild();
        var timestamp = new DateTimeOffset(2026, 11, 2, 10, 0, 0, TimeSpan.Zero);

        await AgentRunEventPublisher.PublishAsync(
            options,
            child,
            "agent.started",
            timeProvider: new FixedTimeProvider(timestamp));

        var published = Assert.Single(sink.Events);
        Assert.Equal(child.RunId, published.RunId);
        Assert.Equal(parent.RunId, published.ParentRunId);
        Assert.Equal(timestamp, published.OccurredAt);
    }

    [Fact]
    public async Task Publisher_IsolatesObserverFailure()
    {
        var options = new AgentRunOptions { EventSink = new ThrowingSink() };

        await AgentRunEventPublisher.PublishAsync(
            options,
            AgentRunContext.Create(),
            "run.started");
    }

    [Fact]
    public async Task Publisher_PropagatesRunCancellation()
    {
        using var source = new CancellationTokenSource();
        await source.CancelAsync();
        var options = new AgentRunOptions { EventSink = new CancellationSink() };

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await AgentRunEventPublisher.PublishAsync(
                options,
                AgentRunContext.Create(),
                "run.started",
                cancellationToken: source.Token));
    }

    private sealed class RecordingSink : IAgentEventSink
    {
        public List<AgentRunEvent> Events { get; } = [];

        public Task PublishAsync(
            AgentRunEvent agentEvent,
            CancellationToken cancellationToken = default)
        {
            Events.Add(agentEvent);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingSink : IAgentEventSink
    {
        public Task PublishAsync(
            AgentRunEvent agentEvent,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("observer failure");
        }
    }

    private sealed class CancellationSink : IAgentEventSink
    {
        public Task PublishAsync(
            AgentRunEvent agentEvent,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }

    private sealed class FixedTimeProvider(DateTimeOffset timestamp) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => timestamp;
    }
}
