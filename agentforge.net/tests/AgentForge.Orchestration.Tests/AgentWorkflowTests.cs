using AgentForge.Abstractions.Execution;
using AgentForge.Orchestration;
using Xunit;

namespace AgentForge.Orchestration.Tests;

public sealed class AgentWorkflowTests
{
    [Fact]
    public async Task RunAsync_ExecutesSequentialStepsAndMapsDeclaredOutputs()
    {
        var workflow = AgentWorkflow.Create<string>("sequential")
            .AddStep("normalize", (context, _) => Task.FromResult(context.WorkflowInput.Trim().ToUpperInvariant()))
            .AddStep(
                "compose",
                (context, _) => Task.FromResult($"processed:{context.GetOutput<string>("normalize")}"),
                ["normalize"])
            .Build();

        var result = await workflow.RunAsync(
            "  hello world  ",
            AgentRunContext.Create(),
            new AgentRunOptions());

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.Equal("processed:HELLO WORLD", result.Output?.GetOutput<string>("compose"));
        Assert.Equal(2, result.Output?.CompletedStepCount);
        Assert.Equal(
            new[] { "normalize", "compose" },
            result.Output?.Steps.Select(step => step.StepId));
    }

    [Fact]
    public async Task RunAsync_ExposesOnlyExplicitlyMappedStepOutputs()
    {
        var workflow = AgentWorkflow.Create<string>("output-isolation")
            .AddStep("first", (_, _) => Task.FromResult(42))
            .AddStep(
                "second",
                (context, _) =>
                {
                    Assert.Equal(42, context.GetOutput<int>("first"));
                    Assert.Throws<KeyNotFoundException>(() => context.GetOutput<int>("not-mapped"));
                    return Task.FromResult("isolated");
                },
                ["first"])
            .Build();

        var result = await workflow.RunAsync(
            "input",
            AgentRunContext.Create(),
            new AgentRunOptions());

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.Equal("isolated", result.Output?.GetOutput<string>("second"));
    }

    [Fact]
    public async Task RunAsync_EvaluatesConditionOnceAndExecutesOnlySelectedBranch()
    {
        var conditionCalls = 0;
        var workflow = AgentWorkflow.Create<string>("conditional")
            .AddStep("review", (_, _) => Task.FromResult(new ReviewResult(true)))
            .AddCondition(
                "needs_revision",
                context =>
                {
                    conditionCalls++;
                    return context.GetOutput<ReviewResult>("review").NeedsRevision;
                },
                ["review"])
            .AddStep(
                "revise",
                (context, _) => Task.FromResult($"revised:{context.GetOutput<ReviewResult>("review").NeedsRevision}"),
                ["review"],
                WorkflowStepCondition.WhenTrue("needs_revision"))
            .AddStep(
                "accept",
                (_, _) => Task.FromResult("accepted"),
                runWhen: WorkflowStepCondition.WhenFalse("needs_revision"))
            .Build();

        var result = await workflow.RunAsync(
            "write report",
            AgentRunContext.Create(),
            new AgentRunOptions());

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.Equal(1, conditionCalls);
        Assert.True(result.Output?.ConditionResults["needs_revision"]);
        Assert.Equal(WorkflowStepStatus.Completed, result.Output?.Steps[1].Status);
        Assert.Equal(WorkflowStepStatus.Skipped, result.Output?.Steps[2].Status);
        Assert.Equal("revised:True", result.Output?.GetOutput<string>("revise"));
    }

    [Fact]
    public async Task RunAsync_SkipsFalseBranchWithoutRequiringItsStepInputs()
    {
        var workflow = AgentWorkflow.Create<string>("skip-branch")
            .AddStep("review", (_, _) => Task.FromResult(new ReviewResult(false)))
            .AddCondition(
                "needs_revision",
                context => context.GetOutput<ReviewResult>("review").NeedsRevision,
                ["review"])
            .AddStep(
                "revise",
                (context, _) => Task.FromResult(context.GetOutput<string>("unavailable")),
                ["unavailable"],
                WorkflowStepCondition.WhenTrue("needs_revision"))
            .AddStep(
                "accept",
                (context, _) => Task.FromResult($"accepted:{context.GetOutput<ReviewResult>("review").NeedsRevision}"),
                ["review"],
                WorkflowStepCondition.WhenFalse("needs_revision"))
            .Build();

        var result = await workflow.RunAsync(
            "accept report",
            AgentRunContext.Create(),
            new AgentRunOptions());

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.Equal(WorkflowStepStatus.Skipped, result.Output?.Steps[1].Status);
        Assert.Equal(WorkflowStepStatus.Completed, result.Output?.Steps[2].Status);
        Assert.Equal("accepted:False", result.Output?.GetOutput<string>("accept"));
    }

    [Fact]
    public async Task RunAsync_BlocksDownstreamStepWhenItsDeclaredOutputWasSkipped()
    {
        var workflow = AgentWorkflow.Create<string>("missing-branch-output")
            .AddStep("review", (_, _) => Task.FromResult(new ReviewResult(false)))
            .AddCondition(
                "needs_revision",
                context => context.GetOutput<ReviewResult>("review").NeedsRevision,
                ["review"])
            .AddStep(
                "revise",
                (_, _) => Task.FromResult("revised"),
                runWhen: WorkflowStepCondition.WhenTrue("needs_revision"))
            .AddStep(
                "publish",
                (context, _) => Task.FromResult(context.GetOutput<string>("revise")),
                ["revise"])
            .Build();

        var result = await workflow.RunAsync(
            "publish report",
            AgentRunContext.Create(),
            new AgentRunOptions());

        Assert.Equal(AgentRunStatus.PartiallyCompleted, result.Status);
        Assert.Equal("workflow.input_unavailable", result.Failure?.Code);
        Assert.Equal(WorkflowStepStatus.Skipped, result.Output?.Steps[1].Status);
        Assert.Equal(WorkflowStepStatus.Blocked, result.Output?.Steps[2].Status);
        Assert.Equal("review", result.Output?.Steps[0].StepId);
    }

    [Fact]
    public async Task RunAsync_StopsAfterFailureAndPreservesCompletedOutput()
    {
        var laterCalls = 0;
        var workflow = AgentWorkflow.Create<string>("stop-on-failure")
            .AddStep("first", (_, _) => Task.FromResult("preserved"))
            .AddStep<string>(
                "failure",
                (_, _) => throw new InvalidOperationException("private endpoint details"),
                ["first"])
            .AddStep(
                "later",
                (_, _) =>
                {
                    laterCalls++;
                    return Task.FromResult("must-not-run");
                })
            .Build();

        var result = await workflow.RunAsync(
            "task",
            AgentRunContext.Create(),
            new AgentRunOptions());

        Assert.Equal(AgentRunStatus.PartiallyCompleted, result.Status);
        Assert.Equal("preserved", result.Output?.GetOutput<string>("first"));
        Assert.Equal(WorkflowStepStatus.Failed, result.Output?.Steps[1].Status);
        Assert.Equal(WorkflowStepStatus.Blocked, result.Output?.Steps[2].Status);
        Assert.Equal("workflow.step_failed", result.Failure?.Code);
        Assert.DoesNotContain("private endpoint", result.Failure?.Message ?? string.Empty);
        Assert.Equal(0, laterCalls);
    }

    [Fact]
    public async Task RunAsync_StopsWhenConditionEvaluatorThrows()
    {
        var laterCalls = 0;
        var workflow = AgentWorkflow.Create<string>("condition-failure")
            .AddStep("review", (_, _) => Task.FromResult("reviewed"))
            .AddCondition(
                "invalid-condition",
                _ => throw new InvalidOperationException("private condition implementation"),
                ["review"])
            .AddStep(
                "conditional",
                (_, _) => Task.FromResult("unreachable"),
                runWhen: WorkflowStepCondition.WhenTrue("invalid-condition"))
            .AddStep(
                "later",
                (_, _) =>
                {
                    laterCalls++;
                    return Task.FromResult("unreachable");
                })
            .Build();

        var result = await workflow.RunAsync(
            "task",
            AgentRunContext.Create(),
            new AgentRunOptions());

        Assert.Equal(AgentRunStatus.PartiallyCompleted, result.Status);
        Assert.Equal("workflow.condition_failed", result.Failure?.Code);
        Assert.Equal(WorkflowStepStatus.Failed, result.Output?.Steps[1].Status);
        Assert.Equal(WorkflowStepStatus.Blocked, result.Output?.Steps[2].Status);
        Assert.Equal(0, laterCalls);
    }

    [Fact]
    public async Task RunAsync_EnforcesMaximumExecutedStepBudget()
    {
        var workflow = AgentWorkflow.Create<string>(
                "step-budget",
                new WorkflowOptions(maxSteps: 1))
            .AddStep("first", (_, _) => Task.FromResult("done"))
            .AddStep("second", (_, _) => Task.FromResult("not-run"))
            .Build();

        var result = await workflow.RunAsync(
            "task",
            AgentRunContext.Create(),
            new AgentRunOptions());

        Assert.Equal(AgentRunStatus.BudgetExhausted, result.Status);
        Assert.Equal("workflow.step_budget_exhausted", result.Failure?.Code);
        Assert.Equal(WorkflowStepStatus.Completed, result.Output?.Steps[0].Status);
        Assert.Equal(WorkflowStepStatus.BudgetExhausted, result.Output?.Steps[1].Status);
    }

    [Fact]
    public async Task RunAsync_MapsStepTimeoutAndBlocksLaterSteps()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var laterCalls = 0;
        var workflow = AgentWorkflow.Create<string>(
                "step-timeout",
                new WorkflowOptions(timeout: TimeSpan.FromSeconds(3), stepTimeout: TimeSpan.FromMilliseconds(100)))
            .AddStep(
                "slow",
                async (_, cancellationToken) =>
                {
                    started.TrySetResult();
                    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                    return "unreachable";
                })
            .AddStep(
                "later",
                (_, _) =>
                {
                    laterCalls++;
                    return Task.FromResult("unreachable");
                })
            .Build();

        var result = await workflow.RunAsync(
            "task",
            AgentRunContext.Create(),
            new AgentRunOptions());

        Assert.True(started.Task.IsCompleted);
        Assert.Equal(AgentRunStatus.TimedOut, result.Status);
        Assert.Equal(WorkflowStepStatus.TimedOut, result.Output?.Steps[0].Status);
        Assert.Equal(WorkflowStepStatus.Blocked, result.Output?.Steps[1].Status);
        Assert.Equal(0, laterCalls);
    }

    [Fact]
    public async Task RunAsync_PropagatesCallerCancellationToActiveStep()
    {
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var laterCalls = 0;
        var workflow = AgentWorkflow.Create<string>("cancel")
            .AddStep(
                "active",
                async (_, cancellationToken) =>
                {
                    started.TrySetResult();
                    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                    return "unreachable";
                })
            .AddStep(
                "later",
                (_, _) =>
                {
                    laterCalls++;
                    return Task.FromResult("unreachable");
                })
            .Build();

        using var cancellation = new CancellationTokenSource();
        var run = workflow.RunAsync(
            "task",
            AgentRunContext.Create(),
            new AgentRunOptions(),
            cancellation.Token);

        await started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await cancellation.CancelAsync();
        var result = await run;

        Assert.Equal(AgentRunStatus.Cancelled, result.Status);
        Assert.Equal(WorkflowStepStatus.Cancelled, result.Output?.Steps[0].Status);
        Assert.Equal(WorkflowStepStatus.Blocked, result.Output?.Steps[1].Status);
        Assert.Equal(0, laterCalls);
    }

    [Fact]
    public async Task RunAsync_PropagatesParentContextAndExecutionLimitsToNestedAgent()
    {
        var agent = new RecordingAgent();
        var workflow = AgentWorkflow.Create<string>("nested-agent")
            .AddAgentStep("agent", agent, context => context.WorkflowInput)
            .Build();
        var parentContext = AgentRunContext.Create();

        var result = await workflow.RunAsync(
            "child input",
            parentContext,
            new AgentRunOptions
            {
                MaxModelRequests = 4,
                MaxToolInvocations = 2
            });

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.Equal("agent:child input", result.Output?.GetOutput<string>("agent"));
        Assert.Equal(parentContext.RunId, agent.ReceivedContext?.ParentRunId);
        Assert.Equal(4, agent.ReceivedOptions?.MaxModelRequests);
        Assert.Equal(2, agent.ReceivedOptions?.MaxToolInvocations);
        Assert.NotNull(result.Output?.Steps[0].ChildRunId);
    }

    [Fact]
    public async Task RunAsync_EmitsSafeWorkflowLifecycleEvents()
    {
        const string privateInput = "sensitive workflow input";
        const string privateOutput = "sensitive operation output";
        var sink = new RecordingEventSink();
        var workflow = AgentWorkflow.Create<string>("events")
            .AddStep("first", (_, _) => Task.FromResult(privateOutput))
            .Build();

        await workflow.RunAsync(
            privateInput,
            AgentRunContext.Create(),
            new AgentRunOptions { EventSink = sink });

        var eventNames = sink.Events.Select(item => item.Name).ToArray();
        Assert.Contains("workflow.started", eventNames);
        Assert.Contains("workflow.step.started", eventNames);
        Assert.Contains("workflow.step.completed", eventNames);
        Assert.Contains("workflow.completed", eventNames);

        var attributes = sink.Events.SelectMany(item => item.Attributes.Values);
        Assert.DoesNotContain(attributes, value => value.Contains(privateInput, StringComparison.Ordinal));
        Assert.DoesNotContain(attributes, value => value.Contains(privateOutput, StringComparison.Ordinal));
    }

    [Fact]
    public void Build_RejectsForwardOutputReferences()
    {
        var builder = AgentWorkflow.Create<string>("forward-reference");
        Assert.Throws<ArgumentException>(() =>
            builder.AddStep(
                "first",
                (context, _) => Task.FromResult(context.GetOutput<string>("later")),
                ["later"]));
    }

    [Fact]
    public void Build_RejectsConditionDeclaredAfterControlledStep()
    {
        var builder = AgentWorkflow.Create<string>("late-condition")
            .AddStep("first", (_, _) => Task.FromResult("first"))
            .AddStep("conditional", (_, _) => Task.FromResult("second"),
                runWhen: WorkflowStepCondition.WhenTrue("late"));

        builder.AddCondition("late", _ => true);
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_RejectsEmptyWorkflow()
    {
        var builder = AgentWorkflow.Create<string>("empty");
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public async Task RunAsync_RejectsInvalidRunOptionsBeforeExecutingSteps()
    {
        var calls = 0;
        var workflow = AgentWorkflow.Create<string>("invalid-options")
            .AddStep(
                "first",
                (_, _) =>
                {
                    calls++;
                    return Task.FromResult("unused");
                })
            .Build();

        var result = await workflow.RunAsync(
            "task",
            AgentRunContext.Create(),
            new AgentRunOptions { MaxSteps = 0 });

        Assert.Equal(AgentRunStatus.Rejected, result.Status);
        Assert.Equal("workflow.invalid_run_options", result.Failure?.Code);
        Assert.Equal(0, calls);
    }

    private sealed record ReviewResult(bool NeedsRevision);

    private sealed class RecordingAgent : IAgent<string, string>
    {
        public AgentRunContext? ReceivedContext { get; private set; }

        public AgentRunOptions? ReceivedOptions { get; private set; }

        public Task<AgentRunResult<string>> RunAsync(
            string input,
            AgentRunContext context,
            AgentRunOptions options,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReceivedContext = context;
            ReceivedOptions = options;

            var now = DateTimeOffset.UtcNow;
            return Task.FromResult(AgentRunResult<string>.Completed(
                $"agent:{input}",
                now,
                now));
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
