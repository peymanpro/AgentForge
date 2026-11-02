namespace AgentForge.Agents.Learning.Domain.Models;
/// <summary>An action with its observed reward estimate.</summary>
public sealed record ActionEstimate<TAction>(TAction Action, long ObservationCount, double AverageReward) where TAction : notnull;