using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Subsumption.Models;

namespace AgentForge.Agents.Subsumption;

internal sealed record ReactiveLayer<TPercept, TAction>(
    string Name,
    int Priority,
    Func<TPercept, bool> Condition,
    Func<TPercept, TAction> ActionFactory)
    where TPercept : notnull
    where TAction : notnull;

internal sealed class SubsumptionAgent<TPercept, TAction>(
    IReadOnlyList<ReactiveLayer<TPercept, TAction>> layers,
    TimeProvider timeProvider)
    : IAgent<SubsumptionAgentInput<TPercept>, SubsumptionAgentOutput<TAction>>
    where TPercept : notnull
    where TAction : notnull
{
    public Task<AgentRunResult<SubsumptionAgentOutput<TAction>>> RunAsync(
        SubsumptionAgentInput<TPercept> input,
        AgentRunContext context,
        AgentRunOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();

        var startedAt = timeProvider.GetUtcNow();

        try
        {
            foreach (var layer in layers)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!layer.Condition(input.Percept))
                    continue;

                var output = new SubsumptionAgentOutput<TAction>(
                    true,
                    layer.ActionFactory(input.Percept),
                    layer.Name,
                    layer.Priority);

                return Task.FromResult(AgentRunResult<SubsumptionAgentOutput<TAction>>.Completed(
                    output, startedAt, timeProvider.GetUtcNow()));
            }

            return Task.FromResult(AgentRunResult<SubsumptionAgentOutput<TAction>>.Completed(
                new SubsumptionAgentOutput<TAction>(false, default, null, null),
                startedAt,
                timeProvider.GetUtcNow()));
        }
        catch (OperationCanceledException)
        {
            return Task.FromResult(AgentRunResult<SubsumptionAgentOutput<TAction>>.Stopped(
                AgentRunStatus.Cancelled,
                new AgentFailure(
                    AgentFailureCategory.Cancelled,
                    "subsumption.cancelled",
                    "The layered reactive run was cancelled."),
                startedAt,
                timeProvider.GetUtcNow()));
        }
        catch (Exception)
        {
            return Task.FromResult(AgentRunResult<SubsumptionAgentOutput<TAction>>.Stopped(
                AgentRunStatus.Failed,
                new AgentFailure(
                    AgentFailureCategory.InternalFailure,
                    "subsumption.layer_failed",
                    "A reactive layer failed during arbitration."),
                startedAt,
                timeProvider.GetUtcNow()));
        }
    }
}