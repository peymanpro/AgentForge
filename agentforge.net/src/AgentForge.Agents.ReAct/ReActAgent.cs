using System.Globalization;
using System.Text.Json;
using AgentForge.Abstractions.Execution;
using AgentForge.Models.Chat;
using AgentForge.Tools;

namespace AgentForge.Agents.ReAct;

/// <summary>
/// Executes a bounded ReAct loop using a replaceable decision strategy and policy-controlled tools.
/// </summary>
public sealed class ReActAgent : IAgent<string, ReActAgentOutput>
{
    private readonly IReactDecisionStrategy _decisionStrategy;
    private readonly ToolRegistry _toolRegistry;
    private readonly ToolExecutor _toolExecutor;
    private readonly ReActAgentOptions _agentOptions;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Creates a ReAct agent with an explicitly scoped tool registry and authorization policy.
    /// </summary>
    /// <param name="decisionStrategy">The strategy that obtains model decisions.</param>
    /// <param name="toolRegistry">The tools available to this agent.</param>
    /// <param name="authorizationPolicy">The application-controlled policy applied to every tool invocation.</param>
    /// <param name="options">Optional agent-specific configuration and finite default limits.</param>
    /// <param name="timeProvider">The clock used for run timestamps.</param>
    public ReActAgent(
        IReactDecisionStrategy decisionStrategy,
        ToolRegistry toolRegistry,
        IToolAuthorizationPolicy authorizationPolicy,
        ReActAgentOptions? options = null,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(decisionStrategy);
        ArgumentNullException.ThrowIfNull(toolRegistry);
        ArgumentNullException.ThrowIfNull(authorizationPolicy);

        _decisionStrategy = decisionStrategy;
        _toolRegistry = toolRegistry;
        _toolExecutor = new ToolExecutor(toolRegistry, authorizationPolicy);
        _agentOptions = options ?? new ReActAgentOptions();
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc />
    public async Task<AgentRunResult<ReActAgentOutput>> RunAsync(
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
                "run.invalid_options",
                "One or more execution limits are invalid.",
                startedAt);
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            return Rejected(
                "run.empty_input",
                "A non-empty task is required.",
                startedAt);
        }

        var maxModelTurns = options.MaxModelRequests ?? _agentOptions.MaxModelTurns;
        var maxToolInvocations = options.MaxToolInvocations ?? _agentOptions.MaxToolInvocations;
        var maxSteps = options.MaxSteps is { } configuredSteps
            ? (long)configuredSteps
            : (long)maxModelTurns + maxToolInvocations;
        var timeout = options.Timeout ?? _agentOptions.Timeout;

        using var timeoutSource = new CancellationTokenSource(timeout);
        using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutSource.Token);
        var runToken = linkedSource.Token;

        var messages = new List<ChatMessage>();
        if (_agentOptions.SystemPrompt is not null)
        {
            messages.Add(ChatMessage.System(_agentOptions.SystemPrompt));
        }

        messages.Add(ChatMessage.User(input));

        var toolExecutions = new List<ToolExecutionResult>();
        var modelTurns = 0;
        var toolInvocations = 0;
        long steps = 0;
        ModelFinishReason? lastFinishReason = null;
        IReadOnlyList<ModelToolCall> pendingToolCalls = Array.Empty<ModelToolCall>();

        try
        {
            await PublishEventAsync(
                options,
                context,
                "agent.run.started",
                cancellationToken: runToken);

            while (true)
            {
                runToken.ThrowIfCancellationRequested();

                if (modelTurns >= maxModelTurns || steps >= maxSteps)
                {
                    return await StopForBudgetAsync(
                        options,
                        context,
                        startedAt,
                        "run.model_budget_exhausted",
                        "The ReAct run reached its model-turn or step limit.",
                        modelTurns,
                        toolInvocations,
                        toolExecutions,
                        lastFinishReason,
                        pendingToolCalls);
                }

                modelTurns++;
                steps++;

                await PublishEventAsync(
                    options,
                    context,
                    "model.request.started",
                    new Dictionary<string, string>
                    {
                        ["model_turn"] = modelTurns.ToString(CultureInfo.InvariantCulture)
                    },
                    runToken);

                ModelResponse response;
                try
                {
                    var request = new ModelRequest(messages, _agentOptions.GenerationOptions);
                    response = await _decisionStrategy.DecideAsync(
                        request,
                        _toolRegistry.Definitions,
                        runToken).ConfigureAwait(false);
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
                        "model.request_failed",
                        "The model decision strategy failed.",
                        modelTurns,
                        toolInvocations,
                        toolExecutions,
                        lastFinishReason,
                        pendingToolCalls);
                }

                if (response is null)
                {
                    return await StopWithFailureAsync(
                        options,
                        context,
                        startedAt,
                        AgentRunStatus.Failed,
                        AgentFailureCategory.ModelProviderFailure,
                        "model.empty_response",
                        "The model decision strategy returned no response.",
                        modelTurns,
                        toolInvocations,
                        toolExecutions,
                        lastFinishReason,
                        pendingToolCalls);
                }

                lastFinishReason = response.FinishReason;
                await PublishEventAsync(
                    options,
                    context,
                    "model.response.received",
                    new Dictionary<string, string>
                    {
                        ["model_turn"] = modelTurns.ToString(CultureInfo.InvariantCulture),
                        ["finish_reason"] = response.FinishReason.ToString()
                    },
                    runToken);

                if (response.FinishReason != ModelFinishReason.ToolCalls)
                {
                    var finalResponse = response.Message.Content ?? string.Empty;
                    var output = BuildOutput(
                        finalResponse,
                        response.FinishReason,
                        modelTurns,
                        toolInvocations,
                        toolExecutions,
                        Array.Empty<ModelToolCall>());

                    if (response.FinishReason == ModelFinishReason.Length)
                    {
                        var failure = new AgentFailure(
                            AgentFailureCategory.ModelProviderFailure,
                            "model.output_limit",
                            "The model stopped after reaching its output limit.");

                        await PublishEventAsync(
                            options,
                            context,
                            "agent.run.partially_completed",
                            new Dictionary<string, string>
                            {
                                ["model_turns"] = modelTurns.ToString(CultureInfo.InvariantCulture),
                                ["tool_invocations"] = toolInvocations.ToString(CultureInfo.InvariantCulture)
                            },
                            runToken);

                        return AgentRunResult<ReActAgentOutput>.PartiallyCompleted(
                            output,
                            failure,
                            startedAt,
                            _timeProvider.GetUtcNow());
                    }

                    await PublishEventAsync(
                        options,
                        context,
                        "agent.run.completed",
                        new Dictionary<string, string>
                        {
                            ["model_turns"] = modelTurns.ToString(CultureInfo.InvariantCulture),
                            ["tool_invocations"] = toolInvocations.ToString(CultureInfo.InvariantCulture)
                        },
                        runToken);

                    return AgentRunResult<ReActAgentOutput>.Completed(
                        output,
                        startedAt,
                        _timeProvider.GetUtcNow());
                }

                messages.Add(response.Message);
                var proposedCalls = response.Message.ToolCalls;

                for (var index = 0; index < proposedCalls.Count; index++)
                {
                    var toolCall = proposedCalls[index];

                    if (toolInvocations >= maxToolInvocations || steps >= maxSteps)
                    {
                        pendingToolCalls = proposedCalls.Skip(index).ToArray();
                        return await StopForBudgetAsync(
                            options,
                            context,
                            startedAt,
                            "run.tool_budget_exhausted",
                            "The ReAct run reached its tool-invocation or step limit.",
                            modelTurns,
                            toolInvocations,
                            toolExecutions,
                            lastFinishReason,
                            pendingToolCalls);
                    }

                    toolInvocations++;
                    steps++;

                    await PublishEventAsync(
                        options,
                        context,
                        "tool.call.proposed",
                        new Dictionary<string, string>
                        {
                            ["tool_call_id"] = toolCall.Id,
                            ["tool_name"] = toolCall.Name,
                            ["tool_invocation"] = toolInvocations.ToString(CultureInfo.InvariantCulture)
                        },
                        runToken);

                    ToolExecutionResult toolResult;
                    try
                    {
                        toolResult = await _toolExecutor.ExecuteAsync(
                            new ToolExecutionRequest(
                                toolCall.Id,
                                toolCall.Name,
                                toolCall.Arguments,
                                context),
                            runToken).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException) when (runToken.IsCancellationRequested)
                    {
                        throw;
                    }
                    catch (Exception)
                    {
                        toolResult = ToolExecutionResult.Unsuccessful(
                            toolCall.Id,
                            ToolExecutionStatus.Failed,
                            new AgentFailure(
                                AgentFailureCategory.ToolFailure,
                                "tool.runtime_failed",
                                "The tool execution runtime failed."));
                    }

                    toolExecutions.Add(toolResult);
                    messages.Add(ChatMessage.ToolResult(
                        toolCall.Id,
                        CreateToolObservation(toolResult)));

                    await PublishEventAsync(
                        options,
                        context,
                        "tool.execution.completed",
                        new Dictionary<string, string>
                        {
                            ["tool_call_id"] = toolCall.Id,
                            ["tool_name"] = toolCall.Name,
                            ["status"] = toolResult.Status.ToString()
                        },
                        runToken);
                }

                pendingToolCalls = Array.Empty<ModelToolCall>();
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            var output = BuildOutput(
                null,
                lastFinishReason,
                modelTurns,
                toolInvocations,
                toolExecutions,
                pendingToolCalls);

            await PublishTerminalEventAsync(
                options,
                context,
                "agent.run.cancelled",
                modelTurns,
                toolInvocations);

            return AgentRunResult<ReActAgentOutput>.Stopped(
                AgentRunStatus.Cancelled,
                new AgentFailure(
                    AgentFailureCategory.Cancelled,
                    "run.cancelled",
                    "The caller cancelled the ReAct run."),
                startedAt,
                _timeProvider.GetUtcNow(),
                output);
        }
        catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested)
        {
            var output = BuildOutput(
                null,
                lastFinishReason,
                modelTurns,
                toolInvocations,
                toolExecutions,
                pendingToolCalls);

            await PublishTerminalEventAsync(
                options,
                context,
                "agent.run.timed_out",
                modelTurns,
                toolInvocations);

            return AgentRunResult<ReActAgentOutput>.Stopped(
                AgentRunStatus.TimedOut,
                new AgentFailure(
                    AgentFailureCategory.TimedOut,
                    "run.timed_out",
                    "The ReAct run exceeded its configured time limit."),
                startedAt,
                _timeProvider.GetUtcNow(),
                output);
        }
    }

    private AgentRunResult<ReActAgentOutput> Rejected(
        string code,
        string message,
        DateTimeOffset startedAt)
    {
        return AgentRunResult<ReActAgentOutput>.Stopped(
            AgentRunStatus.Rejected,
            new AgentFailure(AgentFailureCategory.InvalidInput, code, message),
            startedAt,
            _timeProvider.GetUtcNow());
    }

    private async Task<AgentRunResult<ReActAgentOutput>> StopForBudgetAsync(
        AgentRunOptions runOptions,
        AgentRunContext context,
        DateTimeOffset startedAt,
        string code,
        string message,
        int modelTurns,
        int toolInvocations,
        IReadOnlyList<ToolExecutionResult> toolExecutions,
        ModelFinishReason? finishReason,
        IReadOnlyList<ModelToolCall> pendingCalls)
    {
        await PublishTerminalEventAsync(
            runOptions,
            context,
            "agent.run.budget_exhausted",
            modelTurns,
            toolInvocations);

        return AgentRunResult<ReActAgentOutput>.Stopped(
            AgentRunStatus.BudgetExhausted,
            new AgentFailure(AgentFailureCategory.BudgetExhausted, code, message),
            startedAt,
            _timeProvider.GetUtcNow(),
            BuildOutput(null, finishReason, modelTurns, toolInvocations, toolExecutions, pendingCalls));
    }

    private async Task<AgentRunResult<ReActAgentOutput>> StopWithFailureAsync(
        AgentRunOptions runOptions,
        AgentRunContext context,
        DateTimeOffset startedAt,
        AgentRunStatus status,
        AgentFailureCategory category,
        string code,
        string message,
        int modelTurns,
        int toolInvocations,
        IReadOnlyList<ToolExecutionResult> toolExecutions,
        ModelFinishReason? finishReason,
        IReadOnlyList<ModelToolCall> pendingCalls)
    {
        await PublishTerminalEventAsync(
            runOptions,
            context,
            "agent.run.failed",
            modelTurns,
            toolInvocations);

        return AgentRunResult<ReActAgentOutput>.Stopped(
            status,
            new AgentFailure(category, code, message),
            startedAt,
            _timeProvider.GetUtcNow(),
            BuildOutput(null, finishReason, modelTurns, toolInvocations, toolExecutions, pendingCalls));
    }

    private async ValueTask PublishTerminalEventAsync(
        AgentRunOptions options,
        AgentRunContext context,
        string eventName,
        int modelTurns,
        int toolInvocations)
    {
        await PublishEventAsync(
            options,
            context,
            eventName,
            new Dictionary<string, string>
            {
                ["model_turns"] = modelTurns.ToString(CultureInfo.InvariantCulture),
                ["tool_invocations"] = toolInvocations.ToString(CultureInfo.InvariantCulture)
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

    private ReActAgentOutput BuildOutput(
        string? finalResponse,
        ModelFinishReason? finishReason,
        int modelTurns,
        int toolInvocations,
        IReadOnlyList<ToolExecutionResult> toolExecutions,
        IReadOnlyList<ModelToolCall> pendingToolCalls)
    {
        return new ReActAgentOutput(
            finalResponse,
            finishReason,
            modelTurns,
            toolInvocations,
            toolExecutions,
            pendingToolCalls);
    }

    private string CreateToolObservation(ToolExecutionResult result)
    {
        var observation = JsonSerializer.Serialize(new
        {
            status = result.Status.ToString(),
            output = result.Output,
            failure = result.Failure is null
                ? null
                : new
                {
                    category = result.Failure.Category.ToString(),
                    code = result.Failure.Code,
                    message = result.Failure.Message
                }
        });

        if (observation.Length <= _agentOptions.MaxToolObservationCharacters)
        {
            return observation;
        }

        return JsonSerializer.Serialize(new
        {
            status = "ObservationTruncated",
            message = "The tool result was omitted because it exceeded the configured observation size limit."
        });
    }
}
