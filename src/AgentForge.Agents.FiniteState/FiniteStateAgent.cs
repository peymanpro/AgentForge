using AgentForge.Abstractions.Execution;
using AgentForge.Agents.FiniteState.Models;

namespace AgentForge.Agents.FiniteState;

internal sealed record StateTransition<TState, TEvent, TAction>(
    string Name,
    Func<TState, TEvent, bool> Guard,
    Func<TState, TEvent, TState> NextState,
    Func<TState, TEvent, TAction> ActionFactory)
    where TState : notnull
    where TEvent : notnull
    where TAction : notnull;

internal sealed class FiniteStateAgent<TState, TEvent, TAction>(
    IReadOnlyList<StateTransition<TState, TEvent, TAction>> transitions,
    TimeProvider timeProvider)
    : IAgent<FiniteStateAgentInput<TState, TEvent>, FiniteStateAgentOutput<TState, TAction>>
    where TState : notnull
    where TEvent : notnull
    where TAction : notnull
{
    public Task<AgentRunResult<FiniteStateAgentOutput<TState, TAction>>> RunAsync(
        FiniteStateAgentInput<TState, TEvent> input,
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
            cancellationToken.ThrowIfCancellationRequested();

            foreach (var transition in transitions)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (!transition.Guard(input.CurrentState, input.Event))
                    continue;

                var output = new FiniteStateAgentOutput<TState, TAction>(
                    transition.NextState(input.CurrentState, input.Event),
                    true,
                    transition.Name,
                    transition.ActionFactory(input.CurrentState, input.Event));

                return Task.FromResult(AgentRunResult<FiniteStateAgentOutput<TState, TAction>>.Completed(
                    output, startedAt, timeProvider.GetUtcNow()));
            }

            return Task.FromResult(AgentRunResult<FiniteStateAgentOutput<TState, TAction>>.Completed(
                new FiniteStateAgentOutput<TState, TAction>(input.CurrentState, false, null, default),
                startedAt,
                timeProvider.GetUtcNow()));
        }
        catch (OperationCanceledException)
        {
            return Task.FromResult(AgentRunResult<FiniteStateAgentOutput<TState, TAction>>.Stopped(
                AgentRunStatus.Cancelled,
                new AgentFailure(
                    AgentFailureCategory.Cancelled,
                    "finite_state.cancelled",
                    "The finite-state transition run was cancelled."),
                startedAt,
                timeProvider.GetUtcNow()));
        }
        catch (Exception)
        {
            return Task.FromResult(AgentRunResult<FiniteStateAgentOutput<TState, TAction>>.Stopped(
                AgentRunStatus.Failed,
                new AgentFailure(
                    AgentFailureCategory.InternalFailure,
                    "finite_state.transition_failed",
                    "A finite-state guard or transition failed."),
                startedAt,
                timeProvider.GetUtcNow()));
        }
    }
}