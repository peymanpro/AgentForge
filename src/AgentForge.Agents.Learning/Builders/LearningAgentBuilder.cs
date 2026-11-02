using AgentForge.Abstractions.Execution;
using AgentForge.Agents.Learning.Domain.Models;
using AgentForge.Agents.Learning.Domain.Policies;
using AgentForge.Agents.Learning.Policies;
namespace AgentForge.Agents.Learning.Builders;
/// <summary>Fluent construction entry point for a Learning Agent.</summary>
public static class LearningAgentBuilder
{
    public static LearningAgentBuilder<TAction> Create<TAction>() where TAction : notnull => new();
}
/// <summary>Configures the learning policy and execution clock.</summary>
public sealed class LearningAgentBuilder<TAction> where TAction : notnull
{
    private ILearningPolicy<TAction> _policy = new EpsilonGreedyLearningPolicy<TAction>();
    private TimeProvider? _clock;
    public LearningAgentBuilder<TAction> WithPolicy(ILearningPolicy<TAction> policy) { _policy = policy ?? throw new ArgumentNullException(nameof(policy)); return this; }
    public LearningAgentBuilder<TAction> WithEpsilonGreedy(double epsilon = 0.1, Func<double>? nextDouble = null, Func<int, int>? nextIndex = null) { _policy = new EpsilonGreedyLearningPolicy<TAction>(epsilon, nextDouble, nextIndex); return this; }
    public LearningAgentBuilder<TAction> WithTimeProvider(TimeProvider timeProvider) { _clock = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider)); return this; }
    public AgentHandle<LearningAgentInput<TAction>, LearningAgentOutput<TAction>> Build() => new(new LearningAgent<TAction>(_policy, _clock));
}