using AgentForge.Abstractions.Execution;
using AgentForge.Agents.ModelBasedReflex.Domain.Decisions;
using AgentForge.Agents.ModelBasedReflex.Domain.Rules;

namespace AgentForge.Agents.ModelBasedReflex.Application.UseCases;

/// <summary>
/// Updates the supplied internal state from the previous state and current percept, then selects an action.
/// State is returned to the host rather than retained in mutable fields on the agent instance.
/// </summary>
public sealed class ModelBasedReflexAgent<TPercept, TState, TAction>
    : IAgent<ModelBasedReflexInput<TPercept, TState>, ModelBasedReflexOutput<TState, TAction>>
    where TPercept : notnull
    where TState : notnull
    where TAction : notnull
{
    private readonly Func<TState, TPercept, TState> _stateUpdater;
    private readonly IReadOnlyList<ModelBasedReflexRule<TState, TAction>> _rules;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Creates the application use case.
    /// </summary>
    public ModelBasedReflexAgent(
        Func<TState, TPercept, TState> stateUpdater,
        IEnumerable<ModelBasedReflexRule<TState, TAction>> rules,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(stateUpdater);
        ArgumentNullException.ThrowIfNull(rules);

        var items = rules.ToArray();
        if (items.Length == 0)
        {
            throw new ArgumentException("At least one rule is required.", nameof(rules));
        }

        if (items.Any(static rule => rule is null))
        {
            throw new ArgumentException("The rule collection cannot contain null entries.", nameof(rules));
        }

        var duplicate = items
            .GroupBy(static rule => rule.Name, StringComparer.Ordinal)
            .FirstOrDefault(static group => group.Count() > 1);

        if (duplicate is not null)
        {
            throw new ArgumentException($"Rule names must be unique. Duplicate name: '{duplicate.Key}'.", nameof(rules));
        }

        _stateUpdater = stateUpdater;
        _rules = Array.AsReadOnly(items);
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc />
    public Task<AgentRunResult<ModelBasedReflexOutput<TState, TAction>>> RunAsync(
        ModelBasedReflexInput<TPercept, TState> input,
        AgentRunContext context,
        AgentRunOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(options);

        var startedAt = _timeProvider.GetUtcNow();

        try
        {
            options.Validate();
        }
        catch (ArgumentOutOfRangeException)
        {
            return Task.FromResult(AgentRunResult<ModelBasedReflexOutput<TState, TAction>>.Stopped(
                AgentRunStatus.Rejected,
                new AgentFailure(
                    AgentFailureCategory.InvalidInput,
                    "model_based_reflex.invalid_options",
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
            cancellationToken.ThrowIfCancellationRequested();
            var currentState = _stateUpdater(input.PreviousState, input.Percept);
            ArgumentNullException.ThrowIfNull(currentState);

            foreach (var rule in _rules)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!rule.Matches(currentState))
                {
                    continue;
                }

                var output = new ModelBasedReflexOutput<TState, TAction>(
                    currentState,
                    ModelBasedReflexDecision<TAction>.Select(
                        rule.CreateAction(currentState),
                        $"Rule '{rule.Name}' matched the updated internal state."),
                    rule.Name);

                return Task.FromResult(AgentRunResult<ModelBasedReflexOutput<TState, TAction>>.Completed(
                    output,
                    startedAt,
                    _timeProvider.GetUtcNow()));
            }

            var noAction = new ModelBasedReflexOutput<TState, TAction>(
                currentState,
                ModelBasedReflexDecision<TAction>.NoAction("No configured rule matched the updated internal state."),
                null);

            return Task.FromResult(AgentRunResult<ModelBasedReflexOutput<TState, TAction>>.Completed(
                noAction,
                startedAt,
                _timeProvider.GetUtcNow()));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(Cancelled(startedAt));
        }
        catch (Exception)
        {
            return Task.FromResult(AgentRunResult<ModelBasedReflexOutput<TState, TAction>>.Stopped(
                AgentRunStatus.Failed,
                new AgentFailure(
                    AgentFailureCategory.InternalFailure,
                    "model_based_reflex.decision_failed",
                    "The state update or rule evaluation failed."),
                startedAt,
                _timeProvider.GetUtcNow()));
        }
    }

    private AgentRunResult<ModelBasedReflexOutput<TState, TAction>> Cancelled(DateTimeOffset startedAt)
    {
        return AgentRunResult<ModelBasedReflexOutput<TState, TAction>>.Stopped(
            AgentRunStatus.Cancelled,
            new AgentFailure(
                AgentFailureCategory.Cancelled,
                "model_based_reflex.cancelled",
                "The caller cancelled the agent run."),
            startedAt,
            _timeProvider.GetUtcNow());
    }
}
