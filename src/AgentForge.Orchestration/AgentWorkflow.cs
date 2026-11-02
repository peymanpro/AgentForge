using System.Globalization;
using AgentForge.Abstractions.Execution;

namespace AgentForge.Orchestration;

/// <summary>
/// Executes a validated, in-memory workflow sequentially with explicit routing and bounded execution.
/// </summary>
/// <remarks>
/// The workflow runtime is intentionally not a distributed scheduler. It does not provide persistence,
/// automatic retries, parallel execution, loop constructs, or transactional rollback.
/// </remarks>
public sealed class AgentWorkflow<TWorkflowInput> : IAgent<TWorkflowInput, WorkflowRunOutput>
{
    private readonly WorkflowDefinition<TWorkflowInput> _definition;
    private readonly WorkflowOptions _workflowOptions;
    private readonly TimeProvider _timeProvider;

    internal AgentWorkflow(
        WorkflowDefinition<TWorkflowInput> definition,
        WorkflowOptions workflowOptions,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(workflowOptions);

        _definition = definition;
        _workflowOptions = workflowOptions;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>Gets the stable workflow identifier.</summary>
    public string WorkflowId => _definition.WorkflowId;

    /// <inheritdoc />
    public async Task<AgentRunResult<WorkflowRunOutput>> RunAsync(
        TWorkflowInput input,
        AgentRunContext context,
        AgentRunOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(options);

        var startedAt = _timeProvider.GetUtcNow();
        var startTimestamp = _timeProvider.GetTimestamp();

        try
        {
            options.Validate();
        }
        catch (ArgumentOutOfRangeException)
        {
            return Rejected("workflow.invalid_run_options", "One or more execution limits are invalid.", startedAt);
        }

        var overallTimeout = options.Timeout ?? _workflowOptions.Timeout;
        if (overallTimeout > TimeSpan.FromMilliseconds(4_294_967_294d))
        {
            return Rejected("workflow.timeout_out_of_range", "The configured timeout exceeds the supported timer range.", startedAt);
        }

        var maxSteps = options.MaxSteps ?? _workflowOptions.MaxSteps;
        using var overallTimeoutSource = new CancellationTokenSource(overallTimeout);
        using var workflowSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            overallTimeoutSource.Token);
        var workflowToken = workflowSource.Token;

        var stepResults = new List<WorkflowStepResult>();
        var outputs = new Dictionary<string, object?>(StringComparer.Ordinal);
        var conditionResults = new Dictionary<string, bool>(StringComparer.Ordinal);
        var executedSteps = 0;
        string? activeStepId = null;
        DateTimeOffset? activeStepStartedAt = null;

        try
        {
            await PublishEventAsync(options, context, "workflow.started", new Dictionary<string, string>
            {
                ["workflow_id"] = WorkflowId,
                ["step_count"] = _definition.Steps.Count.ToString(CultureInfo.InvariantCulture)
            }, workflowToken);

            for (var index = 0; index < _definition.Steps.Count; index++)
            {
                workflowToken.ThrowIfCancellationRequested();

                var step = _definition.Steps[index];

                if (step.RunWhen is { } route)
                {
                    if (!_definition.Conditions.TryGetValue(route.ConditionId, out var condition))
                    {
                        var failure = new AgentFailure(
                            AgentFailureCategory.ValidationFailure,
                            "workflow.condition_missing",
                            "A declared workflow condition was not found.");

                        stepResults.Add(CreateResult(step.Id, WorkflowStepStatus.Blocked, failure));
                        AddBlockedSteps(index + 1, stepResults);

                        return await StopAsync(
                            options,
                            context,
                            startedAt,
                            AgentRunStatus.Failed,
                            failure.Category,
                            failure.Code,
                            failure.Message,
                            stepResults,
                            conditionResults);
                    }

                    if (!conditionResults.TryGetValue(route.ConditionId, out var conditionValue))
                    {
                        var conditionContextResult = TryCreateContext(input, context, condition.InputReferences, outputs);
                        if (conditionContextResult.Context is null)
                        {
                            var failure = new AgentFailure(
                                AgentFailureCategory.ValidationFailure,
                                "workflow.condition_input_unavailable",
                                conditionContextResult.ErrorMessage ?? "A condition input is unavailable.");

                            stepResults.Add(CreateResult(step.Id, WorkflowStepStatus.Blocked, failure));
                            AddBlockedSteps(index + 1, stepResults);

                            return await StopAsync(
                                options,
                                context,
                                startedAt,
                                AgentRunStatus.PartiallyCompleted,
                                failure.Category,
                                failure.Code,
                                failure.Message,
                                stepResults,
                                conditionResults);
                        }

                        workflowToken.ThrowIfCancellationRequested();

                        try
                        {
                            conditionValue = await condition.Evaluator(
                                conditionContextResult.Context,
                                workflowToken).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException) when (workflowToken.IsCancellationRequested)
                        {
                            throw;
                        }
                        catch (Exception)
                        {
                            var failure = new AgentFailure(
                                AgentFailureCategory.InternalFailure,
                                "workflow.condition_failed",
                                "A workflow condition evaluator failed.");

                            stepResults.Add(CreateResult(step.Id, WorkflowStepStatus.Failed, failure));
                            AddBlockedSteps(index + 1, stepResults);

                            return await StopAsync(
                                options,
                                context,
                                startedAt,
                                AgentRunStatus.PartiallyCompleted,
                                failure.Category,
                                failure.Code,
                                failure.Message,
                                stepResults,
                                conditionResults);
                        }

                        conditionResults.Add(route.ConditionId, conditionValue);
                        await PublishEventAsync(options, context, "workflow.condition.evaluated", new Dictionary<string, string>
                        {
                            ["condition_id"] = route.ConditionId,
                            ["value"] = conditionValue.ToString().ToLowerInvariant(),
                            ["controlling_step"] = step.Id
                        }, workflowToken);
                    }

                    if (conditionValue != route.ExpectedValue)
                    {
                        stepResults.Add(CreateSkippedResult(step.Id));
                        await PublishEventAsync(options, context, "workflow.step.skipped", new Dictionary<string, string>
                        {
                            ["step_id"] = step.Id,
                            ["condition_id"] = route.ConditionId
                        }, workflowToken);
                        continue;
                    }
                }

                var stepContextResult = TryCreateContext(input, context, step.InputReferences, outputs);
                if (stepContextResult.Context is null)
                {
                    var failure = new AgentFailure(
                        AgentFailureCategory.ValidationFailure,
                        "workflow.input_unavailable",
                        stepContextResult.ErrorMessage ?? "A required workflow input is unavailable.");

                    stepResults.Add(CreateResult(step.Id, WorkflowStepStatus.Blocked, failure));
                    AddBlockedSteps(index + 1, stepResults);

                    return await StopAsync(
                        options,
                        context,
                        startedAt,
                        AgentRunStatus.PartiallyCompleted,
                        failure.Category,
                        failure.Code,
                        failure.Message,
                        stepResults,
                        conditionResults);
                }

                if (executedSteps >= maxSteps)
                {
                    var failure = new AgentFailure(
                        AgentFailureCategory.BudgetExhausted,
                        "workflow.step_budget_exhausted",
                        "The workflow reached its configured executed-step limit.");

                    stepResults.Add(CreateResult(step.Id, WorkflowStepStatus.BudgetExhausted, failure));
                    AddBlockedSteps(index + 1, stepResults);
                    return await StopAsync(
                        options,
                        context,
                        startedAt,
                        AgentRunStatus.BudgetExhausted,
                        failure.Category,
                        failure.Code,
                        failure.Message,
                        stepResults,
                        conditionResults);
                }

                var elapsed = _timeProvider.GetElapsedTime(startTimestamp);
                var remaining = overallTimeout - elapsed;
                if (remaining <= TimeSpan.Zero)
                {
                    throw new OperationCanceledException(workflowToken);
                }

                var stepTimeout = remaining < _workflowOptions.StepTimeout
                    ? remaining
                    : _workflowOptions.StepTimeout;

                using var stepTimeoutSource = new CancellationTokenSource(stepTimeout);
                using var stepSource = CancellationTokenSource.CreateLinkedTokenSource(
                    workflowToken,
                    stepTimeoutSource.Token);
                var stepToken = stepSource.Token;
                var stepStartedAt = _timeProvider.GetUtcNow();

                activeStepId = step.Id;
                activeStepStartedAt = stepStartedAt;
                executedSteps++;

                await PublishEventAsync(options, context, "workflow.step.started", new Dictionary<string, string>
                {
                    ["step_id"] = step.Id,
                    ["executed_step"] = executedSteps.ToString(CultureInfo.InvariantCulture)
                }, workflowToken);

                WorkflowStepExecutionOutcome outcome;
                try
                {
                    var childOptions = new AgentRunOptions
                    {
                        MaxModelRequests = options.MaxModelRequests,
                        MaxToolInvocations = options.MaxToolInvocations,
                        Timeout = stepTimeout,
                        EventSink = options.EventSink
                    };

                    outcome = await step.ExecuteAsync(
                        stepContextResult.Context,
                        childOptions,
                        stepTimeout,
                        _timeProvider,
                        stepToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (workflowToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (OperationCanceledException) when (stepTimeoutSource.IsCancellationRequested)
                {
                    outcome = new WorkflowStepExecutionOutcome(
                        WorkflowStepStatus.TimedOut,
                        null,
                        new AgentFailure(
                            AgentFailureCategory.TimedOut,
                            "workflow.step_timed_out",
                            "The workflow step exceeded its configured time limit."),
                        null);
                }
                catch (Exception)
                {
                    outcome = new WorkflowStepExecutionOutcome(
                        WorkflowStepStatus.Failed,
                        null,
                        new AgentFailure(
                            AgentFailureCategory.InternalFailure,
                            "workflow.step_failed",
                            "A workflow step operation failed."),
                        null);
                }

                if (stepTimeoutSource.IsCancellationRequested && !workflowToken.IsCancellationRequested)
                {
                    outcome = new WorkflowStepExecutionOutcome(
                        WorkflowStepStatus.TimedOut,
                        outcome.Output,
                        new AgentFailure(
                            AgentFailureCategory.TimedOut,
                            "workflow.step_timed_out",
                            "The workflow step exceeded its configured time limit."),
                        outcome.ChildRunId);
                }

                var stepCompletedAt = _timeProvider.GetUtcNow();
                stepResults.Add(new WorkflowStepResult(
                    step.Id,
                    outcome.Status,
                    stepStartedAt,
                    stepCompletedAt,
                    outcome.Output,
                    outcome.Failure,
                    outcome.ChildRunId));

                activeStepId = null;
                activeStepStartedAt = null;

                if (outcome.Status == WorkflowStepStatus.Completed)
                {
                    outputs.Add(step.Id, outcome.Output);
                    await PublishEventAsync(options, context, "workflow.step.completed", new Dictionary<string, string>
                    {
                        ["step_id"] = step.Id
                    }, workflowToken);
                    continue;
                }

                await PublishEventAsync(options, context, "workflow.step.failed", new Dictionary<string, string>
                {
                    ["step_id"] = step.Id,
                    ["status"] = outcome.Status.ToString(),
                    ["failure_code"] = outcome.Failure?.Code ?? "workflow.step_failed"
                }, CancellationToken.None);

                AddBlockedSteps(index + 1, stepResults);

                var partial = stepResults.Any(static item =>
                    item.Status is WorkflowStepStatus.Completed or WorkflowStepStatus.PartiallyCompleted);

                var terminalStatus = MapTerminalStatus(outcome.Status, partial);
                return await StopAsync(
                    options,
                    context,
                    startedAt,
                    terminalStatus,
                    outcome.Failure?.Category ?? AgentFailureCategory.InternalFailure,
                    outcome.Failure?.Code ?? "workflow.step_failed",
                    outcome.Failure?.Message ?? "A workflow step did not complete.",
                    stepResults,
                    conditionResults);
            }

            await PublishTerminalEventAsync(options, context, "workflow.completed", stepResults.Count);

            return AgentRunResult<WorkflowRunOutput>.Completed(
                new WorkflowRunOutput(WorkflowId, stepResults, conditionResults),
                startedAt,
                _timeProvider.GetUtcNow());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            AddActiveAndBlocked(activeStepId, activeStepStartedAt, WorkflowStepStatus.Cancelled,
                AgentFailureCategory.Cancelled, "workflow.cancelled", "The workflow step was cancelled.", stepResults);

            await PublishTerminalEventAsync(options, context, "workflow.cancelled", stepResults.Count);

            return AgentRunResult<WorkflowRunOutput>.Stopped(
                AgentRunStatus.Cancelled,
                new AgentFailure(AgentFailureCategory.Cancelled, "workflow.cancelled", "The caller cancelled the workflow."),
                startedAt,
                _timeProvider.GetUtcNow(),
                new WorkflowRunOutput(WorkflowId, stepResults, conditionResults));
        }
        catch (OperationCanceledException) when (overallTimeoutSource.IsCancellationRequested)
        {
            AddActiveAndBlocked(activeStepId, activeStepStartedAt, WorkflowStepStatus.TimedOut,
                AgentFailureCategory.TimedOut, "workflow.timed_out", "The workflow exceeded its configured time limit.", stepResults);

            await PublishTerminalEventAsync(options, context, "workflow.timed_out", stepResults.Count);

            return AgentRunResult<WorkflowRunOutput>.Stopped(
                AgentRunStatus.TimedOut,
                new AgentFailure(AgentFailureCategory.TimedOut, "workflow.timed_out", "The workflow exceeded its configured time limit."),
                startedAt,
                _timeProvider.GetUtcNow(),
                new WorkflowRunOutput(WorkflowId, stepResults, conditionResults));
        }
    }

    private (WorkflowStepExecutionContext<TWorkflowInput>? Context, string? ErrorMessage) TryCreateContext(
        TWorkflowInput input,
        AgentRunContext context,
        IReadOnlyList<string> references,
        IReadOnlyDictionary<string, object?> outputs)
    {
        var referencedOutputs = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var reference in references)
        {
            if (!outputs.TryGetValue(reference, out var output))
            {
                return (null, $"Required output '{reference}' is unavailable because its step did not complete.");
            }

            referencedOutputs.Add(reference, output);
        }

        return (new WorkflowStepExecutionContext<TWorkflowInput>(
            input,
            context,
            referencedOutputs), null);
    }

    private WorkflowStepResult CreateResult(
        string stepId,
        WorkflowStepStatus status,
        AgentFailure failure)
    {
        var now = _timeProvider.GetUtcNow();
        return new WorkflowStepResult(stepId, status, now, now, failure: failure);
    }

    private WorkflowStepResult CreateSkippedResult(string stepId)
    {
        var now = _timeProvider.GetUtcNow();
        return new WorkflowStepResult(stepId, WorkflowStepStatus.Skipped, now, now);
    }

    private void AddBlockedSteps(int startIndex, List<WorkflowStepResult> results)
    {
        for (var index = startIndex; index < _definition.Steps.Count; index++)
        {
            var step = _definition.Steps[index];
            if (results.Any(result => string.Equals(result.StepId, step.Id, StringComparison.Ordinal)))
            {
                continue;
            }

            var failure = new AgentFailure(
                AgentFailureCategory.InternalFailure,
                "workflow.step_blocked",
                "The step was not started because an earlier workflow step stopped execution.");

            results.Add(CreateResult(step.Id, WorkflowStepStatus.Blocked, failure));
        }
    }

    private void AddActiveAndBlocked(
        string? activeStepId,
        DateTimeOffset? activeStartedAt,
        WorkflowStepStatus activeStatus,
        AgentFailureCategory category,
        string code,
        string message,
        List<WorkflowStepResult> results)
    {
        if (activeStepId is not null && activeStartedAt is { } startedAt)
        {
            results.Add(new WorkflowStepResult(
                activeStepId,
                activeStatus,
                startedAt,
                _timeProvider.GetUtcNow(),
                failure: new AgentFailure(category, code, message)));
        }

        var nextIndex = activeStepId is null
            ? results.Count
            : _definition.Steps.ToList().FindIndex(step => string.Equals(step.Id, activeStepId, StringComparison.Ordinal)) + 1;

        AddBlockedSteps(Math.Max(nextIndex, 0), results);
    }

    private static AgentRunStatus MapTerminalStatus(WorkflowStepStatus status, bool hasPartialOutput)
    {
        return status switch
        {
            WorkflowStepStatus.Cancelled => AgentRunStatus.Cancelled,
            WorkflowStepStatus.TimedOut => AgentRunStatus.TimedOut,
            WorkflowStepStatus.BudgetExhausted => AgentRunStatus.BudgetExhausted,
            WorkflowStepStatus.Rejected when !hasPartialOutput => AgentRunStatus.Rejected,
            _ when hasPartialOutput => AgentRunStatus.PartiallyCompleted,
            _ => AgentRunStatus.Failed
        };
    }

    private AgentRunResult<WorkflowRunOutput> Rejected(
        string code,
        string message,
        DateTimeOffset startedAt)
    {
        return AgentRunResult<WorkflowRunOutput>.Stopped(
            AgentRunStatus.Rejected,
            new AgentFailure(AgentFailureCategory.InvalidInput, code, message),
            startedAt,
            _timeProvider.GetUtcNow());
    }

    private async Task<AgentRunResult<WorkflowRunOutput>> StopAsync(
        AgentRunOptions options,
        AgentRunContext context,
        DateTimeOffset startedAt,
        AgentRunStatus status,
        AgentFailureCategory category,
        string code,
        string message,
        IReadOnlyList<WorkflowStepResult> stepResults,
        IReadOnlyDictionary<string, bool> conditionResults)
    {
        var output = new WorkflowRunOutput(WorkflowId, stepResults, conditionResults);
        await PublishTerminalEventAsync(options, context, "workflow.stopped", stepResults.Count);

        var failure = new AgentFailure(category, code, message);
        var hasPartialOutput = stepResults.Any(static item =>
            item.Status is WorkflowStepStatus.Completed or WorkflowStepStatus.PartiallyCompleted);

        if (status == AgentRunStatus.PartiallyCompleted && hasPartialOutput)
        {
            return AgentRunResult<WorkflowRunOutput>.PartiallyCompleted(
                output,
                failure,
                startedAt,
                _timeProvider.GetUtcNow());
        }

        var terminalStatus = status == AgentRunStatus.PartiallyCompleted
            ? AgentRunStatus.Failed
            : status;

        return AgentRunResult<WorkflowRunOutput>.Stopped(
            terminalStatus,
            failure,
            startedAt,
            _timeProvider.GetUtcNow(),
            output);
    }

    private async ValueTask PublishTerminalEventAsync(
        AgentRunOptions options,
        AgentRunContext context,
        string name,
        int resultCount)
    {
        await PublishEventAsync(options, context, name, new Dictionary<string, string>
        {
            ["workflow_id"] = WorkflowId,
            ["step_results"] = resultCount.ToString(CultureInfo.InvariantCulture)
        }, CancellationToken.None);
    }

    private ValueTask PublishEventAsync(
        AgentRunOptions options,
        AgentRunContext context,
        string name,
        IReadOnlyDictionary<string, string>? attributes,
        CancellationToken cancellationToken)
    {
        return AgentRunEventPublisher.PublishAsync(
            options,
            context,
            name,
            attributes,
            _timeProvider,
            cancellationToken);
    }
}
