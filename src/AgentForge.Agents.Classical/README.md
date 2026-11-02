# AgentForge.Agents.Classical

A deterministic starting point for classical AI agent architectures in AgentForge.

## Implemented architectures

### Simple-reflex agent

`SimpleReflexAgent<TPercept, TAction>` evaluates ordered condition-action rules against the current percept and selects the action produced by the first matching rule.

- Rule order is explicit and deterministic.
- A run with no matching rule is a successful no-action decision, not an execution failure.
- The selected action is a proposal. The agent does not execute external side effects.
- Rule evaluation failures are returned as normalized agent failures.

### Model-based reflex agent

`ModelBasedReflexAgent<TPercept, TState, TAction>` updates an internal world-model state from the previous state and latest percept, then applies ordered rules to that updated state.

The state is passed into and returned from each run rather than stored in mutable fields on the agent instance. This makes state ownership visible to the application, supports persistence between runs, and avoids state leakage when one agent instance serves concurrent callers.

## Example

```csharp
using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Classical;

var rules = new[]
{
    new ReflexRule<int, string>("too-cold", temperature => temperature < 19, _ => "heat"),
    new ReflexRule<int, string>("too-hot", temperature => temperature > 23, _ => "cool"),
};

var agent = new SimpleReflexAgent<int, string>(rules);
var result = await agent.RunAsync(
    input: 27,
    context: AgentRunContext.Create(),
    options: new AgentRunOptions());

if (result.Status == AgentRunStatus.Completed && result.Output?.Decision.HasAction == true)
{
    Console.WriteLine(result.Output.Decision.Action);
}
```

The application remains responsible for validating and executing the proposed action against its environment and permissions.

## Run behavior

Both agents implement the shared `IAgent<TInput, TOutput>` contract and return `AgentRunResult<TOutput>`.

- Run options are validated before evaluating rules.
- Cancellation is cooperative and checked before each rule.
- The first matching rule wins; ties are therefore resolved by declared rule order.
- No match yields `ClassicalDecision.NoAction`.
- Unexpected state-update or rule exceptions are converted to generic internal failures; exception contents are not exposed in the result.
- This initial slice is a single-decision step. It does not own an environment loop, persist state, or implement search/planning algorithms.

## Next classical families

The catalog and roadmap include goal-based and utility-based agents, production systems, state-space search, behavior trees, finite-state controllers, subsumption, BDI, adversarial search, bandits, and reinforcement learning. They will be added as separate, tested capabilities when their contracts are defined. An algorithm such as A* or minimax will not be mislabeled as a complete agent.
