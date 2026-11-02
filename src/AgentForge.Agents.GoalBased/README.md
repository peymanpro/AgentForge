# AgentForge.Agents.GoalBased

A standalone Goal-Based agent module that searches a state space for an action sequence satisfying an explicit goal predicate. The initial implementation uses bounded breadth-first search (BFS), returning a shortest plan when one is found. It proposes actions; it does not execute them.

## Installation

After this package is published to NuGet.org:

```bash
dotnet add package AgentForge.Agents.GoalBased
```

## Quick Start

This example searches a small route graph. Each edge is an available action that moves the system to another state:

```csharp
using AgentForge.Agents.GoalBased.Builders;
using AgentForge.Agents.GoalBased.Domain.Models;

var routes = new Dictionary<string, GoalBasedTransition<string, string>[]>
{
    ["Home"] =
    [
        new("Go to station", "Station"),
        new("Go to park", "Park")
    ],
    ["Station"] = [new("Take the train", "Museum")],
    ["Park"] = [new("Walk to museum", "Museum")],
    ["Museum"] = []
};

var agent = GoalBasedAgentBuilder
    .Create<string, string, string>()
    .WithSuccessors(state => routes[state])
    .WithGoalPredicate((state, goal) => state == goal)
    .WithLimits(maxDepth: 8, maxExploredStates: 100)
    .Build();

var result = await agent.RunAsync(
    new GoalBasedInput<string, string>("Home", "Museum"),
    cancellationToken);

if (result.Output?.Plan is { } plan)
{
    Console.WriteLine(string.Join(" -> ", plan.Actions));
    Console.WriteLine($"Depth: {plan.Depth}; explored: {plan.ExploredStateCount}");
}
```

## Search behavior

- Uses breadth-first search; with unit-cost transitions, the first found plan has the fewest actions.
- Tests the initial state before expanding successors.
- Expands successors in the order returned by the application-supplied provider.
- Uses state equality to avoid revisiting already discovered states. Supply a comparer when default equality is not appropriate.
- Returns one of three outcomes: `GoalReached`, `GoalNotReachable`, or `SearchLimitReached`.
- Search is bounded by both maximum depth and maximum explored states.
- Cancellation is checked during traversal and successor enumeration.
- The result contains the ordered actions and corresponding state path.
- The agent proposes a plan only; the host is responsible for authorization, persistence, and action execution.

## Configuration

- `WithSuccessors`: maps a state to its possible action/next-state transitions.
- `WithGoalPredicate`: determines whether a state satisfies the supplied goal.
- `WithLimits`: sets maximum depth and explored-state count (defaults: 32 and 10,000).
- `WithStateComparer`: supplies domain-specific state equality.
- `WithTimeProvider`: supplies a clock for deterministic timestamps and tests.

## Boundaries and limitations

This module is a deterministic, single-agent, single-objective search architecture. It does not generate plans with an LLM, execute actions, assign utility scores, learn a transition model, or provide heuristic/A* search. Transition costs are treated as equal; BFS is therefore not suitable when actions have different costs. Use the Planning module for validated step-handler execution, and Utility-Based for selecting among actions by preference score.
