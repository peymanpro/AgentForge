namespace AgentForge.Agents.Learning.Domain.Models;

/// <summary>Selected action and LinUCB score diagnostics for a contextual decision.</summary>
public sealed record ContextualLearningAgentOutput<TAction>(
    TAction SelectedAction,
    double EstimatedReward,
    double ExplorationBonus,
    double UpperConfidenceScore,
    long TotalObservations) where TAction : notnull;
