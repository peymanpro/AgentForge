using AgentForge.Abstractions.Execution;
using AgentForge.Agents.UtilityBased.Domain.Decisions;
using AgentForge.Agents.UtilityBased.Domain.Evaluation;
using AgentForge.Agents.UtilityBased.Domain.Policies;

namespace AgentForge.Agents.UtilityBased.Application.UseCases;

/// <summary>
/// Evaluates a finite candidate set and proposes the action with the greatest utility.
/// The agent does not execute environment side effects.
/// </summary>
/// <typeparam name="TPercept">The current environment percept.</typeparam>
/// <typeparam name="TAction">The candidate action type.</typeparam>
public sealed class UtilityBasedAgent<TPercept, TAction> : IAgent<TPercept, UtilityBasedOutput<TAction>>
    where TPercept : notnull
    where TAction : notnull
{
    private readonly Func<TPercept, IEnumerable<TAction>> _candidateProvider;
    private readonly Func<TPercept, TAction, double> _utilityEvaluator;
    private readonly UtilityTieBreakPolicy _tieBreakPolicy;
    private readonly TimeProvider _timeProvider;

    /// <summary>Creates the agent from candidate and utility strategies.</summary>
    public UtilityBasedAgent(
        Func<TPercept, IEnumerable<TAction>> candidateProvider,
        Func<TPercept, TAction, double> utilityEvaluator,
        UtilityTieBreakPolicy tieBreakPolicy = UtilityTieBreakPolicy.FirstCandidate,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(candidateProvider);
        ArgumentNullException.ThrowIfNull(utilityEvaluator);

        if (!Enum.IsDefined(tieBreakPolicy))
        {
            throw new ArgumentOutOfRangeException(nameof(tieBreakPolicy), tieBreakPolicy, "The tie-break policy is not defined.");
        }

        _candidateProvider = candidateProvider;
        _utilityEvaluator = utilityEvaluator;
        _tieBreakPolicy = tieBreakPolicy;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc />
    public Task<AgentRunResult<UtilityBasedOutput<TAction>>> RunAsync(
        TPercept input,
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
            return Task.FromResult(AgentRunResult<UtilityBasedOutput<TAction>>.Stopped(
                AgentRunStatus.Rejected,
                new AgentFailure(
                    AgentFailureCategory.InvalidInput,
                    "utility_based.invalid_options",
                    "One or more run limits are invalid."),
                startedAt,
                _timeProvider.GetUtcNow()));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(Cancelled(startedAt));
        }

        try
        {
            var suppliedCandidates = _candidateProvider(input);
            if (suppliedCandidates is null)
            {
                return Task.FromResult(Failed(startedAt, "utility_based.null_candidates",
                    "The candidate provider returned a null collection."));
            }

            var candidates = suppliedCandidates.ToArray();
            if (candidates.Any(static candidate => candidate is null))
            {
                return Task.FromResult(Failed(startedAt, "utility_based.null_candidate",
                    "The candidate collection contains a null action."));
            }

            var evaluations = new List<UtilityEvaluation<TAction>>(candidates.Length);
            if (candidates.Length == 0)
            {
                var emptyOutput = new UtilityBasedOutput<TAction>(
                    UtilityBasedDecision<TAction>.NoAction("The candidate provider returned no available actions."),
                    evaluations);

                return Task.FromResult(AgentRunResult<UtilityBasedOutput<TAction>>.Completed(
                    emptyOutput, startedAt, _timeProvider.GetUtcNow()));
            }

            var bestIndex = -1;
            var bestUtility = double.NegativeInfinity;
            var bestCount = 0;

            for (var index = 0; index < candidates.Length; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var candidate = candidates[index];
                var utility = _utilityEvaluator(input, candidate);

                if (!double.IsFinite(utility))
                {
                    return Task.FromResult(Failed(startedAt, "utility_based.non_finite_utility",
                        "The utility evaluator returned a non-finite score."));
                }

                evaluations.Add(new UtilityEvaluation<TAction>(candidate, utility));

                if (utility > bestUtility)
                {
                    bestUtility = utility;
                    bestIndex = index;
                    bestCount = 1;
                }
                else if (utility == bestUtility)
                {
                    bestCount++;

                    if (_tieBreakPolicy == UtilityTieBreakPolicy.LastCandidate)
                    {
                        bestIndex = index;
                    }
                }
            }

            if (_tieBreakPolicy == UtilityTieBreakPolicy.RejectAmbiguous && bestCount > 1)
            {
                var ambiguousOutput = new UtilityBasedOutput<TAction>(
                    UtilityBasedDecision<TAction>.NoAction(
                        $"No action was selected because {bestCount} candidates share the maximum utility ({bestUtility})."),
                    evaluations);

                return Task.FromResult(AgentRunResult<UtilityBasedOutput<TAction>>.Completed(
                    ambiguousOutput, startedAt, _timeProvider.GetUtcNow()));
            }

            var selected = candidates[bestIndex];
            var output = new UtilityBasedOutput<TAction>(
                UtilityBasedDecision<TAction>.Select(
                    selected,
                    bestUtility,
                    $"Selected candidate at index {bestIndex} with the maximum utility."),
                evaluations);

            return Task.FromResult(AgentRunResult<UtilityBasedOutput<TAction>>.Completed(
                output, startedAt, _timeProvider.GetUtcNow()));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(Cancelled(startedAt));
        }
        catch (Exception)
        {
            return Task.FromResult(Failed(startedAt, "utility_based.evaluation_failed",
                "Candidate generation or utility evaluation failed."));
        }
    }

    private AgentRunResult<UtilityBasedOutput<TAction>> Cancelled(DateTimeOffset startedAt)
    {
        return AgentRunResult<UtilityBasedOutput<TAction>>.Stopped(
            AgentRunStatus.Cancelled,
            new AgentFailure(
                AgentFailureCategory.Cancelled,
                "utility_based.cancelled",
                "The caller cancelled the agent run."),
            startedAt,
            _timeProvider.GetUtcNow());
    }

    private AgentRunResult<UtilityBasedOutput<TAction>> Failed(
        DateTimeOffset startedAt,
        string code,
        string message)
    {
        return AgentRunResult<UtilityBasedOutput<TAction>>.Stopped(
            AgentRunStatus.Failed,
            new AgentFailure(AgentFailureCategory.InternalFailure, code, message),
            startedAt,
            _timeProvider.GetUtcNow());
    }
}