namespace AgentForge.Agents.FiniteState.Models;

/// <summary>Supplies the current state and one event to a finite-state agent.</summary>
public sealed record FiniteStateAgentInput<TState, TEvent>(TState CurrentState, TEvent Event)
    where TState : notnull
    where TEvent : notnull;

/// <summary>Describes the single state transition selected for an input event.</summary>
public sealed record FiniteStateAgentOutput<TState, TAction>(
    TState CurrentState,
    bool Transitioned,
    string? TransitionName,
    TAction? Action)
    where TState : notnull
    where TAction : notnull;