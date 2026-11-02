using AgentForge.Abstractions.Execution;
using AgentForge.Agents.SimpleReflex.Domain.Decisions;
using AgentForge.Agents.SimpleReflex.Domain.Rules;

namespace AgentForge.Agents.SimpleReflex.Application.UseCases;

/// <summary>
/// Executes one simple-reflex decision step by evaluating ordered condition-action rules.
/// The selected action is returned as a proposal; this use case does not execute environment side effects.
/// </summary>
/// <typeparam name="TPercept">The current percept type.</typeparam>
/// <typeparam name="TAction">The proposed action type.</typeparam>
public sealed class SimpleReflexAgent<TPercept, TAction> : IAgent<TPercept, SimpleReflexOutput<TAction>>
    where TPercept : notnull
    where TAction : notnull
{
    private readonly IReadOnlyList<SimpleReflexRule<TPercept, TAction>> _rules;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Creates the application use case with an ordered rule set.
    /// </summary>
    public SimpleReflexAgent(
        IEnumerable<SimpleReflexRule<TPercept, TAction>> rules,
        TimeProvider? timeProvider = null)
    {
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

        _rules = Array.AsReadOnly(items);
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc />
    public Task<AgentRunResult<SimpleReflexOutput<TAction>>> RunAsync(
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
            return Task.FromResult(AgentRunResult<SimpleReflexOutput<TAction>>.Stopped(
                AgentRunStatus.Rejected,
                new AgentFailure(
                    AgentFailureCategory.InvalidInput,
                    "simple_reflex.invalid_options",
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
            foreach (var rule in _rules)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!rule.Matches(input))
                {
                    continue;
                }

                var output = new SimpleReflexOutput<TAction>(
                    SimpleReflexDecision<TAction>.Select(
                        rule.CreateAction(input),
                        $"Rule '{rule.Name}' matched the current percept."),
                    rule.Name);

                return Task.FromResult(AgentRunResult<SimpleReflexOutput<TAction>>.Completed(
                    output,
                    startedAt,
                    _timeProvider.GetUtcNow()));
            }

            var noAction = new SimpleReflexOutput<TAction>(
                SimpleReflexDecision<TAction>.NoAction("No configured rule matched the current percept."),
                null);

            return Task.FromResult(AgentRunResult<SimpleReflexOutput<TAction>>.Completed(
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
            return Task.FromResult(AgentRunResult<SimpleReflexOutput<TAction>>.Stopped(
                AgentRunStatus.Failed,
                new AgentFailure(
                    AgentFailureCategory.InternalFailure,
                    "simple_reflex.decision_failed",
                    "A simple-reflex rule failed during decision evaluation."),
                startedAt,
                _timeProvider.GetUtcNow()));
        }
    }

    private AgentRunResult<SimpleReflexOutput<TAction>> Cancelled(DateTimeOffset startedAt)
    {
        return AgentRunResult<SimpleReflexOutput<TAction>>.Stopped(
            AgentRunStatus.Cancelled,
            new AgentFailure(
                AgentFailureCategory.Cancelled,
                "simple_reflex.cancelled",
                "The caller cancelled the agent run."),
            startedAt,
            _timeProvider.GetUtcNow());
    }
}
