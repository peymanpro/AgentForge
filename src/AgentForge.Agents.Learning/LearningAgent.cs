using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Learning.Application.UseCases;
using AgentForge.Agents.Learning.Domain.Models;
using AgentForge.Agents.Learning.Domain.Policies;
namespace AgentForge.Agents.Learning;
/// <summary>A stateless decision component; the host supplies and persists immutable learned state.</summary>
public sealed class LearningAgent<TAction> : IAgent<LearningAgentInput<TAction>, LearningAgentOutput<TAction>> where TAction : notnull
{
    private readonly SelectLearningAction<TAction> _selection;
    private readonly TimeProvider _clock;
    public LearningAgent(ILearningPolicy<TAction> policy, TimeProvider? timeProvider = null)
    {
        _selection = new SelectLearningAction<TAction>(policy ?? throw new ArgumentNullException(nameof(policy)));
        _clock = timeProvider ?? TimeProvider.System;
    }
    public Task<AgentRunResult<LearningAgentOutput<TAction>>> RunAsync(LearningAgentInput<TAction> input, AgentRunContext context, AgentRunOptions options, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(options);
        var started = _clock.GetUtcNow();
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(AgentRunResult<LearningAgentOutput<TAction>>.Stopped(AgentRunStatus.Cancelled, new AgentFailure("cancelled", "The learning decision was cancelled."), started, _clock.GetUtcNow()));
        try
        {
            var output = _selection.Execute(input);
            return Task.FromResult(AgentRunResult<LearningAgentOutput<TAction>>.Completed(output, started, _clock.GetUtcNow()));
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            return Task.FromResult(AgentRunResult<LearningAgentOutput<TAction>>.Stopped(AgentRunStatus.Failed, new AgentFailure("learning_decision_failed", ex.Message), started, _clock.GetUtcNow()));
        }
    }
}