using System.Text.Json;
using AgentForge.Abstractions.Execution;
using AgentForge.Agents.ReAct;
using AgentForge.Models.Chat;
using AgentForge.Tools;
using Xunit;

namespace AgentForge.Agents.ReAct.Tests;

public sealed class ReActAgentTests
{
    [Fact]
    public async Task RunAsync_CompletesWhenModelReturnsFinalResponse()
    {
        var strategy = new ScriptedDecisionStrategy(FinalResponse("The answer is 42."));
        var agent = CreateAgent(strategy);

        var result = await agent.RunAsync("Answer the question.", AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.Equal("The answer is 42.", result.Output?.FinalResponse);
        Assert.Equal(1, result.Output?.ModelTurnCount);
        Assert.Empty(result.Output!.ToolExecutions);
    }

    [Fact]
    public async Task RunAsync_ExecutesToolAndReturnsObservationToModel()
    {
        var tool = new FakeTool("lookup", """{"value":"found"}""");
        var strategy = new ScriptedDecisionStrategy(
            ToolCallResponse(new ModelToolCall("call-1", "lookup", CreateJson("""{"id":"42"}"""))),
            FinalResponse("The record was found."));
        var agent = CreateAgent(strategy, [tool]);

        var result = await agent.RunAsync("Look up record 42.", AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.Equal("The record was found.", result.Output?.FinalResponse);
        Assert.Equal(1, result.Output?.ToolInvocationCount);
        Assert.Single(result.Output!.ToolExecutions);
        Assert.Equal(ToolExecutionStatus.Succeeded, result.Output.ToolExecutions[0].Status);
        Assert.Equal(1, tool.ExecutionCount);

        Assert.Equal(2, strategy.Requests.Count);
        Assert.Equal(3, strategy.Requests[1].Messages.Count);
        Assert.Equal(ChatMessageRole.Assistant, strategy.Requests[1].Messages[1].Role);
        Assert.Equal(ChatMessageRole.Tool, strategy.Requests[1].Messages[2].Role);
        Assert.Equal("call-1", strategy.Requests[1].Messages[2].ToolCallId);
        Assert.Contains("found", strategy.Requests[1].Messages[2].Content ?? string.Empty);
    }

    [Fact]
    public async Task RunAsync_DoesNotExecuteDeniedToolAndReturnsDenialObservation()
    {
        var tool = new FakeTool("delete", """{"deleted":true}""");
        var strategy = new ScriptedDecisionStrategy(
            ToolCallResponse(new ModelToolCall("call-1", "delete", CreateJson("""{"id":"42"}"""))),
            FinalResponse("I cannot perform that operation."));
        var agent = CreateAgent(
            strategy,
            [tool],
            new FixedAuthorizationPolicy(ToolAuthorizationDecision.Denied));

        var result = await agent.RunAsync("Delete record 42.", AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.Equal(0, tool.ExecutionCount);
        Assert.Equal(ToolExecutionStatus.Denied, result.Output?.ToolExecutions[0].Status);
        Assert.Contains("tool.denied", strategy.Requests[1].Messages[2].Content ?? string.Empty);
    }

    [Fact]
    public async Task RunAsync_StopsAtToolLimitAndPreservesPendingToolCalls()
    {
        var tool = new FakeTool("lookup", """{"value":"found"}""");
        var strategy = new ScriptedDecisionStrategy(
            ToolCallResponse(
                new ModelToolCall("call-1", "lookup", CreateJson("""{"id":"1"}""")),
                new ModelToolCall("call-2", "lookup", CreateJson("""{"id":"2"}"""))));
        var agent = CreateAgent(
            strategy,
            [tool],
            options: new ReActAgentOptions(maxToolInvocations: 1));

        var result = await agent.RunAsync("Look up two records.", AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.BudgetExhausted, result.Status);
        Assert.Equal(1, result.Output?.ToolInvocationCount);
        Assert.Single(result.Output!.ToolExecutions);
        Assert.Single(result.Output.PendingToolCalls);
        Assert.Equal("call-2", result.Output.PendingToolCalls[0].Id);
        Assert.Equal(1, tool.ExecutionCount);
    }

    [Fact]
    public async Task RunAsync_StopsAtModelTurnLimitAfterACompletedToolCall()
    {
        var tool = new FakeTool("lookup", """{"value":"found"}""");
        var strategy = new ScriptedDecisionStrategy(
            ToolCallResponse(new ModelToolCall("call-1", "lookup", CreateJson("""{"id":"1"}"""))),
            FinalResponse("This response must not be requested."));
        var agent = CreateAgent(
            strategy,
            [tool],
            options: new ReActAgentOptions(maxModelTurns: 1));

        var result = await agent.RunAsync("Look up one record.", AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.BudgetExhausted, result.Status);
        Assert.Single(result.Output!.ToolExecutions);
        Assert.Equal(1, result.Output.ModelTurnCount);
        Assert.Single(strategy.Requests);
    }

    [Fact]
    public async Task RunAsync_UsesApplicationSystemPromptAndGenerationOptions()
    {
        var strategy = new ScriptedDecisionStrategy(FinalResponse("done"));
        var generationOptions = new ModelGenerationOptions(modelId: "test-model", maxOutputTokens: 128);
        var agent = CreateAgent(
            strategy,
            options: new ReActAgentOptions(
                systemPrompt: "Follow the application policy.",
                generationOptions: generationOptions));

        await agent.RunAsync("Perform the task.", AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(ChatMessageRole.System, strategy.Requests[0].Messages[0].Role);
        Assert.Equal("Follow the application policy.", strategy.Requests[0].Messages[0].Content);
        Assert.Same(generationOptions, strategy.Requests[0].Options);
    }

    [Fact]
    public async Task RunAsync_TruncatesOversizedToolObservationsSentBackToModel()
    {
        var largeValue = new string('x', 512);
        var tool = new FakeTool("lookup", JsonSerializer.Serialize(new { payload = largeValue }));
        var strategy = new ScriptedDecisionStrategy(
            ToolCallResponse(new ModelToolCall("call-1", "lookup", CreateJson("""{"id":"1"}"""))),
            FinalResponse("done"));
        var agent = CreateAgent(
            strategy,
            [tool],
            options: new ReActAgentOptions(maxToolObservationCharacters: 64));

        var result = await agent.RunAsync("Look up the item.", AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        var observation = strategy.Requests[1].Messages[2].Content ?? string.Empty;
        Assert.Contains("ObservationTruncated", observation);
        Assert.DoesNotContain(largeValue, observation);
        Assert.Contains(largeValue, result.Output!.ToolExecutions[0].Output?.GetProperty("payload").GetString() ?? string.Empty);
    }

    [Fact]
    public async Task RunAsync_ReturnsSafeFailureWhenDecisionStrategyThrows()
    {
        var strategy = new ScriptedDecisionStrategy(throwOnDecision: true);
        var agent = CreateAgent(strategy);

        var result = await agent.RunAsync("Perform the task.", AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.Failed, result.Status);
        Assert.Equal(AgentFailureCategory.ModelProviderFailure, result.Failure?.Category);
        Assert.DoesNotContain("private provider endpoint", result.Failure?.Message ?? string.Empty);
    }

    [Fact]
    public async Task RunAsync_MapsCallerCancellationToCancelledStatus()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        var strategy = new ScriptedDecisionStrategy(blockUntilCancelled: true);
        var agent = CreateAgent(strategy);

        var result = await agent.RunAsync(
            "Perform the task.",
            AgentRunContext.Create(),
            new AgentRunOptions(),
            cancellation.Token);

        Assert.Equal(AgentRunStatus.Cancelled, result.Status);
        Assert.Equal(AgentFailureCategory.Cancelled, result.Failure?.Category);
        Assert.NotNull(result.Output);
    }

    [Fact]
    public async Task RunAsync_MapsTimeoutToTimedOutStatus()
    {
        var strategy = new ScriptedDecisionStrategy(blockUntilCancelled: true);
        var agent = CreateAgent(strategy, options: new ReActAgentOptions(timeout: TimeSpan.FromMilliseconds(100)));

        var result = await agent.RunAsync("Perform the task.", AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.TimedOut, result.Status);
        Assert.Equal(AgentFailureCategory.TimedOut, result.Failure?.Category);
    }

    [Fact]
    public async Task RunAsync_EmitsStructuredEventsWithoutTaskContent()
    {
        var sink = new RecordingEventSink();
        var strategy = new ScriptedDecisionStrategy(FinalResponse("done"));
        var agent = CreateAgent(strategy);
        var options = new AgentRunOptions { EventSink = sink };
        const string privateTask = "sensitive task content";

        await agent.RunAsync(privateTask, AgentRunContext.Create(), options);

        var names = sink.Events.Select(agentEvent => agentEvent.Name).ToArray();
        Assert.Contains("agent.run.started", names);
        Assert.Contains("model.request.started", names);
        Assert.Contains("model.response.received", names);
        Assert.Contains("agent.run.completed", names);
        Assert.DoesNotContain(sink.Events.SelectMany(agentEvent => agentEvent.Attributes.Values), value => value.Contains(privateTask, StringComparison.Ordinal));
    }

    private static ReActAgent CreateAgent(
        ScriptedDecisionStrategy strategy,
        IEnumerable<ITool>? tools = null,
        IToolAuthorizationPolicy? policy = null,
        ReActAgentOptions? options = null)
    {
        return new ReActAgent(
            strategy,
            new ToolRegistry(tools ?? []),
            policy ?? new FixedAuthorizationPolicy(ToolAuthorizationDecision.Allowed),
            options);
    }

    private static ModelResponse FinalResponse(string content)
    {
        return new ModelResponse(ChatMessage.Assistant(content), ModelFinishReason.Stop);
    }

    private static ModelResponse ToolCallResponse(params ModelToolCall[] calls)
    {
        return new ModelResponse(ChatMessage.AssistantToolCalls(calls), ModelFinishReason.ToolCalls);
    }

    private static JsonElement CreateJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private sealed class ScriptedDecisionStrategy : IReactDecisionStrategy
    {
        private readonly Queue<ModelResponse> _responses;
        private readonly bool _throwOnDecision;
        private readonly bool _blockUntilCancelled;

        public ScriptedDecisionStrategy(
            params ModelResponse[] responses)
        {
            _responses = new Queue<ModelResponse>(responses);
        }

        public ScriptedDecisionStrategy(
            bool throwOnDecision = false,
            bool blockUntilCancelled = false)
        {
            _responses = new Queue<ModelResponse>();
            _throwOnDecision = throwOnDecision;
            _blockUntilCancelled = blockUntilCancelled;
        }

        public List<ModelRequest> Requests { get; } = [];

        public async Task<ModelResponse> DecideAsync(
            ModelRequest request,
            IReadOnlyList<ToolDefinition> availableTools,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests.Add(request);

            if (_blockUntilCancelled)
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            }

            if (_throwOnDecision)
            {
                throw new InvalidOperationException("private provider endpoint and credentials");
            }

            if (_responses.Count == 0)
            {
                throw new InvalidOperationException("The scripted strategy has no response remaining.");
            }

            return _responses.Dequeue();
        }
    }

    private sealed class FixedAuthorizationPolicy(ToolAuthorizationDecision decision) : IToolAuthorizationPolicy
    {
        public Task<ToolAuthorizationDecision> AuthorizeAsync(
            ToolAuthorizationRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(decision);
        }
    }

    private sealed class FakeTool : ITool
    {
        private readonly JsonElement _output;

        public FakeTool(string name, string outputJson)
        {
            using var schema = JsonDocument.Parse("""{"type":"object","properties":{"id":{"type":"string"}}}""");
            Definition = new ToolDefinition(name, "A deterministic test tool.", schema.RootElement);
            _output = CreateJson(outputJson);
        }

        public ToolDefinition Definition { get; }

        public int ExecutionCount { get; private set; }

        public ToolValidationResult Validate(JsonElement arguments)
        {
            if (arguments.ValueKind == JsonValueKind.Object &&
                arguments.TryGetProperty("id", out var id) &&
                id.ValueKind == JsonValueKind.String)
            {
                return ToolValidationResult.Valid();
            }

            return ToolValidationResult.Invalid([
                new ToolValidationIssue("$.id", "A string id is required.")
            ]);
        }

        public Task<JsonElement> ExecuteAsync(
            JsonElement arguments,
            AgentRunContext runContext,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ExecutionCount++;
            return Task.FromResult(_output.Clone());
        }
    }

    private sealed class RecordingEventSink : IAgentEventSink
    {
        public List<AgentRunEvent> Events { get; } = [];

        public Task PublishAsync(
            AgentRunEvent agentEvent,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Events.Add(agentEvent);
            return Task.CompletedTask;
        }
    }
}
