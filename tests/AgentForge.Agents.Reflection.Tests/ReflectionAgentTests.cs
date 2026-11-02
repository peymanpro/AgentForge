using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Reflection;
using AgentForge.Agents.Reflection.Evaluation;
using AgentForge.Agents.Reflection.Generation;
using Xunit;

namespace AgentForge.Agents.Reflection.Tests;

public sealed class ReflectionAgentTests
{
    [Fact]
    public async Task RunAsync_CompletesWhenFirstCandidateIsAccepted()
    {
        var generator = new ScriptedGenerator("accepted candidate");
        var evaluator = new ScriptedEvaluator(new ReflectionEvaluation(true, "Meets the criteria.", 0.95));
        var agent = CreateAgent(generator, evaluator);

        var result = await agent.RunAsync("Complete the task.", AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.True(result.Output?.IsAccepted);
        Assert.Equal("accepted candidate", result.Output?.BestCandidate);
        Assert.Equal(0.95, result.Output?.BestScore);
        Assert.Single(generator.Requests);
        Assert.Single(evaluator.Requests);
    }

    [Fact]
    public async Task RunAsync_PassesPreviousCandidateAndFeedbackIntoNextAttempt()
    {
        var generator = new ScriptedGenerator("first draft", "improved draft");
        var evaluator = new ScriptedEvaluator(
            new ReflectionEvaluation(false, "Add the missing constraints.", 0.4),
            new ReflectionEvaluation(true, "All acceptance criteria are met.", 0.9));
        var agent = CreateAgent(generator, evaluator);

        var result = await agent.RunAsync("Write the specification.", AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.Equal("improved draft", result.Output?.BestCandidate);
        Assert.Equal(2, result.Output?.AttemptCount);
        Assert.Null(generator.Requests[0].PreviousCandidate);
        Assert.Null(generator.Requests[0].Feedback);
        Assert.Equal("first draft", generator.Requests[1].PreviousCandidate);
        Assert.Equal("Add the missing constraints.", generator.Requests[1].Feedback);
    }

    [Fact]
    public async Task RunAsync_PreservesBestScoredCandidateAtAttemptLimit()
    {
        var generator = new ScriptedGenerator("high quality", "lower quality", "medium quality");
        var evaluator = new ScriptedEvaluator(
            new ReflectionEvaluation(false, "Refine terminology.", 0.9),
            new ReflectionEvaluation(false, "Try a different structure.", 0.5),
            new ReflectionEvaluation(false, "Clarify the conclusion.", 0.7));
        var agent = CreateAgent(generator, evaluator);

        var result = await agent.RunAsync(
            "Improve the report.",
            AgentRunContext.Create(),
            new AgentRunOptions());

        Assert.Equal(AgentRunStatus.BudgetExhausted, result.Status);
        Assert.Equal("reflection.attempt_limit", result.Failure?.Code);
        Assert.Equal("high quality", result.Output?.BestCandidate);
        Assert.Equal(0.9, result.Output?.BestScore);
        Assert.Equal(3, result.Output?.AttemptCount);
        Assert.False(result.Output?.IsAccepted);
    }

    [Fact]
    public async Task RunAsync_UsesLatestCandidateWhenEvaluatorDoesNotProvideScores()
    {
        var generator = new ScriptedGenerator("draft one", "draft two", "draft three");
        var evaluator = new ScriptedEvaluator(
            new ReflectionEvaluation(false, "Add examples."),
            new ReflectionEvaluation(false, "Tighten the explanation."),
            new ReflectionEvaluation(false, "Clarify the final section."));
        var agent = CreateAgent(generator, evaluator);

        var result = await agent.RunAsync("Improve the answer.", AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.BudgetExhausted, result.Status);
        Assert.Equal("draft three", result.Output?.BestCandidate);
        Assert.Equal(3, result.Output?.AttemptCount);
        Assert.Null(result.Output?.BestScore);
    }

    [Fact]
    public async Task RunAsync_CanStopWhenScoreNoLongerImproves()
    {
        var generator = new ScriptedGenerator("first", "second", "third");
        var evaluator = new ScriptedEvaluator(
            new ReflectionEvaluation(false, "Improve accuracy.", 0.8),
            new ReflectionEvaluation(false, "No material improvement.", 0.8),
            new ReflectionEvaluation(false, "This response should not be generated.", 0.9));
        var agent = CreateAgent(
            generator,
            evaluator,
            new ReflectionAgentOptions(maxAttempts: 3, stopOnNoImprovement: true));

        var result = await agent.RunAsync("Improve the result.", AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.PartiallyCompleted, result.Status);
        Assert.Equal("reflection.no_improvement", result.Failure?.Code);
        Assert.Equal("first", result.Output?.BestCandidate);
        Assert.Equal(2, result.Output?.AttemptCount);
        Assert.Equal(2, generator.Requests.Count);
    }

    [Fact]
    public async Task RunAsync_PreservesCandidateWhenEvaluatorThrows()
    {
        var generator = new ScriptedGenerator("candidate");
        var evaluator = new ScriptedEvaluator(throwOnEvaluation: true);
        var agent = CreateAgent(generator, evaluator);

        var result = await agent.RunAsync("Perform the task.", AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.PartiallyCompleted, result.Status);
        Assert.Equal(AgentFailureCategory.EvaluationFailure, result.Failure?.Category);
        Assert.Equal("candidate", result.Output?.BestCandidate);
        Assert.Equal("reflection.evaluation_failed", result.Output?.Attempts[0].Failure?.Code);
        Assert.Null(result.Output?.Attempts[0].Evaluation);
        Assert.DoesNotContain("private evaluator endpoint", result.Failure?.Message ?? string.Empty);
    }

    [Fact]
    public async Task RunAsync_PreservesPreviousBestWhenNextGenerationFails()
    {
        var generator = new ScriptedGenerator("first candidate", throwAfterResponses: true);
        var evaluator = new ScriptedEvaluator(new ReflectionEvaluation(false, "Try again.", 0.75));
        var agent = CreateAgent(generator, evaluator);

        var result = await agent.RunAsync("Perform the task.", AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.PartiallyCompleted, result.Status);
        Assert.Equal("first candidate", result.Output?.BestCandidate);
        Assert.Equal(1, result.Output?.AttemptCount);
        Assert.Equal("reflection.generation_failed", result.Failure?.Code);
    }

    [Fact]
    public async Task RunAsync_ReturnsFailedWhenGeneratorProducesNoCandidate()
    {
        var generator = new ScriptedGenerator(" ");
        var evaluator = new ScriptedEvaluator();
        var agent = CreateAgent(generator, evaluator);

        var result = await agent.RunAsync("Perform the task.", AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.Failed, result.Status);
        Assert.Equal("reflection.empty_candidate", result.Failure?.Code);
        Assert.Empty(evaluator.Requests);
    }

    [Fact]
    public async Task RunAsync_MapsCallerCancellationAndPreservesGeneratedCandidate()
    {
        var generator = new ScriptedGenerator("candidate");
        var evaluator = new BlockingEvaluator();
        var agent = CreateAgent(generator, evaluator);
        using var cancellation = new CancellationTokenSource();

        var run = agent.RunAsync(
            "Perform the task.",
            AgentRunContext.Create(),
            new AgentRunOptions(),
            cancellation.Token);

        await evaluator.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
        await cancellation.CancelAsync();
        var result = await run;

        Assert.Equal(AgentRunStatus.Cancelled, result.Status);
        Assert.Equal("candidate", result.Output?.BestCandidate);
        Assert.Single(result.Output!.Attempts);
        Assert.Null(result.Output.Attempts[0].Evaluation);
    }

    [Fact]
    public async Task RunAsync_MapsTimeoutToTimedOutStatus()
    {
        var generator = new ScriptedGenerator("candidate");
        var evaluator = new BlockingEvaluator();
        var agent = CreateAgent(
            generator,
            evaluator,
            new ReflectionAgentOptions(timeout: TimeSpan.FromMilliseconds(150)));

        var result = await agent.RunAsync("Perform the task.", AgentRunContext.Create(), new AgentRunOptions());

        Assert.Equal(AgentRunStatus.TimedOut, result.Status);
        Assert.Equal("candidate", result.Output?.BestCandidate);
    }

    [Fact]
    public async Task RunAsync_EmitsStructuredEventsWithoutTaskOrCandidateContent()
    {
        var sink = new RecordingEventSink();
        const string privateTask = "private task content";
        const string privateCandidate = "private candidate output";
        var generator = new ScriptedGenerator(privateCandidate);
        var evaluator = new ScriptedEvaluator(new ReflectionEvaluation(true, "Accepted.", 1));
        var agent = CreateAgent(generator, evaluator);

        await agent.RunAsync(
            privateTask,
            AgentRunContext.Create(),
            new AgentRunOptions { EventSink = sink });

        var names = sink.Events.Select(agentEvent => agentEvent.Name).ToArray();
        Assert.Contains("reflection.run.started", names);
        Assert.Contains("reflection.candidate.generated", names);
        Assert.Contains("reflection.candidate.evaluated", names);
        Assert.Contains("reflection.accepted", names);

        var attributeValues = sink.Events.SelectMany(agentEvent => agentEvent.Attributes.Values);
        Assert.DoesNotContain(attributeValues, value => value.Contains(privateTask, StringComparison.Ordinal));
        Assert.DoesNotContain(attributeValues, value => value.Contains(privateCandidate, StringComparison.Ordinal));
    }

    private static ReflectionAgent CreateAgent(
        ScriptedGenerator generator,
        IReflectionEvaluator evaluator,
        ReflectionAgentOptions? options = null)
    {
        return new ReflectionAgent(generator, evaluator, options);
    }

    private sealed class ScriptedGenerator(
        params string[] candidates) : IReflectionGenerationStrategy
    {
        private readonly Queue<string> _candidates = new(candidates);
        private readonly bool _throwAfterResponses = false;

        public ScriptedGenerator(string candidate, bool throwAfterResponses)
            : this(candidate)
        {
            _throwAfterResponses = throwAfterResponses;
        }

        public List<ReflectionGenerationRequest> Requests { get; } = [];

        public Task<string> GenerateAsync(
            ReflectionGenerationRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests.Add(request);

            if (_candidates.Count == 0 || (_throwAfterResponses && _candidates.Count == 0))
            {
                throw new InvalidOperationException("private generation endpoint");
            }

            return Task.FromResult(_candidates.Dequeue());
        }
    }

    private sealed class ScriptedEvaluator : IReflectionEvaluator
    {
        private readonly Queue<ReflectionEvaluation> _evaluations;
        private readonly bool _throwOnEvaluation;

        public ScriptedEvaluator(params ReflectionEvaluation[] evaluations)
        {
            _evaluations = new Queue<ReflectionEvaluation>(evaluations);
        }

        public ScriptedEvaluator(bool throwOnEvaluation)
        {
            _evaluations = new Queue<ReflectionEvaluation>();
            _throwOnEvaluation = throwOnEvaluation;
        }

        public List<ReflectionEvaluationRequest> Requests { get; } = [];

        public Task<ReflectionEvaluation> EvaluateAsync(
            ReflectionEvaluationRequest request,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Requests.Add(request);

            if (_throwOnEvaluation)
            {
                throw new InvalidOperationException("private evaluator endpoint");
            }

            if (_evaluations.Count == 0)
            {
                throw new InvalidOperationException("No scripted evaluation remains.");
            }

            return Task.FromResult(_evaluations.Dequeue());
        }
    }

    private sealed class BlockingEvaluator : IReflectionEvaluator
    {
        public TaskCompletionSource Started { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task<ReflectionEvaluation> EvaluateAsync(
            ReflectionEvaluationRequest request,
            CancellationToken cancellationToken = default)
        {
            Started.TrySetResult();
            await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
            return new ReflectionEvaluation(true, "Unreachable.");
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
