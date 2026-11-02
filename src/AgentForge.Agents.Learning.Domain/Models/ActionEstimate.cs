namespace AgentForge.Agents.Learning.Domain.Models;
/// <summary>An action with its observed reward estimate.</summary>
/// <typeparam name="TAction">The action identity type.</typeparam>
/// <param name="Action">Candidate action.</param>
/// <param name="ObservationCount">Number of observed outcomes for the action.</param>
/// <param name="AverageReward">Mean reward observed for the action.</param>
public sealed record ActionEstimate<TAction>(TAction Action, long ObservationCount, double AverageReward) where TAction : notnull;