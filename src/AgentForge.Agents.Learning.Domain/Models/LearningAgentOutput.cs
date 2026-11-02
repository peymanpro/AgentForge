namespace AgentForge.Agents.Learning.Domain.Models;
/// <summary>Selected action and statistics used for the decision.</summary>
/// <typeparam name="TAction">The action identity type.</typeparam>
/// <param name="SelectedAction">Action selected by the policy.</param>
/// <param name="WasExploration">Whether the policy selected through exploration.</param>
/// <param name="TotalObservations">Total feedback observations available to the policy.</param>
/// <param name="EstimatedReward">Mean reward estimate for the selected action.</param>
public sealed record LearningAgentOutput<TAction>(TAction SelectedAction, bool WasExploration, long TotalObservations, double EstimatedReward) where TAction : notnull;