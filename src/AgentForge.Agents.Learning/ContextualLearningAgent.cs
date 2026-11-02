using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Learning.Application.UseCases;
using AgentForge.Agents.Learning.Domain.Models;

namespace AgentForge.Agents.Learning;

/// <summary>A stateless LinUCB decision component; the host supplies context, feedback, and persisted state.</summary>
public sealed class ContextualLearningAgent<TAction> :
    IAgent<ContextualLearningAgentInput<TAction>, ContextualLearningAgentOutput<TAction>> where TAction : notnull
{
    private readonly SelectContextualLearningAction<TAction> _selection;
    private readonly TimeProvider _clock;

    /// <summary>Creates a contextual learning agent.</summary>
    public ContextualLearningAgent(double explorationCoefficient = 1.0, TimeProvider? timeProvider = null)
    {
        _selection = new SelectContextualLearningAction<TAction>(explorationCoefficient);
        _clock = timeProvider ?? TimeProvider.System;
    }

    /// <inheritdoc />
    public Task<AgentRunResult<ContextualLearningAgentOutput<TAction>>> RunAsync(
        ContextualLearningAgentInput<TAction> input, AgentRunContext context, AgentRunOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(options);
        var started = _clock.GetUtcNow();
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(AgentRunResult<ContextualLearningAgentOutput<TAction>>.Stopped(
                AgentRunStatus.Cancelled,
                new AgentFailure(AgentFailureCategory.Cancelled, "contextual_learning_cancelled",
                    "The contextual learning decision was cancelled."), started, _clock.GetUtcNow()));
        try
        {
            var output = _selection.Execute(input);
            return Task.FromResult(AgentRunResult<ContextualLearningAgentOutput<TAction>>.Completed(
                output, started, _clock.GetUtcNow()));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Task.FromResult(AgentRunResult<ContextualLearningAgentOutput<TAction>>.Stopped(
                AgentRunStatus.Failed,
                new AgentFailure(AgentFailureCategory.InternalFailure, "contextual_learning_failed", ex.Message),
                started, _clock.GetUtcNow()));
        }
    }
}
