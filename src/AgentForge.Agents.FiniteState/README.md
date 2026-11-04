# AgentForge.Agents.FiniteState

A standalone deterministic finite-state agent. The host supplies the current state and one event; the agent evaluates ordered guards, proposes the first matching transition and action, and returns the next state. State persistence and event delivery remain host responsibilities.

## Installation

After publication to NuGet.org:

\`\`\`bash
dotnet add package AgentForge.Agents.FiniteState
\`\`\`

## Quick Start

\`\`\`csharp
using AgentForge.Agents.FiniteState.Builders;
using AgentForge.Agents.FiniteState.Models;

var agent = FiniteStateAgentBuilder.Create<DoorState, DoorEvent, DoorAction>()
    .When(
        "open-door",
        (state, trigger) => state == DoorState.Closed && trigger == DoorEvent.Open,
        (_, _) => DoorState.Open,
        (_, _) => DoorAction.UnlockAndOpen)
    .When(
        "close-door",
        (state, trigger) => state == DoorState.Open && trigger == DoorEvent.Close,
        (_, _) => DoorState.Closed,
        (_, _) => DoorAction.CloseAndLock)
    .Build();

var result = await agent.RunAsync(
    new FiniteStateAgentInput<DoorState, DoorEvent>(DoorState.Closed, DoorEvent.Open),
    cancellationToken);

Console.WriteLine(result.Output?.CurrentState);
Console.WriteLine(result.Output?.Action);

public enum DoorState { Closed, Open }
public enum DoorEvent { Open, Close }
public enum DoorAction { UnlockAndOpen, CloseAndLock }
\`\`\`

## Execution semantics

- One run processes one event and performs at most one transition.
- Guards are evaluated in registration order; the first matching transition wins.
- If no guard matches, the current state is returned unchanged with no action.
- The next state and action are proposals; the host decides whether to persist state and perform the action.
- State is supplied on every run, so a configured agent instance retains no per-session mutable state.
- Cancellation and transition failures use the shared normalized run result.

## Fluent configuration

- \`Create<TState,TEvent,TAction>()\` creates a typed builder.
- \`When(name, guard, nextState, actionFactory)\` registers a transition.
- \`WithTimeProvider(provider)\` configures timestamps.
- \`Build()\` requires at least one transition and returns the shared \`AgentHandle\`.

## Boundaries

This module is a finite-state decision controller, not a workflow engine or a persistent state-machine host. It does not schedule events, execute external side effects, persist state, or define retry semantics. Use the host application or a dedicated workflow capability for those responsibilities.