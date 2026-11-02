namespace AgentForge.Agents.GoalBased.Application.UseCases;

/// <summary>Resource limits for a Goal-Based breadth-first search.</summary>
public sealed class GoalBasedAgentOptions
{
    /// <summary>Creates validated limits for maximum search depth and explored states.</summary>
    /// <param name="maxDepth">Maximum number of transitions from the initial state.</param>
    /// <param name="maxExploredStates">Maximum number of dequeued states.</param>
    public GoalBasedAgentOptions(int maxDepth = 32, int maxExploredStates = 10_000)
    {
        if (maxDepth < 0) throw new ArgumentOutOfRangeException(nameof(maxDepth));
        if (maxExploredStates < 1) throw new ArgumentOutOfRangeException(nameof(maxExploredStates));
        MaxDepth = maxDepth;
        MaxExploredStates = maxExploredStates;
    }

    /// <summary>Gets the maximum number of transitions from the initial state.</summary>
    public int MaxDepth { get; }

    /// <summary>Gets the maximum number of dequeued states.</summary>
    public int MaxExploredStates { get; }
}
