using AgentForge.Abstractions.Execution;
using AgentForge.Agents.BDI.Models;

namespace AgentForge.Agents.BDI;

internal sealed class BdiAgent<TBelief, TDesire, TIntention, TObservation, TAction>(
    Func<IReadOnlyList<TBelief>, TObservation, IReadOnlyList<TBelief>> beliefRevision,
    Func<IReadOnlyList<TBelief>, IReadOnlyList<TDesire>, IReadOnlyList<TDesire>> desireSelection,
    Func<TDesire, TIntention> intentionFactory,
    Func<IReadOnlyList<TBelief>, TIntention, bool> commitmentPolicy,
    Func<IReadOnlyList<TBelief>, IReadOnlyList<TIntention>, BdiAction<TAction>> actionSelection,
    int maxIntentions,
    TimeProvider timeProvider)
    : IAgent<
        BdiAgentInput<TBelief, TDesire, TIntention, TObservation>,
        BdiAgentOutput<TBelief, TDesire, TIntention, TAction>>
    where TBelief : notnull
    where TDesire : notnull
    where TIntention : notnull
    where TObservation : notnull
    where TAction : notnull
{
    public Task<AgentRunResult<BdiAgentOutput<TBelief, TDesire, TIntention, TAction>>> RunAsync(
        BdiAgentInput<TBelief, TDesire, TIntention, TObservation> input,
        AgentRunContext context,
        AgentRunOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(input.Beliefs);
        ArgumentNullException.ThrowIfNull(input.AvailableDesires);
        ArgumentNullException.ThrowIfNull(input.CurrentIntentions);
        options.Validate();

        var startedAt = timeProvider.GetUtcNow();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var beliefs = beliefRevision(input.Beliefs.ToArray(), input.Observation)
                ?? throw new InvalidOperationException("Belief revision returned null.");
            cancellationToken.ThrowIfCancellationRequested();

            var eligibleDesires = desireSelection(beliefs, input.AvailableDesires.ToArray())
                ?? throw new InvalidOperationException("Desire selection returned null.");
            cancellationToken.ThrowIfCancellationRequested();

            var intentions = input.CurrentIntentions
                .Where(intention => commitmentPolicy(beliefs, intention))
                .Distinct()
                .Take(maxIntentions)
                .ToList();

            foreach (var desire in eligibleDesires)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var intention = intentionFactory(desire)
                    ?? throw new InvalidOperationException("The intention factory returned null.");

                if (intentions.Contains(intention))
                    continue;
                if (intentions.Count >= maxIntentions)
                    break;

                intentions.Add(intention);
            }

            var action = actionSelection(beliefs, intentions.AsReadOnly())
                ?? throw new InvalidOperationException("Action selection returned null.");

            var output = new BdiAgentOutput<TBelief, TDesire, TIntention, TAction>(
                Array.AsReadOnly(beliefs.ToArray()),
                Array.AsReadOnly(eligibleDesires.ToArray()),
                intentions.AsReadOnly(),
                action);

            return Task.FromResult(AgentRunResult<BdiAgentOutput<TBelief, TDesire, TIntention, TAction>>.Completed(
                output, startedAt, timeProvider.GetUtcNow()));
        }
        catch (OperationCanceledException)
        {
            return Task.FromResult(AgentRunResult<BdiAgentOutput<TBelief, TDesire, TIntention, TAction>>.Stopped(
                AgentRunStatus.Cancelled,
                new AgentFailure(
                    AgentFailureCategory.Cancelled,
                    "bdi.cancelled",
                    "The BDI deliberation cycle was cancelled."),
                startedAt,
                timeProvider.GetUtcNow()));
        }
        catch (Exception)
        {
            return Task.FromResult(AgentRunResult<BdiAgentOutput<TBelief, TDesire, TIntention, TAction>>.Stopped(
                AgentRunStatus.Failed,
                new AgentFailure(
                    AgentFailureCategory.InternalFailure,
                    "bdi.deliberation_failed",
                    "A BDI strategy failed during belief revision, desire selection, commitment, or action selection."),
                startedAt,
                timeProvider.GetUtcNow()));
        }
    }
}