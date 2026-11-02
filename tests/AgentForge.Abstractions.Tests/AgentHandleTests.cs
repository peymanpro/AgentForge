using AgentForge.Abstractions.Execution;
using Xunit;

namespace AgentForge.Abstractions.Tests;

public sealed class AgentHandleTests
{
    [Fact]
    public async Task RunAsync_CreatesRunContextAndUsesDefaultOptions()
    {
        var inner = new RecordingAgent();
        var handle = new AgentHandle<string, string>(inner);

        var result = await handle.RunAsync("hello");

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.Equal("hello", result.Output);
        Assert.NotNull(inner.Context);
        Assert.NotEqual(Guid.Empty, inner.Context.RunId);
        Assert.NotNull(inner.Options);
    }

    [Fact]
    public async Task RunAsync_ForwardsCallerOptions()
    {
        var inner = new RecordingAgent();
        var handle = new AgentHandle<string, string>(inner);
        var options = new AgentRunOptions { MaxSteps = 3 };

        await handle.RunAsync("hello", options);

        Assert.Same(options, inner.Options);
    }

    [Fact]
    public async Task RunAsync_ForwardsExplicitContextAndOptions()
    {
        var inner = new RecordingAgent();
        var handle = new AgentHandle<string, string>(inner);
        var context = AgentRunContext.Create();
        var options = new AgentRunOptions { MaxModelRequests = 2 };

        await handle.RunAsync("hello", context, options);

        Assert.Same(context, inner.Context);
        Assert.Same(options, inner.Options);
    }

    private sealed class RecordingAgent : IAgent<string, string>
    {
        public AgentRunContext? Context { get; private set; }

        public AgentRunOptions? Options { get; private set; }

        public Task<AgentRunResult<string>> RunAsync(
            string input,
            AgentRunContext context,
            AgentRunOptions options,
            CancellationToken cancellationToken = default)
        {
            Context = context;
            Options = options;
            var now = DateTimeOffset.UtcNow;
            return Task.FromResult(AgentRunResult<string>.Completed(input, now, now));
        }
    }
}
