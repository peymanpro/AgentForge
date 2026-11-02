using System.Text.Json;
using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Planning;
using AgentForge.Agents.Planning.Execution;
using AgentForge.Agents.Planning.Planning;
using Xunit;

namespace AgentForge.Agents.Planning.Tests;

public sealed class PlanAndExecuteAgentTests
{
    [Fact]
    public async Task RunAsync_ExecutesSequentialPlanAndPassesOnlyDeclaredOutputs()
    {
        var observedReferences = new List<IReadOnlyDictionary<string, JsonElement>>();
        var first = new FakeHandler("produce", _ => CreateJson("""{"value":"alpha"}"""));
        var second = new FakeHandler("independent", _ => CreateJson("""{"value":"beta"}"""));
        var third = new FakeHandler("consume", context =>
        {
            observedReferences.Add(context.ReferencedOutputs);
            Assert.Single(context.ReferencedOutputs);
            Assert.Equal("alpha", context.ReferencedOutputs["first"].GetProperty("value").GetString());
            return CreateJson("""{"consumed":true}""");
        });

        var plan = new ExecutionPlan(
            "Produce and consume values.",
            [
                new PlanStep("first", "produce", "Produce the first value."),
                new PlanStep("second", "independent", "Produce an independent value."),
                new PlanStep("third", "consume", "Consume only the first value.", ["first"])
            ]);
        var agent = CreateAgent(plan, [first, second, third]);

        var result = await agent.RunAsync(
            "Run the planned task.",
            AgentRunContext.Create(),
            new AgentRunOptions());

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.Equal(3, result.Output?.CompletedStepCount);
        Assert.Equal(new[] { "first", "second", "third" }, result.Output?.Steps.Select(step => step.StepId));
        Assert.Single(observedReferences);
        Assert.Equal(1, first.ExecutionCount);
        Assert.Equal(1, second.ExecutionCount);
        Assert.Equal(1, third.ExecutionCount);
    }

    [Fact]
    public async Task RunAsync_RejectsEmptyPlanBeforeAnyStepExecutes()
    {
        var handler = new FakeHandler("work", _ => CreateJson("""{"ok":true}"""));
        var plan = new ExecutionPlan("No steps.", []);
        var agent = CreateAgent(plan, [handler]);

        var result = await agent.RunAsync("Perform the task.", AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.Rejected, result.Status);
        Assert.Equal("plan.steps_required", result.Failure?.Code);
        Assert.Equal(0, handler.ExecutionCount);
    }

    [Fact]
    public async Task RunAsync_RejectsDuplicateStepIdentifiersBeforeExecution()
    {
        var handler = new FakeHandler("work", _ => CreateJson("""{"ok":true}"""));
        var plan = new ExecutionPlan(
            "Duplicate IDs.",
            [
                new PlanStep("same", "work", "First."),
                new PlanStep("same", "work", "Second.")
            ]);
        var agent = CreateAgent(plan, [handler]);

        var result = await agent.RunAsync("Perform the task.", AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.Rejected, result.Status);
        Assert.Equal("plan.duplicate_step_id", result.Failure?.Code);
        Assert.Equal(0, handler.ExecutionCount);
    }

    [Fact]
    public async Task RunAsync_RejectsSelfReferencesAndForwardReferences()
    {
        var handler = new FakeHandler("work", _ => CreateJson("""{"ok":true}"""));
        var plan = new ExecutionPlan(
            "Invalid references.",
            [
                new PlanStep("first", "work", "Self reference.", ["first"]),
                new PlanStep("second", "work", "Forward reference.", ["third"]),
                new PlanStep("third", "work", "Later step.")
            ]);
        var agent = CreateAgent(plan, [handler]);

        var result = await agent.RunAsync("Perform the task.", AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.Rejected, result.Status);
        Assert.Equal("plan.forward_or_unknown_reference", result.Failure?.Code);
        Assert.Equal(0, handler.ExecutionCount);
    }

    [Fact]
    public async Task RunAsync_RejectsUnknownStepTypeBeforeExecution()
    {
        var handler = new FakeHandler("known", _ => CreateJson("""{"ok":true}"""));
        var plan = new ExecutionPlan(
            "Unknown step type.",
            [new PlanStep("first", "unknown", "Use an unavailable handler.")]);
        var agent = CreateAgent(plan, [handler]);

        var result = await agent.RunAsync("Perform the task.", AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.Rejected, result.Status);
        Assert.Equal("plan.unknown_step_type", result.Failure?.Code);
        Assert.Equal(0, handler.ExecutionCount);
    }

    [Fact]
    public async Task RunAsync_StopsOnFailureAndPreservesCompletedAndBlockedStates()
    {
        var handler = new FakeHandler(
            "work",
            context =>
            {
                if (context.Step.Id == "fail")
                {
                    throw new InvalidOperationException("private backend details");
                }

                return CreateJson("""{"ok":true}""");
            });
        var plan = new ExecutionPlan(
            "Stop on failure.",
            [
                new PlanStep("first", "work", "First succeeds."),
                new PlanStep("fail", "work", "Second fails."),
                new PlanStep("last", "work", "Must not execute.")
            ]);
        var agent = CreateAgent(plan, [handler]);

        var result = await agent.RunAsync("Perform the task.", AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.PartiallyCompleted, result.Status);
        Assert.Equal(1, result.Output?.CompletedStepCount);
        Assert.Collection(
            result.Output!.Steps,
            first => Assert.Equal(PlanStepStatus.Completed, first.Status),
            failed =>
            {
                Assert.Equal(PlanStepStatus.Failed, failed.Status);
                Assert.Equal("plan.step_execution_failed", failed.Failure?.Code);
            },
            blocked => Assert.Equal(PlanStepStatus.Blocked, blocked.Status));
        Assert.DoesNotContain("private backend details", result.Failure?.Message ?? string.Empty);
        Assert.Equal(2, handler.ExecutionCount);
    }

    [Fact]
    public async Task RunAsync_StopsAtExecutionStepBudgetAndMarksRemainingStepsSkipped()
    {
        var handler = new FakeHandler("work", _ => CreateJson("""{"ok":true}"""));
        var plan = new ExecutionPlan(
            "Bounded plan.",
            [
                new PlanStep("first", "work", "First."),
                new PlanStep("second", "work", "Second."),
                new PlanStep("third", "work", "Third.")
            ]);
        var agent = CreateAgent(plan, [handler]);

        var result = await agent.RunAsync(
            "Perform the task.",
            AgentRunContext.Create(),
            new AgentRunOptions { MaxSteps = 1 });

        Assert.Equal(AgentRunStatus.BudgetExhausted, result.Status);
        Assert.Equal(1, result.Output?.CompletedStepCount);
        Assert.Equal(3, result.Output?.Steps.Count);
        Assert.Equal(PlanStepStatus.Skipped, result.Output?.Steps[1].Status);
        Assert.Equal(PlanStepStatus.Skipped, result.Output?.Steps[2].Status);
        Assert.Equal(1, handler.ExecutionCount);
    }

    [Fact]
    public async Task RunAsync_PropagatesCancellationAndPreservesActiveAndBlockedSteps()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var blocking = new BlockingHandler("work", started);
        var plan = new ExecutionPlan(
            "Cancel in progress.",
            [
                new PlanStep("active", "work", "Wait."),
                new PlanStep("later", "work", "Must remain blocked.")
            ]);
        var agent = CreateAgent(plan, [blocking]);
        using var cancellation = new CancellationTokenSource();

        var run = agent.RunAsync("Perform the task.", AgentRunContext.Create(), new AgentRunOptions(), cancellation.Token);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await cancellation.CancelAsync();
        var result = await run;

        Assert.Equal(AgentRunStatus.Cancelled, result.Status);
        Assert.Collection(
            result.Output!.Steps,
            active => Assert.Equal(PlanStepStatus.Cancelled, active.Status),
            later => Assert.Equal(PlanStepStatus.Blocked, later.Status));
    }

    [Fact]
    public async Task RunAsync_MapsOverallTimeoutToTimedOutAndBlocksRemainingSteps()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var blocking = new BlockingHandler("work", started);
        var plan = new ExecutionPlan(
            "Timeout in progress.",
            [
                new PlanStep("active", "work", "Wait."),
                new PlanStep("later", "work", "Must remain blocked.")
            ]);
        var agent = CreateAgent(plan, [blocking]);

        var result = await agent.RunAsync(
            "Perform the task.",
            AgentRunContext.Create(),
            new AgentRunOptions { Timeout = TimeSpan.FromMilliseconds(150) });

        Assert.Equal(AgentRunStatus.TimedOut, result.Status);
        Assert.Collection(
            result.Output!.Steps,
            active => Assert.Equal(PlanStepStatus.Cancelled, active.Status),
            later => Assert.Equal(PlanStepStatus.Blocked, later.Status));
    }

    [Fact]
    public async Task RunAsync_EmitsEventsWithoutTaskContent()
    {
        var sink = new RecordingEventSink();
        var handler = new FakeHandler("work", _ => CreateJson("""{"ok":true}"""));
        var plan = new ExecutionPlan("Safe events.", [new PlanStep("first", "work", "Do work.")]);
        var agent = CreateAgent(plan, [handler]);
        const string privateTask = "private customer instruction";

        await agent.RunAsync(
            privateTask,
            AgentRunContext.Create(),
            new AgentRunOptions { EventSink = sink });

        var names = sink.Events.Select(agentEvent => agentEvent.Name).ToArray();
        Assert.Contains("plan.generation.started", names);
        Assert.Contains("plan.validation.passed", names);
        Assert.Contains("plan.step.started", names);
        Assert.Contains("plan.step.completed", names);
        Assert.Contains("plan.completed", names);
        Assert.DoesNotContain(
            sink.Events.SelectMany(agentEvent => agentEvent.Attributes.Values),
            value => value.Contains(privateTask, StringComparison.Ordinal));
    }

    private static PlanAndExecuteAgent CreateAgent(
        ExecutionPlan plan,
        IEnumerable<IPlanStepHandler> handlers,
        PlanAndExecuteAgentOptions? options = null)
    {
        return new PlanAndExecuteAgent(
            new FixedPlanningStrategy(plan),
            new PlanStepHandlerRegistry(handlers),
            options);
    }

    private static JsonElement CreateJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    private sealed class FixedPlanningStrategy(ExecutionPlan plan) : IPlanningStrategy
    {
        public PlanningRequest? LastRequest { get; private set; }

        public Task<ExecutionPlan> GenerateAsync(
            PlanningRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastRequest = request;
            return Task.FromResult(plan);
        }
    }

    private sealed class FakeHandler(
        string stepType,
        Func<PlanStepExecutionContext, JsonElement> execute) : IPlanStepHandler
    {
        public string StepType => stepType;

        public int ExecutionCount { get; private set; }

        public Task<JsonElement> ExecuteAsync(
            PlanStepExecutionContext context,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ExecutionCount++;
            return Task.FromResult(execute(context));
        }
    }

    private sealed class BlockingHandler(
        string stepType,
        TaskCompletionSource started) : IPlanStepHandler
    {
        public string StepType => stepType;

        public async Task<JsonElement> ExecuteAsync(
            PlanStepExecutionContext context,
            CancellationToken cancellationToken = default)
        {
            started.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return CreateJson("""{"unreachable":true}""");
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
