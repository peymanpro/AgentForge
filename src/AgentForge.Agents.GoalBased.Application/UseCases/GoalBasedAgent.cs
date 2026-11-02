using AgentForge.Abstractions.Execution;
using AgentForge.Agents.GoalBased.Domain.Models;

namespace AgentForge.Agents.GoalBased.Application.UseCases;

/// <summary>
/// Searches a finite state space with breadth-first search and returns a shortest
/// action sequence that satisfies the supplied goal predicate. It never executes actions.
/// </summary>
public sealed class GoalBasedAgent<TState, TGoal, TAction> :
    IAgent<GoalBasedInput<TState, TGoal>, GoalBasedOutput<TState, TAction>>
    where TState : notnull
    where TGoal : notnull
    where TAction : notnull
{
    private readonly Func<TState, IEnumerable<GoalBasedTransition<TState, TAction>>> _successors;
    private readonly Func<TState, TGoal, bool> _isGoal;
    private readonly GoalBasedAgentOptions _agentOptions;
    private readonly IEqualityComparer<TState> _stateComparer;
    private readonly TimeProvider _timeProvider;

    /// <summary>Creates a Goal-Based search agent from application-supplied transitions and a goal predicate.</summary>
    /// <param name="successors">Returns the ordered outgoing transitions for a state.</param>
    /// <param name="isGoal">Determines whether a state satisfies the requested goal.</param>
    /// <param name="agentOptions">Optional depth and explored-state limits.</param>
    /// <param name="stateComparer">Optional equality comparer used to identify visited states.</param>
    /// <param name="timeProvider">Optional clock used for run timestamps.</param>
    public GoalBasedAgent(
        Func<TState, IEnumerable<GoalBasedTransition<TState, TAction>>> successors,
        Func<TState, TGoal, bool> isGoal,
        GoalBasedAgentOptions? agentOptions = null,
        IEqualityComparer<TState>? stateComparer = null,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(successors);
        ArgumentNullException.ThrowIfNull(isGoal);
        _successors = successors;
        _isGoal = isGoal;
        _agentOptions = agentOptions ?? new GoalBasedAgentOptions();
        _stateComparer = stateComparer ?? EqualityComparer<TState>.Default;
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    /// <summary>Searches for a shortest goal-reaching action sequence within the configured limits.</summary>
    /// <param name="input">The initial state and target goal.</param>
    /// <param name="context">The identity and correlation context for this run.</param>
    /// <param name="options">The common execution limits.</param>
    /// <param name="cancellationToken">A token used to request cooperative cancellation.</param>
    /// <returns>A normalized run result containing the search outcome and optional plan.</returns>
    public Task<AgentRunResult<GoalBasedOutput<TState, TAction>>> RunAsync(
        GoalBasedInput<TState, TGoal> input,
        AgentRunContext context,
        AgentRunOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(options);
        var startedAt = _timeProvider.GetUtcNow();

        try { options.Validate(); }
        catch (ArgumentOutOfRangeException)
        {
            return Task.FromResult(AgentRunResult<GoalBasedOutput<TState, TAction>>.Stopped(
                AgentRunStatus.Rejected,
                new AgentFailure(AgentFailureCategory.InvalidInput, "goal_based.invalid_run_options",
                    "One or more common run limits are invalid."),
                startedAt, _timeProvider.GetUtcNow()));
        }

        if (cancellationToken.IsCancellationRequested)
            return Cancelled(startedAt);

        try
        {
            var initial = input.InitialState;
            if (_isGoal(initial, input.Goal))
            {
                var initialPlan = new GoalBasedPlan<TState, TAction>(
                    Array.Empty<TAction>(), new[] { initial }, 1, 0);
                return Completed(new GoalBasedOutput<TState, TAction>(
                    GoalBasedSearchStatus.GoalReached, initialPlan,
                    "The initial state already satisfies the goal."), startedAt);
            }

            var queue = new Queue<SearchNode>();
            var visited = new HashSet<TState>(_stateComparer) { initial };
            queue.Enqueue(new SearchNode(initial, null, default, 0));
            var explored = 0;
            var depthLimitReached = false;

            while (queue.Count > 0)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (explored >= _agentOptions.MaxExploredStates)
                {
                    return Completed(new GoalBasedOutput<TState, TAction>(
                        GoalBasedSearchStatus.SearchLimitReached, null,
                        $"Search stopped at the configured limit of {_agentOptions.MaxExploredStates} explored states."),
                        startedAt);
                }

                var current = queue.Dequeue();
                explored++;

                if (current.Depth >= _agentOptions.MaxDepth)
                {
                    depthLimitReached = true;
                    continue;
                }

                var transitions = _successors(current.State);
                if (transitions is null)
                    return Failed(startedAt, "goal_based.null_successors",
                        "The successor provider returned a null collection.");

                foreach (var transition in transitions)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (transition is null || transition.NextState is null || transition.Action is null)
                        return Failed(startedAt, "goal_based.invalid_transition",
                            "The successor provider returned a null transition, state, or action.");

                    if (!visited.Add(transition.NextState))
                        continue;

                    var next = new SearchNode(transition.NextState, current, transition.Action, current.Depth + 1);
                    if (_isGoal(next.State, input.Goal))
                    {
                        var plan = BuildPlan(next, explored);
                        return Completed(new GoalBasedOutput<TState, TAction>(
                            GoalBasedSearchStatus.GoalReached, plan,
                            $"A shortest goal-reaching plan was found at depth {plan.Depth} after exploring {explored} states."),
                            startedAt);
                    }

                    queue.Enqueue(next);
                }
            }

            var status = depthLimitReached
                ? GoalBasedSearchStatus.SearchLimitReached
                : GoalBasedSearchStatus.GoalNotReachable;
            var explanation = depthLimitReached
                ? $"No goal was found within the configured maximum depth of {_agentOptions.MaxDepth}."
                : $"The reachable state space was exhausted after exploring {explored} states.";
            return Completed(new GoalBasedOutput<TState, TAction>(status, null, explanation), startedAt);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(Cancelled(startedAt));
        }
        catch (Exception)
        {
            return Failed(startedAt, "goal_based.search_failed",
                "Goal evaluation or successor generation failed.");
        }
    }

    private static GoalBasedPlan<TState, TAction> BuildPlan(SearchNode goal, int explored)
    {
        var actions = new List<TAction>();
        var states = new List<TState> { goal.State };
        var node = goal;
        while (node.Parent is not null)
        {
            actions.Add(node.Action!);
            node = node.Parent;
            states.Add(node.State);
        }
        actions.Reverse();
        states.Reverse();
        return new GoalBasedPlan<TState, TAction>(actions.AsReadOnly(), states.AsReadOnly(), explored, goal.Depth);
    }

    private Task<AgentRunResult<GoalBasedOutput<TState, TAction>>> Completed(
        GoalBasedOutput<TState, TAction> output, DateTimeOffset startedAt) =>
        Task.FromResult(AgentRunResult<GoalBasedOutput<TState, TAction>>.Completed(
            output, startedAt, _timeProvider.GetUtcNow()));

    private Task<AgentRunResult<GoalBasedOutput<TState, TAction>>> Failed(
        DateTimeOffset startedAt, string code, string message) =>
        Task.FromResult(AgentRunResult<GoalBasedOutput<TState, TAction>>.Stopped(
            AgentRunStatus.Failed,
            new AgentFailure(AgentFailureCategory.InternalFailure, code, message),
            startedAt, _timeProvider.GetUtcNow()));

    private Task<AgentRunResult<GoalBasedOutput<TState, TAction>>> Cancelled(DateTimeOffset startedAt) =>
        Task.FromResult(AgentRunResult<GoalBasedOutput<TState, TAction>>.Stopped(
            AgentRunStatus.Cancelled,
            new AgentFailure(AgentFailureCategory.Cancelled, "goal_based.cancelled",
                "The caller cancelled the search."),
            startedAt, _timeProvider.GetUtcNow()));

    private sealed record SearchNode(TState State, SearchNode? Parent, TAction? Action, int Depth);
}
