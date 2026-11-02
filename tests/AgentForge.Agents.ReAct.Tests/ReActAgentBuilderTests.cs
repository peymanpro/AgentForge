using AgentForge.Abstractions.Execution;
using AgentForge.Agents.ReAct.Builders;
using AgentForge.Models.Chat;
using AgentForge.Tools;
using Xunit;

namespace AgentForge.Agents.ReAct.Tests;

public sealed class ReActAgentBuilderTests
{
    [Fact]
    public void Build_RequiresDecisionStrategy()
    {
        var builder = ReActAgentBuilder.Create()
            .WithAuthorization(new AllowAllAuthorizationPolicy());

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Contains("decision strategy", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Build_RequiresAuthorizationPolicy()
    {
        var builder = ReActAgentBuilder.Create()
            .WithDecisionStrategy(new FixedDecisionStrategy());

        var exception = Assert.Throws<InvalidOperationException>(() => builder.Build());

        Assert.Contains("authorization policy", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Build_CreatesRunnableAgentWithFluentOptions()
    {
        var strategy = new FixedDecisionStrategy();
        var agent = ReActAgentBuilder.Create()
            .WithDecisionStrategy(strategy)
            .WithAuthorization(new AllowAllAuthorizationPolicy())
            .WithSystemPrompt("Follow the application policy.")
            .WithMaxModelTurns(3)
            .WithMaxToolInvocations(4)
            .WithTimeout(TimeSpan.FromSeconds(10))
            .WithMaxToolObservationCharacters(256)
            .WithGenerationOptions(new ModelGenerationOptions(modelId: "test-model", maxOutputTokens: 128))
            .Build();

        var result = await agent.RunAsync("Complete this task.");

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.Equal("done", result.Output?.FinalResponse);
        Assert.Equal(ChatMessageRole.System, strategy.LastRequest?.Messages[0].Role);
        Assert.Equal("Follow the application policy.", strategy.LastRequest?.Messages[0].Content);
        Assert.Equal("test-model", strategy.LastRequest?.Options.ModelId);
    }

    [Fact]
    public void WithLimits_RejectsNonPositiveValues()
    {
        var builder = ReActAgentBuilder.Create();

        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithMaxModelTurns(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithMaxToolInvocations(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithTimeout(TimeSpan.Zero));
        Assert.Throws<ArgumentOutOfRangeException>(() => builder.WithMaxToolObservationCharacters(0));
    }

    private sealed class FixedDecisionStrategy : IReactDecisionStrategy
    {
        public ModelRequest? LastRequest { get; private set; }

        public Task<ModelResponse> DecideAsync(
            ModelRequest request,
            IReadOnlyList<ToolDefinition> availableTools,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastRequest = request;
            return Task.FromResult(new ModelResponse(
                ChatMessage.Assistant("done"),
                ModelFinishReason.Stop));
        }
    }

    private sealed class AllowAllAuthorizationPolicy : IToolAuthorizationPolicy
    {
        public Task<ToolAuthorizationDecision> AuthorizeAsync(
            ToolAuthorizationRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(ToolAuthorizationDecision.Allowed);
        }
    }
}
