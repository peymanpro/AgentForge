# AgentForge.Agents.Learning

A standalone, feedback-driven Learning Agent module. It selects candidate actions using learned reward statistics and updates its behavior when the host supplies outcome feedback.

## Quick Start

~~~csharp
using AgentForge.Agents.Learning.Builders;
using AgentForge.Agents.Learning.Domain.Models;

var agent = LearningAgentBuilder.Create<string>().WithEpsilonGreedy(epsilon: 0.1).Build();
var state = new LearningState<string>();

for (var round = 0; round < 1_000; round++)
{
    var decision = await agent.RunAsync(new LearningAgentInput<string>(
        ["search-primary", "search-fallback"], state), cancellationToken);
    var selected = decision.Output!.SelectedAction;

    // The host executes the action and evaluates its outcome.
    var reward = selected == "search-primary" ? 1.0 : 0.2;
    state = state.Observe(selected, reward);
}
~~~

## Learning model

The initial implementation uses a stationary multi-armed bandit model:

- **Performance element:** selects an action using current estimates.
- **Learning element:** updates the selected action's running mean reward.
- **Critic:** the host evaluates the outcome and supplies a finite numeric reward.
- **Problem generator:** epsilon-greedy exploration occasionally selects a non-greedy action.

The sample mean is updated incrementally:

~~~text
Q(n) = Q(n-1) + (r(n) - Q(n-1)) / n
~~~

The policy tries unseen candidates first, then explores with probability epsilon and otherwise selects the highest estimated mean reward.

## State and feedback boundaries

- LearningState is immutable and contains action counts and average rewards.
- The host owns state persistence, reward design, action execution, and authorization.
- The host can persist the statistics snapshot using its preferred storage format.
- Observe(action, reward) returns a new state; it never mutates the previous snapshot.
- Rewards must be finite. Their scale and meaning are domain-specific.
- The initial model assumes stationary action rewards and does not model context-dependent outcomes.

## Extension points and limitations

- Implement ILearningPolicy<TAction> to add UCB, Thompson Sampling, or another selection policy.
- Supply a custom policy through WithPolicy.
- No LLM, database, storage SDK, or external dependency is required.
- This first version is a single-state, stationary bandit learner, not a general Markov Decision Process or deep reinforcement-learning implementation.

## Installation

After publication to NuGet.org:

~~~bash
dotnet add package AgentForge.Agents.Learning
~~~

## Architecture

- **Domain:** immutable learning state, action statistics, and policy contract.
- **Application:** action selection use case.
- **Public API:** fluent builder and common AgentForge execution handle.
- **Infrastructure:** intentionally left to the host application.
