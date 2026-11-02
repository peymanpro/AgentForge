using AgentForge.Abstractions.Execution;

namespace AgentForge.Agents.Classical;

/// <summary>
/// Updates an explicit internal state from the previous state and current percept, then applies ordered condition-action rules.
/// State is supplied and returned by each run; the agent keeps no mutable per-run state.
/// </summary>
/// <typeparam name="TPercept">The current percept type.</typeparam>
/// <typeparam name="TState">The internal world-model state type.</typeparam>
/// <typeparam name="TAction">The action type.</typeparam>
public sealed class ModelBasedReflexAgent<TPercept, TState, TAction>
    : IAgent<ModelBasedReflexInput<TPercept, TState>, ModelBasedReflexOutput<TState, TAction>>
    where TPercept : notnull
    where TState : notnull
    where TAction : notnull
{
    private readonly Func<TState, TPercept, TState> _stateUpdater;
    private readonly IReadOnlyList<ReflexRule<TState, TAction>> _rules;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Creates a model-based reflex agent.
    /// </summary>
    /// <param name="stateUpdater">Updates the internal state using the previous state and current percept.</param>
    /// <param name="rules">Rules are checked in enumeration order; the first matching rule wins.</param>
    /// <param name="timeProvider">Optional clock used for deterministic tests.</param>
    public ModelBasedReflexAgent(
        Func<TState, TPercept, TState> stateUpdater,
        IEnumerable<ReflexRule<TState, TAction>> rules,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(stateUpdater);
        ArgumentNullException.ThrowIfNull(rules);

        var ruleArray = rules.ToArray();
        if (ruleArray.Length == 0)
        {
            throw new ArgumentException("At least one reflex rule is required.", nameof(rules));
        }

        if (ruleArray.Any(rule => rule is null))
        {
            throw new ArgumentException("The rule collection cannot contain null entries.", nameof(rules));
        }

        var duplicateName = ruleArray
            .GroupBy(rule => rule.Name, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateName is not null)
        {
            throw new ArgumentException($"Rule names must be unique. Duplicate name: '{duplicateName.Key}'.", nameof(rules));
        }

        _stateUpdater = stateUpdater;
        _rules = ruleArray;
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
                new AgentFailure(AgentFailureCategory.InvalidInput, "classical.invalid_options", "One or more run limits are invalid."),
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

                if (!rule.IsApplicable(currentState))
                {
                    continue;
                }

                var action = rule.CreateAction(currentState);
                var output = new ModelBasedReflexOutput<TState, TAction>(
                    currentState,
                    ClassicalDecision<TAction>.Select(action, $"Rule '{rule.Name}' matched the updated internal state."),
                    rule.Name);

                return Task.FromResult(AgentRunResult<ModelBasedReflexOutput<TState, TAction>>.Completed(
                    output,
                    startedAt,
                    _timeProvider.GetUtcNow()));
            }

            var noActionOutput = new ModelBasedReflexOutput<TState, TAction>(
                currentState,
                ClassicalDecision<TAction>.NoAction("No configured rule matched the updated internal state."),
                null);

            return Task.FromResult(AgentRunResult<ModelBasedReflexOutput<TState, TAction>>.Completed(
                noActionOutput,
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
                new AgentFailure(AgentFailureCategory.InternalFailure, "classical.state_or_rule_evaluation_failed", "The state update or reflex rule evaluation failed."),
                startedAt,
                _timeProvider.GetUtcNow()));
        }
    }

    private AgentRunResult<ModelBasedReflexOutput<TState, TAction>> Cancelled(DateTimeOffset startedAt)
    {
        return AgentRunResult<ModelBasedReflexOutput<TState, TAction>>.Stopped(
            AgentRunStatus.Cancelled,
            new AgentFailure(AgentFailureCategory.Cancelled, "classical.cancelled", "The caller cancelled the classical agent run."),
            startedAt,
            _timeProvider.GetUtcNow());
    }
}
