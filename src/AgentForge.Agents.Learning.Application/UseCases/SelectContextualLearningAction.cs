using AgentForge.Agents.Learning.Domain.Models;

namespace AgentForge.Agents.Learning.Application.UseCases;

/// <summary>Selects the candidate with the highest disjoint LinUCB upper-confidence score.</summary>
public sealed class SelectContextualLearningAction<TAction> where TAction : notnull
{
    private readonly double _explorationCoefficient;

    /// <summary>Creates a LinUCB selector with a finite, non-negative exploration coefficient.</summary>
    public SelectContextualLearningAction(double explorationCoefficient = 1.0)
    {
        if (!double.IsFinite(explorationCoefficient) || explorationCoefficient < 0)
            throw new ArgumentOutOfRangeException(nameof(explorationCoefficient));
        _explorationCoefficient = explorationCoefficient;
    }

    /// <summary>Scores each candidate and returns the first candidate with the maximum score.</summary>
    public ContextualLearningAgentOutput<TAction> Execute(ContextualLearningAgentInput<TAction> input)
    {
        ArgumentNullException.ThrowIfNull(input);
        TAction? selected = default;
        var bestScore = double.NegativeInfinity;
        var bestReward = 0.0;
        var bestBonus = 0.0;

        foreach (var action in input.Candidates)
        {
            var statistics = input.State.GetStatistics(action);
            var lower = CholeskyFactorization.Factor(statistics.CopyDesignMatrix());
            var theta = CholeskyFactorization.Solve(lower, statistics.CopyRewardVector());
            var inverseContext = CholeskyFactorization.Solve(lower, input.Context);
            var estimatedReward = Dot(input.Context, theta);
            var variance = Dot(input.Context, inverseContext);
            if (!double.IsFinite(estimatedReward) || !double.IsFinite(variance) || variance < -1e-10)
                throw new ArithmeticException("LinUCB produced an invalid score.");
            var bonus = _explorationCoefficient * Math.Sqrt(Math.Max(0, variance));
            var score = estimatedReward + bonus;
            if (!double.IsFinite(score)) throw new ArithmeticException("LinUCB produced a non-finite score.");

            if (score > bestScore)
            {
                selected = action;
                bestScore = score;
                bestReward = estimatedReward;
                bestBonus = bonus;
            }
        }

        return new ContextualLearningAgentOutput<TAction>(selected!, bestReward, bestBonus, bestScore,
            input.State.TotalObservations);
    }

    private static double Dot(IReadOnlyList<double> left, IReadOnlyList<double> right)
    {
        var sum = 0.0;
        for (var index = 0; index < left.Count; index++) sum += left[index] * right[index];
        return sum;
    }
}
