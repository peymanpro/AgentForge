using System.Globalization;
using System.Text.Json;
using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Planning.Execution;
using AgentForge.Agents.Planning.Planning;

namespace AgentForge.Agents.Planning;

/// <summary>
/// Generates, validates, and executes a structured plan sequentially through registered step handlers.
/// </summary>
public sealed class PlanAndExecuteAgent : IAgent<string, PlanAndExecuteOutput>
{
    private readonly IPlanningStrategy _planningStrategy;
    private readonly PlanStepHandlerRegistry _handlerRegistry;
    private readonly PlanValidator _validator;
    private readonly PlanAndExecuteAgentOptions _agentOptions;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Creates a Plan-and-Execute agent.
    /// </summary>
    public PlanAndExecuteAgent(
        IPlanningStrategy planningStrategy,
        PlanStepHandlerRegistry handlerRegistry,
        PlanAndExecuteAgentOptions? options = null,
        PlanValidator? validator = null,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(planningStrategy);
        ArgumentNullException.ThrowIfNull(handlerRegistry);

        _planningStrategy = planningStrategy;
        _handlerRegistry = handlerRegistry;
        _agentOptions = options ?? new PlanAndExecuteAgentOptions();
        _validator = validator ?? new PlanValidator();
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc />
    public async Task<AgentRunResult<PlanAndExecuteOutput>> RunAsync(
        string input,
        AgentRunContext context,
        AgentRunOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(options);

        var startedAt = _timeProvider.GetUtcNow();

        try
        {
            options.Validate();
        }
        catch (ArgumentOutOfRangeException)
        {
            return Rejected(
                "plan.invalid_run_options",
                "One or more execution limits are invalid.",
                startedAt);
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            return Rejected(
                "plan.empty_task",
                "A non-empty task is required.",
                startedAt);
        }

        var timeout = options.Timeout ?? _agentOptions.Timeout;
        if (timeout > TimeSpan.FromMilliseconds(4_294_967_294d))
        {
            return Rejected(
                "plan.timeout_out_of_range",
                "The configured timeout exceeds the supported timer range.",
                startedAt);
        }

        var maxExecutedSteps = options.MaxSteps ?? _agentOptions.MaxPlanSteps;

        using var timeoutSource = new CancellationTokenSource(timeout);
        using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutSource.Token);
        var runToken = linkedSource.Token;

        ExecutionPlan? plan = null;
        var stepResults = new List<PlanStepResult>();
        PlanStep? activeStep = null;
        DateTimeOffset? activeStepStartedAt = null;

        try
        {
            await PublishEventAsync(
                options,
                context,
                "plan.generation.started",
                cancellationToken: runToken);

            try
            {
                var request = new PlanningRequest(
                    input,
                    _agentOptions.MaxPlanSteps,
                    _handlerRegistry.StepTypes);

                plan = await _planningStrategy.GenerateAsync(request, runToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (runToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception)
            {
                return await StopWithFailureAsync(
                    options,
                    context,
                    startedAt,
                    AgentRunStatus.Failed,
                    AgentFailureCategory.ModelProviderFailure,
                    "plan.generation_failed",
                    "The planning strategy failed to generate a plan.",
                    plan,
                    stepResults);
            }

            if (plan is null)
            {
                return await StopWithFailureAsync(
                    options,
                    context,
                    startedAt,
                    AgentRunStatus.Failed,
                    AgentFailureCategory.ModelProviderFailure,
                    "plan.empty_candidate",
                    "The planning strategy returned no plan.",
                    null,
                    stepResults);
            }

            await PublishEventAsync(
                options,
                context,
                "plan.generation.completed",
                new Dictionary<string, string>
                {
                    ["step_count"] = plan.Steps.Count.ToString(CultureInfo.InvariantCulture)
                },
                runToken);

            var validation = _validator.Validate(
                plan,
                _handlerRegistry.StepTypes,
                _agentOptions.MaxPlanSteps);

            if (!validation.IsValid)
            {
                await PublishEventAsync(
                    options,
                    context,
                    "plan.validation.failed",
                    new Dictionary<string, string>
                    {
                        ["issue_count"] = validation.Issues.Count.ToString(CultureInfo.InvariantCulture)
                    },
                    runToken);

                var firstIssue = validation.Issues[0];
                return AgentRunResult<PlanAndExecuteOutput>.Stopped(
                    AgentRunStatus.Rejected,
                    new AgentFailure(
                        AgentFailureCategory.ValidationFailure,
                        firstIssue.Code,
                        firstIssue.Message),
                    startedAt,
                    _timeProvider.GetUtcNow(),
                    new PlanAndExecuteOutput(plan, stepResults));
            }

            await PublishEventAsync(
                options,
                context,
                "plan.validation.passed",
                new Dictionary<string, string>
                {
                    ["step_count"] = plan.Steps.Count.ToString(CultureInfo.InvariantCulture)
                },
                runToken);

            var completedOutputs = new Dictionary<string, JsonElement>(StringComparer.Ordinal);

            for (var index = 0; index < plan.Steps.Count; index++)
            {
                runToken.ThrowIfCancellationRequested();

                var step = plan.Steps[index];
                if (stepResults.Count >= maxExecutedSteps)
                {
                    AddSkippedSteps(
                        plan,
                        index,
                        stepResults,
                        "plan.step_budget_exhausted",
                        "The step was not started because the execution step budget was exhausted.");

                    await PublishTerminalEventAsync(
                        options,
                        context,
                        "plan.budget_exhausted",
                        stepResults.Count,
                        plan.Steps.Count);

                    return AgentRunResult<PlanAndExecuteOutput>.Stopped(
                        AgentRunStatus.BudgetExhausted,
                        new AgentFailure(
                            AgentFailureCategory.BudgetExhausted,
                            "plan.step_budget_exhausted",
                            "The plan could not finish within the configured execution step budget."),
                        startedAt,
                        _timeProvider.GetUtcNow(),
                        new PlanAndExecuteOutput(plan, stepResults));
                }

                if (!_handlerRegistry.TryGet(step.StepType, out var handler) || handler is null)
                {
                    var failure = new AgentFailure(
                        AgentFailureCategory.UnsupportedCapability,
                        "plan.step_handler_missing",
                        "A registered step handler is not available for the validated plan step.");

                    stepResults.Add(new PlanStepResult(
                        step.Id,
                        PlanStepStatus.Failed,
                        _timeProvider.GetUtcNow(),
                        _timeProvider.GetUtcNow(),
                        failure: failure));

                    AddBlockedSteps(plan, index + 1, stepResults);

                    return await ReturnStepFailureAsync(
                        options,
                        context,
                        startedAt,
                        plan,
                        stepResults,
                        failure);
                }

                var referencedOutputs = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
                foreach (var reference in step.InputReferences)
                {
                    if (completedOutputs.TryGetValue(reference, out var referencedOutput))
                    {
                        referencedOutputs.Add(reference, referencedOutput.Clone());
                    }
                }

                activeStep = step;
                activeStepStartedAt = _timeProvider.GetUtcNow();

                await PublishEventAsync(
                    options,
                    context,
                    "plan.step.started",
                    new Dictionary<string, string>
                    {
                        ["step_id"] = step.Id,
                        ["step_type"] = step.StepType,
                        ["step_index"] = (index + 1).ToString(CultureInfo.InvariantCulture)
                    },
                    runToken);

                JsonElement output;
                try
                {
                    var stepContext = new PlanStepExecutionContext(
                        input,
                        step,
                        context,
                        referencedOutputs);

                    output = await handler.ExecuteAsync(stepContext, runToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (runToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception)
                {
                    var failure = new AgentFailure(
                        AgentFailureCategory.InternalFailure,
                        "plan.step_execution_failed",
                        "The plan step handler failed.");

                    stepResults.Add(new PlanStepResult(
                        step.Id,
                        PlanStepStatus.Failed,
                        activeStepStartedAt.Value,
                        _timeProvider.GetUtcNow(),
                        failure: failure));

                    activeStep = null;
                    activeStepStartedAt = null;
                    AddBlockedSteps(plan, index + 1, stepResults);

                    return await ReturnStepFailureAsync(
                        options,
                        context,
                        startedAt,
                        plan,
                        stepResults,
                        failure);
                }

                if (output.ValueKind == JsonValueKind.Undefined)
                {
                    var failure = new AgentFailure(
                        AgentFailureCategory.ValidationFailure,
                        "plan.step_output_undefined",
                        "The plan step handler returned an undefined JSON value.");

                    stepResults.Add(new PlanStepResult(
                        step.Id,
                        PlanStepStatus.Failed,
                        activeStepStartedAt.Value,
                        _timeProvider.GetUtcNow(),
                        failure: failure));

                    activeStep = null;
                    activeStepStartedAt = null;
                    AddBlockedSteps(plan, index + 1, stepResults);

                    return await ReturnStepFailureAsync(
                        options,
                        context,
                        startedAt,
                        plan,
                        stepResults,
                        failure);
                }

                var completedAt = _timeProvider.GetUtcNow();
                var ownedOutput = output.Clone();
                completedOutputs.Add(step.Id, ownedOutput);

                stepResults.Add(new PlanStepResult(
                    step.Id,
                    PlanStepStatus.Completed,
                    activeStepStartedAt.Value,
                    completedAt,
                    output: ownedOutput));

                activeStep = null;
                activeStepStartedAt = null;

                await PublishEventAsync(
                    options,
                    context,
                    "plan.step.completed",
                    new Dictionary<string, string>
                    {
                        ["step_id"] = step.Id
                    },
                    runToken);
            }

            await PublishTerminalEventAsync(
                options,
                context,
                "plan.completed",
                stepResults.Count,
                plan.Steps.Count);

            return AgentRunResult<PlanAndExecuteOutput>.Completed(
                new PlanAndExecuteOutput(plan, stepResults),
                startedAt,
                _timeProvider.GetUtcNow());
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            AddCancelledActiveStep(activeStep, activeStepStartedAt, stepResults);
            if (plan is not null)
            {
                AddBlockedSteps(plan, stepResults.Count, stepResults);
            }

            await PublishTerminalEventAsync(
                options,
                context,
                "plan.cancelled",
                stepResults.Count,
                plan?.Steps.Count ?? 0);

            return AgentRunResult<PlanAndExecuteOutput>.Stopped(
                AgentRunStatus.Cancelled,
                new AgentFailure(
                    AgentFailureCategory.Cancelled,
                    "plan.cancelled",
                    "The caller cancelled plan generation or execution."),
                startedAt,
                _timeProvider.GetUtcNow(),
                new PlanAndExecuteOutput(plan, stepResults));
        }
        catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested)
        {
            AddCancelledActiveStep(activeStep, activeStepStartedAt, stepResults);
            if (plan is not null)
            {
                AddBlockedSteps(plan, stepResults.Count, stepResults);
            }

            await PublishTerminalEventAsync(
                options,
                context,
                "plan.timed_out",
                stepResults.Count,
                plan?.Steps.Count ?? 0);

            return AgentRunResult<PlanAndExecuteOutput>.Stopped(
                AgentRunStatus.TimedOut,
                new AgentFailure(
                    AgentFailureCategory.TimedOut,
                    "plan.timed_out",
                    "Plan generation or execution exceeded its configured time limit."),
                startedAt,
                _timeProvider.GetUtcNow(),
                new PlanAndExecuteOutput(plan, stepResults));
        }
    }

    private AgentRunResult<PlanAndExecuteOutput> Rejected(
        string code,
        string message,
        DateTimeOffset startedAt)
    {
        return AgentRunResult<PlanAndExecuteOutput>.Stopped(
            AgentRunStatus.Rejected,
            new AgentFailure(AgentFailureCategory.InvalidInput, code, message),
            startedAt,
            _timeProvider.GetUtcNow());
    }

    private async Task<AgentRunResult<PlanAndExecuteOutput>> ReturnStepFailureAsync(
        AgentRunOptions options,
        AgentRunContext context,
        DateTimeOffset startedAt,
        ExecutionPlan plan,
        IReadOnlyList<PlanStepResult> stepResults,
        AgentFailure failure)
    {
        var failedStep = stepResults.LastOrDefault(static step => step.Status == PlanStepStatus.Failed);
        if (failedStep is not null)
        {
            await PublishEventAsync(
                options,
                context,
                "plan.step.failed",
                new Dictionary<string, string>
                {
                    ["step_id"] = failedStep.StepId,
                    ["failure_code"] = failedStep.Failure?.Code ?? "plan.step_failed"
                },
                CancellationToken.None);
        }

        AddBlockedSteps(plan, stepResults.Count, stepResults);

        await PublishTerminalEventAsync(
            options,
            context,
            "plan.partially_completed",
            stepResults.Count,
            plan.Steps.Count);

        var output = new PlanAndExecuteOutput(plan, stepResults);
        var completedCount = output.CompletedStepCount;

        if (completedCount > 0)
        {
            return AgentRunResult<PlanAndExecuteOutput>.PartiallyCompleted(
                output,
                failure,
                startedAt,
                _timeProvider.GetUtcNow());
        }

        return AgentRunResult<PlanAndExecuteOutput>.Stopped(
            AgentRunStatus.Failed,
            failure,
            startedAt,
            _timeProvider.GetUtcNow(),
            output);
    }

    private void AddSkippedSteps(
        ExecutionPlan plan,
        int startIndex,
        List<PlanStepResult> results,
        string code,
        string message)
    {
        var now = _timeProvider.GetUtcNow();
        for (var index = startIndex; index < plan.Steps.Count; index++)
        {
            results.Add(new PlanStepResult(
                plan.Steps[index].Id,
                PlanStepStatus.Skipped,
                now,
                now,
                failure: new AgentFailure(AgentFailureCategory.BudgetExhausted, code, message)));
        }
    }

    private void AddBlockedSteps(
        ExecutionPlan plan,
        int startIndex,
        List<PlanStepResult> results)
    {
        var now = DateTimeOffset.UtcNow;
        for (var index = startIndex; index < plan.Steps.Count; index++)
        {
            results.Add(new PlanStepResult(
                plan.Steps[index].Id,
                PlanStepStatus.Blocked,
                now,
                now,
                failure: new AgentFailure(
                    AgentFailureCategory.InternalFailure,
                    "plan.step_blocked",
                    "The step was not started because an earlier step failed.")));
        }
    }

    private void AddCancelledActiveStep(
        PlanStep? activeStep,
        DateTimeOffset? activeStepStartedAt,
        List<PlanStepResult> results)
    {
        if (activeStep is null || activeStepStartedAt is null)
        {
            return;
        }

        results.Add(new PlanStepResult(
            activeStep.Id,
            PlanStepStatus.Cancelled,
            activeStepStartedAt.Value,
            _timeProvider.GetUtcNow(),
            failure: new AgentFailure(
                AgentFailureCategory.Cancelled,
                "plan.step_cancelled",
                "The active plan step was cancelled.")));
    }

    private async Task<AgentRunResult<PlanAndExecuteOutput>> StopWithFailureAsync(
        AgentRunOptions options,
        AgentRunContext context,
        DateTimeOffset startedAt,
        AgentRunStatus status,
        AgentFailureCategory category,
        string code,
        string message,
        ExecutionPlan? plan,
        IReadOnlyList<PlanStepResult> stepResults)
    {
        await PublishTerminalEventAsync(options, context, "plan.failed", stepResults.Count, plan?.Steps.Count ?? 0);

        return AgentRunResult<PlanAndExecuteOutput>.Stopped(
            status,
            new AgentFailure(category, code, message),
            startedAt,
            _timeProvider.GetUtcNow(),
            new PlanAndExecuteOutput(plan, stepResults));
    }

    private async ValueTask PublishTerminalEventAsync(
        AgentRunOptions options,
        AgentRunContext context,
        string eventName,
        int completedOrAttemptedSteps,
        int totalSteps)
    {
        await PublishEventAsync(
            options,
            context,
            eventName,
            new Dictionary<string, string>
            {
                ["attempted_steps"] = completedOrAttemptedSteps.ToString(CultureInfo.InvariantCulture),
                ["total_steps"] = totalSteps.ToString(CultureInfo.InvariantCulture)
            },
            CancellationToken.None);
    }

    private ValueTask PublishEventAsync(
        AgentRunOptions options,
        AgentRunContext context,
        string eventName,
        IReadOnlyDictionary<string, string>? attributes = null,
        CancellationToken cancellationToken = default)
    {
        return AgentRunEventPublisher.PublishAsync(
            options,
            context,
            eventName,
            attributes,
            _timeProvider,
            cancellationToken);
    }
}
