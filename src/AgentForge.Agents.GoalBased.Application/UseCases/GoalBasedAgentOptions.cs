namespace AgentForge.Agents.GoalBased.Application.UseCases;

/// <summary>Resource limits for a Goal-Based breadth-first search.</summary>
public sealed class GoalBasedAgentOptions
{
    public GoalBasedAgentOptions(int maxDepth = 32, int maxExploredStates = 10_000)
    {
        if (maxDepth < 0) throw new ArgumentOutOfRangeException(nameof(maxDepth));
        if (maxExploredStates < 1) throw new ArgumentOutOfRangeException(nameof(maxExploredStates));
        MaxDepth = maxDepth;
        MaxExploredStates = maxExploredStates;
    }

    public int MaxDepth { get; }
    public int MaxExploredStates { get; }
}
