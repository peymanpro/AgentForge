namespace AgentForge.Agents.Subsumption.Models;

/// <summary>Supplies the current percept to a layered reactive agent.</summary>
public sealed record SubsumptionAgentInput<TPercept>(TPercept Percept)
    where TPercept : notnull;

/// <summary>Describes the behavior selected by priority arbitration.</summary>
public sealed record SubsumptionAgentOutput<TAction>(
    bool HasAction,
    TAction? Action,
    string? WinningLayer,
    int? WinningPriority)
    where TAction : notnull;