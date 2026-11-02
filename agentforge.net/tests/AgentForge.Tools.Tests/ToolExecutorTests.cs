using System.Text.Json;
using AgentForge.Abstractions.Execution;
using AgentForge.Tools;
using Xunit;

namespace AgentForge.Tools.Tests;

public sealed class ToolExecutorTests
{
    [Fact]
    public async Task ExecuteAsync_ExecutesOnlyAfterValidationAndAuthorization()
    {
        var order = new List<string>();
        var tool = new FakeTool("lookup", order);
        var policy = new FakePolicy(ToolAuthorizationDecision.Allowed, order);
        var executor = new ToolExecutor(new ToolRegistry([tool]), policy);

        var result = await executor.ExecuteAsync(CreateRequest("lookup"));

        Assert.Equal(ToolExecutionStatus.Succeeded, result.Status);
        Assert.Equal(1, tool.ExecutionCount);
        Assert.Equal(1, policy.CallCount);
        Assert.Equal(new[] { "validate", "authorize", "execute" }, order);
        Assert.True(result.Output.HasValue);
        Assert.Equal(JsonValueKind.Object, result.Output.Value.ValueKind);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotInvokeToolOrPolicyWhenArgumentsAreInvalid()
    {
        var order = new List<string>();
        var tool = new FakeTool(
            "lookup",
            order,
            ToolValidationResult.Invalid([new ToolValidationIssue("$.id", "The id is required.")]));
        var policy = new FakePolicy(ToolAuthorizationDecision.Allowed, order);
        var executor = new ToolExecutor(new ToolRegistry([tool]), policy);

        var result = await executor.ExecuteAsync(CreateRequest("lookup"));

        Assert.Equal(ToolExecutionStatus.InvalidArguments, result.Status);
        Assert.Equal(0, tool.ExecutionCount);
        Assert.Equal(0, policy.CallCount);
        Assert.Equal(AgentFailureCategory.ValidationFailure, result.Failure?.Category);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotInvokeDeniedTool()
    {
        var order = new List<string>();
        var tool = new FakeTool("delete", order);
        var policy = new FakePolicy(ToolAuthorizationDecision.Denied, order);
        var executor = new ToolExecutor(new ToolRegistry([tool]), policy);

        var result = await executor.ExecuteAsync(CreateRequest("delete"));

        Assert.Equal(ToolExecutionStatus.Denied, result.Status);
        Assert.Equal(0, tool.ExecutionCount);
        Assert.Equal(AgentFailureCategory.PolicyDenied, result.Failure?.Category);
        Assert.Equal(new[] { "validate", "authorize" }, order);
    }

    [Fact]
    public async Task ExecuteAsync_FailsClosedWhenApprovalIsRequired()
    {
        var order = new List<string>();
        var tool = new FakeTool("delete", order);
        var policy = new FakePolicy(ToolAuthorizationDecision.ApprovalRequired, order);
        var executor = new ToolExecutor(new ToolRegistry([tool]), policy);

        var result = await executor.ExecuteAsync(CreateRequest("delete"));

        Assert.Equal(ToolExecutionStatus.ApprovalRequired, result.Status);
        Assert.Equal(0, tool.ExecutionCount);
        Assert.Equal(["validate", "authorize"], order);
    }

    [Fact]
    public async Task ExecuteAsync_ReturnsNotFoundForUnregisteredTool()
    {
        var order = new List<string>();
        var executor = new ToolExecutor(
            new ToolRegistry([new FakeTool("lookup", order)]),
            new FakePolicy(ToolAuthorizationDecision.Allowed, order));

        var result = await executor.ExecuteAsync(CreateRequest("unknown"));

        Assert.Equal(ToolExecutionStatus.NotFound, result.Status);
        Assert.Empty(order);
    }

    [Fact]
    public async Task ExecuteAsync_FailsClosedWhenAuthorizationPolicyThrows()
    {
        var order = new List<string>();
        var tool = new FakeTool("delete", order);
        var policy = new FakePolicy(ToolAuthorizationDecision.Allowed, order, throwOnAuthorize: true);
        var executor = new ToolExecutor(new ToolRegistry([tool]), policy);

        var result = await executor.ExecuteAsync(CreateRequest("delete"));

        Assert.Equal(ToolExecutionStatus.Failed, result.Status);
        Assert.Equal(AgentFailureCategory.AuthorizationFailed, result.Failure?.Category);
        Assert.Equal(0, tool.ExecutionCount);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotExposeToolExceptionDetails()
    {
        var order = new List<string>();
        var tool = new FakeTool("lookup", order, throwOnExecute: true);
        var executor = new ToolExecutor(
            new ToolRegistry([tool]),
            new FakePolicy(ToolAuthorizationDecision.Allowed, order));

        var result = await executor.ExecuteAsync(CreateRequest("lookup"));

        Assert.Equal(ToolExecutionStatus.Failed, result.Status);
        Assert.Equal(AgentFailureCategory.ToolFailure, result.Failure?.Category);
        Assert.DoesNotContain("private database host", result.Failure?.Message ?? string.Empty);
    }

    [Fact]
    public async Task ExecuteAsync_PropagatesCallerCancellation()
    {
        using var source = new CancellationTokenSource();
        await source.CancelAsync();

        var order = new List<string>();
        var tool = new FakeTool("lookup", order);
        var executor = new ToolExecutor(
            new ToolRegistry([tool]),
            new FakePolicy(ToolAuthorizationDecision.Allowed, order));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => executor.ExecuteAsync(CreateRequest("lookup"), source.Token));

        Assert.Empty(order);
    }

    [Fact]
    public async Task ExecuteAsync_DoesNotTreatUndefinedOutputAsSuccess()
    {
        var order = new List<string>();
        var tool = new FakeTool("lookup", order, returnUndefinedOutput: true);
        var executor = new ToolExecutor(
            new ToolRegistry([tool]),
            new FakePolicy(ToolAuthorizationDecision.Allowed, order));

        var result = await executor.ExecuteAsync(CreateRequest("lookup"));

        Assert.Equal(ToolExecutionStatus.Failed, result.Status);
        Assert.Equal(AgentFailureCategory.ToolFailure, result.Failure?.Category);
    }

    private static ToolExecutionRequest CreateRequest(string toolName)
    {
        using var document = JsonDocument.Parse("""{"id":"item-1"}""");
        return new ToolExecutionRequest(
            "call-1",
            toolName,
            document.RootElement,
            AgentRunContext.Create());
    }

    private sealed class FakePolicy(
        ToolAuthorizationDecision decision,
        List<string> order,
        bool throwOnAuthorize = false) : IToolAuthorizationPolicy
    {
        public int CallCount { get; private set; }

        public Task<ToolAuthorizationDecision> AuthorizeAsync(
            ToolAuthorizationRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            CallCount++;
            order.Add("authorize");

            if (throwOnAuthorize)
            {
                throw new InvalidOperationException("private policy details");
            }

            return Task.FromResult(decision);
        }
    }

    private sealed class FakeTool(
        string name,
        List<string> order,
        ToolValidationResult? validation = null,
        bool throwOnExecute = false,
        bool returnUndefinedOutput = false) : ITool
    {
        public int ExecutionCount { get; private set; }

        public ToolDefinition Definition { get; } = CreateDefinition(name);

        public ToolValidationResult Validate(JsonElement arguments)
        {
            order.Add("validate");
            return validation ?? ToolValidationResult.Valid();
        }

        public Task<JsonElement> ExecuteAsync(
            JsonElement arguments,
            AgentRunContext runContext,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ExecutionCount++;
            order.Add("execute");

            if (throwOnExecute)
            {
                throw new InvalidOperationException("private database host must not be exposed");
            }

            if (returnUndefinedOutput)
            {
                return Task.FromResult(default(JsonElement));
            }

            return Task.FromResult(CreateJson("""{"ok":true}"""));
        }
    }

    private static ToolDefinition CreateDefinition(string name)
    {
        using var document = JsonDocument.Parse("""{"type":"object"}""");
        return new ToolDefinition(name, "A test tool.", document.RootElement);
    }

    private static JsonElement CreateJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
