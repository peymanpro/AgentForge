using AgentForge.Abstractions.Execution;

namespace AgentForge.Agents.Classical;

/// <summary>
/// Selects the first action whose condition matches the current percept.
/// The agent proposes an action; it does not execute environment side effects.
/// </summary>
/// <typeparam name="TPercept">The current percept type.</typeparam>
/// <typeparam name="TAction">The action type.</typeparam>
public sealed class SimpleReflexAgent<TPercept, TAction> : IAgent<TPercept, ClassicalAgentOutput<TAction>>
    where TPercept : notnull
    where TAction : notnull
{
    private readonly IReadOnlyList<ReflexRule<TPercept, TAction>> _rules;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Creates a simple-reflex agent with an explicitly ordered rule set.
    /// </summary>
    /// <param name="rules">Rules are checked in enumeration order; the first matching rule wins.</param>
    /// <param name="timeProvider">Optional clock used for deterministic tests.</param>
    public SimpleReflexAgent(
        IEnumerable<ReflexRule<TPercept, TAction>> rules,
        TimeProvider? timeProvider = null)
    {
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

        _rules = ruleArray;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc />
    public Task<AgentRunResult<ClassicalAgentOutput<TAction>>> RunAsync(
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
            return Task.FromResult(AgentRunResult<ClassicalAgentOutput<TAction>>.Stopped(
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
            foreach (var rule in _rules)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!rule.IsApplicable(input))
                {
                    continue;
                }

                var action = rule.CreateAction(input);
                var output = new ClassicalAgentOutput<TAction>(
                    ClassicalDecision<TAction>.Select(action, $"Rule '{rule.Name}' matched the current percept."),
                    rule.Name);

                return Task.FromResult(AgentRunResult<ClassicalAgentOutput<TAction>>.Completed(
                    output,
                    startedAt,
                    _timeProvider.GetUtcNow()));
            }

            var noActionOutput = new ClassicalAgentOutput<TAction>(
                ClassicalDecision<TAction>.NoAction("No configured rule matched the current percept."),
                null);

            return Task.FromResult(AgentRunResult<ClassicalAgentOutput<TAction>>.Completed(
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
            return Task.FromResult(AgentRunResult<ClassicalAgentOutput<TAction>>.Stopped(
                AgentRunStatus.Failed,
                new AgentFailure(AgentFailureCategory.InternalFailure, "classical.rule_evaluation_failed", "A reflex rule failed during evaluation."),
                startedAt,
                _timeProvider.GetUtcNow()));
        }
    }

    private AgentRunResult<ClassicalAgentOutput<TAction>> Cancelled(DateTimeOffset startedAt)
    {
        return AgentRunResult<ClassicalAgentOutput<TAction>>.Stopped(
            AgentRunStatus.Cancelled,
            new AgentFailure(AgentFailureCategory.Cancelled, "classical.cancelled", "The caller cancelled the classical agent run."),
            startedAt,
            _timeProvider.GetUtcNow());
    }
}
