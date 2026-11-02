using System.Globalization;
using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Reflection.Evaluation;
using AgentForge.Agents.Reflection.Generation;

namespace AgentForge.Agents.Reflection;

/// <summary>
/// Generates candidates, evaluates them independently, and performs bounded iterative refinement.
/// </summary>
public sealed class ReflectionAgent : IAgent<string, ReflectionAgentOutput>
{
    private readonly IReflectionGenerationStrategy _generationStrategy;
    private readonly IReflectionEvaluator _evaluator;
    private readonly ReflectionAgentOptions _agentOptions;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Creates a Reflection agent with independent generation and evaluation strategies.
    /// </summary>
    public ReflectionAgent(
        IReflectionGenerationStrategy generationStrategy,
        IReflectionEvaluator evaluator,
        ReflectionAgentOptions? options = null,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(generationStrategy);
        ArgumentNullException.ThrowIfNull(evaluator);

        _generationStrategy = generationStrategy;
        _evaluator = evaluator;
        _agentOptions = options ?? new ReflectionAgentOptions();
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc />
    public async Task<AgentRunResult<ReflectionAgentOutput>> RunAsync(
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
                "reflection.invalid_run_options",
                "One or more execution limits are invalid.",
                startedAt);
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            return Rejected(
                "reflection.empty_task",
                "A non-empty task is required.",
                startedAt);
        }

        var timeout = options.Timeout ?? _agentOptions.Timeout;
        if (timeout > TimeSpan.FromMilliseconds(4_294_967_294d))
        {
            return Rejected(
                "reflection.timeout_out_of_range",
                "The configured timeout exceeds the supported timer range.",
                startedAt);
        }

        var maxAttempts = options.MaxSteps ?? _agentOptions.MaxAttempts;

        using var timeoutSource = new CancellationTokenSource(timeout);
        using var linkedSource = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutSource.Token);
        var runToken = linkedSource.Token;

        var attempts = new List<ReflectionAttempt>();
        string? previousCandidate = null;
        string? feedback = null;
        string? bestCandidate = null;
        double? bestScore = null;
        var isAccepted = false;

        try
        {
            await PublishEventAsync(
                options,
                context,
                "reflection.run.started",
                cancellationToken: runToken);

            for (var attemptNumber = 1; attemptNumber <= maxAttempts; attemptNumber++)
            {
                runToken.ThrowIfCancellationRequested();

                await PublishEventAsync(
                    options,
                    context,
                    "reflection.generation.started",
                    new Dictionary<string, string>
                    {
                        ["attempt"] = attemptNumber.ToString(CultureInfo.InvariantCulture),
                        ["max_attempts"] = maxAttempts.ToString(CultureInfo.InvariantCulture)
                    },
                    runToken);

                string candidate;
                try
                {
                    candidate = await _generationStrategy.GenerateAsync(
                        new ReflectionGenerationRequest(
                            input,
                            attemptNumber,
                            maxAttempts,
                            previousCandidate,
                            feedback),
                        runToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (runToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception)
                {
                    return await StopForFailureAsync(
                        options,
                        context,
                        startedAt,
                        AgentFailureCategory.ModelProviderFailure,
                        "reflection.generation_failed",
                        "The candidate generation strategy failed.",
                        bestCandidate,
                        bestScore,
                        attempts);
                }

                if (string.IsNullOrWhiteSpace(candidate))
                {
                    return await StopForFailureAsync(
                        options,
                        context,
                        startedAt,
                        AgentFailureCategory.ModelProviderFailure,
                        "reflection.empty_candidate",
                        "The candidate generation strategy returned empty content.",
                        bestCandidate,
                        bestScore,
                        attempts);
                }

                previousCandidate = candidate;
                if (bestCandidate is null)
                {
                    bestCandidate = candidate;
                }

                attempts.Add(new ReflectionAttempt(
                    attemptNumber,
                    candidate,
                    Evaluation: null,
                    Failure: null));

                await PublishEventAsync(
                    options,
                    context,
                    "reflection.candidate.generated",
                    new Dictionary<string, string>
                    {
                        ["attempt"] = attemptNumber.ToString(CultureInfo.InvariantCulture),
                        ["candidate_length"] = candidate.Length.ToString(CultureInfo.InvariantCulture)
                    },
                    runToken);

                await PublishEventAsync(
                    options,
                    context,
                    "reflection.evaluation.started",
                    new Dictionary<string, string>
                    {
                        ["attempt"] = attemptNumber.ToString(CultureInfo.InvariantCulture)
                    },
                    runToken);

                ReflectionEvaluation evaluation;
                try
                {
                    evaluation = await _evaluator.EvaluateAsync(
                        new ReflectionEvaluationRequest(input, candidate, attemptNumber),
                        runToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (runToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception)
                {
                    var failure = new AgentFailure(
                        AgentFailureCategory.EvaluationFailure,
                        "reflection.evaluation_failed",
                        "The candidate evaluator failed.");

                    attempts[^1] = attempts[^1] with { Failure = failure };

                    return await StopForFailureAsync(
                        options,
                        context,
                        startedAt,
                        AgentFailureCategory.EvaluationFailure,
                        failure.Code,
                        failure.Message,
                        bestCandidate,
                        bestScore,
                        attempts);
                }

                if (evaluation is null)
                {
                    var failure = new AgentFailure(
                        AgentFailureCategory.EvaluationFailure,
                        "reflection.empty_evaluation",
                        "The evaluator returned no evaluation result.");

                    attempts[^1] = attempts[^1] with { Failure = failure };

                    return await StopForFailureAsync(
                        options,
                        context,
                        startedAt,
                        failure.Category,
                        failure.Code,
                        failure.Message,
                        bestCandidate,
                        bestScore,
                        attempts);
                }

                attempts[^1] = attempts[^1] with { Evaluation = evaluation };

                var previousBestScore = bestScore;

                if (evaluation.IsAccepted)
                {
                    bestCandidate = candidate;
                    bestScore = evaluation.Score ?? bestScore;
                    isAccepted = true;
                }
                else if (evaluation.Score is { } score)
                {
                    if (bestScore is null || score > bestScore.Value)
                    {
                        bestCandidate = candidate;
                        bestScore = score;
                    }
                }
                else if (bestScore is null)
                {
                    // Without comparable scores, retain the latest evaluated candidate.
                    bestCandidate = candidate;
                }

                await PublishEventAsync(
                    options,
                    context,
                    "reflection.candidate.evaluated",
                    new Dictionary<string, string>
                    {
                        ["attempt"] = attemptNumber.ToString(CultureInfo.InvariantCulture),
                        ["accepted"] = evaluation.IsAccepted.ToString().ToLowerInvariant(),
                        ["score"] = evaluation.Score?.ToString("0.####", CultureInfo.InvariantCulture) ?? "unknown",
                        ["best_score"] = bestScore?.ToString("0.####", CultureInfo.InvariantCulture) ?? "unknown"
                    },
                    runToken);

                if (evaluation.IsAccepted)
                {
                    isAccepted = true;

                    await PublishTerminalEventAsync(
                        options,
                        context,
                        "reflection.accepted",
                        attemptNumber,
                        bestScore);

                    return AgentRunResult<ReflectionAgentOutput>.Completed(
                        new ReflectionAgentOutput(bestCandidate, bestScore, true, attempts),
                        startedAt,
                        _timeProvider.GetUtcNow());
                }

                if (_agentOptions.StopOnNoImprovement &&
                    previousBestScore.HasValue &&
                    evaluation.Score.HasValue &&
                    evaluation.Score.Value <= previousBestScore.Value + _agentOptions.MinimumScoreImprovement)
                {
                    await PublishTerminalEventAsync(
                        options,
                        context,
                        "reflection.no_improvement",
                        attemptNumber,
                        bestScore);

                    return AgentRunResult<ReflectionAgentOutput>.PartiallyCompleted(
                        new ReflectionAgentOutput(bestCandidate, bestScore, false, attempts),
                        new AgentFailure(
                            AgentFailureCategory.EvaluationFailure,
                            "reflection.no_improvement",
                            "The latest candidate did not improve on the best known score."),
                        startedAt,
                        _timeProvider.GetUtcNow());
                }

                feedback = evaluation.Feedback;
            }

            await PublishTerminalEventAsync(
                options,
                context,
                "reflection.attempt_limit_reached",
                attempts.Count,
                bestScore);

            return AgentRunResult<ReflectionAgentOutput>.Stopped(
                AgentRunStatus.BudgetExhausted,
                new AgentFailure(
                    AgentFailureCategory.BudgetExhausted,
                    "reflection.attempt_limit",
                    "The Reflection agent reached its configured attempt limit without an accepted candidate."),
                startedAt,
                _timeProvider.GetUtcNow(),
                new ReflectionAgentOutput(bestCandidate, bestScore, isAccepted, attempts));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await PublishTerminalEventAsync(
                options,
                context,
                "reflection.cancelled",
                attempts.Count,
                bestScore);

            return AgentRunResult<ReflectionAgentOutput>.Stopped(
                AgentRunStatus.Cancelled,
                new AgentFailure(
                    AgentFailureCategory.Cancelled,
                    "reflection.cancelled",
                    "The caller cancelled the Reflection run."),
                startedAt,
                _timeProvider.GetUtcNow(),
                new ReflectionAgentOutput(bestCandidate, bestScore, isAccepted, attempts));
        }
        catch (OperationCanceledException) when (timeoutSource.IsCancellationRequested)
        {
            await PublishTerminalEventAsync(
                options,
                context,
                "reflection.timed_out",
                attempts.Count,
                bestScore);

            return AgentRunResult<ReflectionAgentOutput>.Stopped(
                AgentRunStatus.TimedOut,
                new AgentFailure(
                    AgentFailureCategory.TimedOut,
                    "reflection.timed_out",
                    "The Reflection run exceeded its configured time limit."),
                startedAt,
                _timeProvider.GetUtcNow(),
                new ReflectionAgentOutput(bestCandidate, bestScore, isAccepted, attempts));
        }
    }

    private AgentRunResult<ReflectionAgentOutput> Rejected(
        string code,
        string message,
        DateTimeOffset startedAt)
    {
        return AgentRunResult<ReflectionAgentOutput>.Stopped(
            AgentRunStatus.Rejected,
            new AgentFailure(AgentFailureCategory.InvalidInput, code, message),
            startedAt,
            _timeProvider.GetUtcNow());
    }

    private async Task<AgentRunResult<ReflectionAgentOutput>> StopForFailureAsync(
        AgentRunOptions options,
        AgentRunContext context,
        DateTimeOffset startedAt,
        AgentFailureCategory category,
        string code,
        string message,
        string? bestCandidate,
        double? bestScore,
        IReadOnlyList<ReflectionAttempt> attempts)
    {
        await PublishTerminalEventAsync(
            options,
            context,
            "reflection.failed",
            attempts.Count,
            bestScore);

        var output = new ReflectionAgentOutput(bestCandidate, bestScore, false, attempts);
        if (bestCandidate is not null)
        {
            return AgentRunResult<ReflectionAgentOutput>.PartiallyCompleted(
                output,
                new AgentFailure(category, code, message),
                startedAt,
                _timeProvider.GetUtcNow());
        }

        return AgentRunResult<ReflectionAgentOutput>.Stopped(
            AgentRunStatus.Failed,
            new AgentFailure(category, code, message),
            startedAt,
            _timeProvider.GetUtcNow(),
            output);
    }

    private async ValueTask PublishTerminalEventAsync(
        AgentRunOptions options,
        AgentRunContext context,
        string eventName,
        int attempts,
        double? bestScore)
    {
        await PublishEventAsync(
            options,
            context,
            eventName,
            new Dictionary<string, string>
            {
                ["attempts"] = attempts.ToString(CultureInfo.InvariantCulture),
                ["best_score"] = bestScore?.ToString("0.####", CultureInfo.InvariantCulture) ?? "unknown"
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
