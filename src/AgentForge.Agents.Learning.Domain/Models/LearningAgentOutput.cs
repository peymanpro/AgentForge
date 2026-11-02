namespace AgentForge.Agents.Learning.Domain.Models;
/// <summary>Selected action and statistics used for the decision.</summary>
public sealed record LearningAgentOutput<TAction>(TAction SelectedAction, bool WasExploration, long TotalObservations, double EstimatedReward) where TAction : notnull;