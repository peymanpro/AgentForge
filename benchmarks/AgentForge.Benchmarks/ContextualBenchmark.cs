using AgentForge.Agents.Learning.Application.UseCases;
using AgentForge.Agents.Learning.Domain.Models;
using System.Diagnostics;
using System.Globalization;

namespace AgentForge.Benchmarks;

internal static class ContextualBenchmark
{
    private const int ArmCount = 5;
    private const int FeatureCount = 5;
    private const double NoiseStandardDeviation = 0.2;
    private const double Regularization = 1.0;
    private const double LinUcbAlpha = 0.4;

    public static async Task RunAsync(string outputDirectory, int seedCount, int horizon)
    {
        var results = new List<ContextualRunResult>();
        for (var seed = 1; seed <= seedCount; seed++)
        {
            results.Add(RunLinUcb(seed, horizon));
            results.Add(RunLinearThompsonSampling(seed, horizon));
        }

        await File.WriteAllTextAsync(
            Path.Combine(outputDirectory, "contextual-bandit-runs.csv"),
            WriteRaw(results), new System.Text.UTF8Encoding(false));
        await File.WriteAllTextAsync(
            Path.Combine(outputDirectory, "contextual-bandit-summary.csv"),
            WriteSummary(results), new System.Text.UTF8Encoding(false));
    }

    private static ContextualRunResult RunLinUcb(int seed, int horizon)
    {
        var environment = CreateEnvironment(seed, horizon);
        var selector = new SelectContextualLearningAction<int>(LinUcbAlpha);
        var state = new ContextualLearningState<int>(FeatureCount, Regularization);
        var metrics = new RunMetrics(horizon);

        for (var round = 0; round < horizon; round++)
        {
            var context = environment.Contexts[round];
            var input = new ContextualLearningAgentInput<int>(
                Enumerable.Range(0, ArmCount), context, state);
            var started = Stopwatch.GetTimestamp();
            var decision = selector.Execute(input);
            metrics.Latencies[round] = Stopwatch.GetElapsedTime(started).TotalNanoseconds;

            var action = decision.SelectedAction;
            Observe(metrics, environment, state, round, action);
            state = state.Observe(action, context, environment.Rewards[action][round]);
        }

        return metrics.ToResult(seed, horizon, "LinUCB");
    }

    private static ContextualRunResult RunLinearThompsonSampling(int seed, int horizon)
    {
        var environment = CreateEnvironment(seed, horizon);
        var policyRandom = new Random(unchecked(seed * 32452843 + 97));
        var state = new ContextualLearningState<int>(FeatureCount, Regularization);
        var metrics = new RunMetrics(horizon);

        for (var round = 0; round < horizon; round++)
        {
            var context = environment.Contexts[round];
            var started = Stopwatch.GetTimestamp();
            var selected = 0;
            var bestSampledReward = double.NegativeInfinity;

            for (var action = 0; action < ArmCount; action++)
            {
                var statistics = state.GetStatistics(action);
                var lower = Cholesky(statistics.DesignMatrix);
                var mean = Solve(lower, statistics.RewardVector);
                var standardNormal = Enumerable.Range(0, FeatureCount)
                    .Select(_ => NextStandardNormal(policyRandom)).ToArray();
                var perturbation = SolveUpperTranspose(lower, standardNormal);
                var sampledReward = Dot(context, mean) +
                    NoiseStandardDeviation * Dot(context, perturbation);
                if (sampledReward > bestSampledReward)
                {
                    bestSampledReward = sampledReward;
                    selected = action;
                }
            }

            metrics.Latencies[round] = Stopwatch.GetElapsedTime(started).TotalNanoseconds;
            Observe(metrics, environment, state, round, selected);
            state = state.Observe(selected, context, environment.Rewards[selected][round]);
        }

        return metrics.ToResult(seed, horizon, "LinearThompsonSampling");
    }

    private static void Observe(RunMetrics metrics, EnvironmentData environment,
        ContextualLearningState<int> state, int round, int action)
    {
        var expected = Dot(environment.Contexts[round], environment.Parameters[action]);
        var oracle = Enumerable.Range(0, ArmCount)
            .Max(candidate => Dot(environment.Contexts[round], environment.Parameters[candidate]));
        var reward = environment.Rewards[action][round];
        metrics.CumulativeReward += reward;
        metrics.CumulativePseudoRegret += oracle - expected;
        if (Math.Abs(expected - oracle) < 1e-10) metrics.OptimalSelections++;
    }

    private static EnvironmentData CreateEnvironment(int seed, int horizon)
    {
        var parameterRandom = new Random(unchecked(seed * 65537 + 11));
        var contextRandom = new Random(unchecked(seed * 65537 + 23));
        var parameters = new double[ArmCount][];
        for (var action = 0; action < ArmCount; action++)
            parameters[action] = Enumerable.Range(0, FeatureCount)
                .Select(_ => 0.25 * NextStandardNormal(parameterRandom)).ToArray();

        var contexts = new double[horizon][];
        for (var round = 0; round < horizon; round++)
        {
            contexts[round] = new double[FeatureCount];
            contexts[round][0] = 1.0;
            for (var feature = 1; feature < FeatureCount; feature++)
                contexts[round][feature] = NextStandardNormal(contextRandom);
        }

        // Independent, reproducible reward streams per action; both policies see the same
        // action-specific sequence whenever they select that action.
        var rewards = new double[ArmCount][];
        for (var action = 0; action < ArmCount; action++)
        {
            var random = new Random(unchecked(seed * 99991 + action * 7919 + 31));
            rewards[action] = new double[horizon];
            for (var round = 0; round < horizon; round++)
                rewards[action][round] = Dot(contexts[round], parameters[action]) +
                    NoiseStandardDeviation * NextStandardNormal(random);
        }

        return new EnvironmentData(parameters, contexts, rewards);
    }

    private static double[,] Cholesky(double[,] matrix)
    {
        var size = matrix.GetLength(0);
        var lower = new double[size, size];
        for (var row = 0; row < size; row++)
        for (var column = 0; column <= row; column++)
        {
            var value = matrix[row, column];
            for (var k = 0; k < column; k++)
                value -= lower[row, k] * lower[column, k];
            if (row == column)
            {
                if (!double.IsFinite(value) || value <= 1e-12)
                    throw new ArithmeticException("Posterior precision matrix is not positive definite.");
                lower[row, column] = Math.Sqrt(value);
            }
            else
            {
                lower[row, column] = value / lower[column, column];
            }
        }
        return lower;
    }

    private static double[] Solve(double[,] lower, IReadOnlyList<double> right)
    {
        var size = right.Count;
        var intermediate = new double[size];
        for (var row = 0; row < size; row++)
        {
            var value = right[row];
            for (var column = 0; column < row; column++)
                value -= lower[row, column] * intermediate[column];
            intermediate[row] = value / lower[row, row];
        }

        var solution = new double[size];
        for (var row = size - 1; row >= 0; row--)
        {
            var value = intermediate[row];
            for (var column = row + 1; column < size; column++)
                value -= lower[column, row] * solution[column];
            solution[row] = value / lower[row, row];
        }
        return solution;
    }

    // If A = L L^T, then L^-T z has covariance A^-1 for z ~ N(0,I).
    private static double[] SolveUpperTranspose(double[,] lower, IReadOnlyList<double> right)
    {
        var size = right.Count;
        var solution = new double[size];
        for (var row = size - 1; row >= 0; row--)
        {
            var value = right[row];
            for (var column = row + 1; column < size; column++)
                value -= lower[column, row] * solution[column];
            solution[row] = value / lower[row, row];
        }
        return solution;
    }

    private static double Dot(IReadOnlyList<double> left, IReadOnlyList<double> right)
    {
        var sum = 0.0;
        for (var index = 0; index < left.Count; index++) sum += left[index] * right[index];
        return sum;
    }

    private static double NextStandardNormal(Random random)
    {
        var first = Math.Max(double.Epsilon, random.NextDouble());
        var second = random.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(first)) * Math.Cos(2.0 * Math.PI * second);
    }

    private static string WriteRaw(IEnumerable<ContextualRunResult> results)
    {
        var lines = new List<string>
        {
            "seed,horizon,algorithm,cumulative_reward,cumulative_pseudo_regret,optimal_action_rate,mean_decision_latency_ns,p95_decision_latency_ns"
        };
        lines.AddRange(results.Select(x => string.Join(",",
            x.Seed, x.Horizon, x.Algorithm, F(x.CumulativeReward), F(x.CumulativePseudoRegret),
            F(x.OptimalActionRate), F(x.MeanDecisionLatencyNs), F(x.P95DecisionLatencyNs))));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static string WriteSummary(IEnumerable<ContextualRunResult> results)
    {
        var lines = new List<string>
        {
            "algorithm,seeds,horizon,mean_reward,sd_reward,reward_ci95_low,reward_ci95_high,mean_regret,sd_regret,regret_ci95_low,regret_ci95_high,mean_optimal_action_rate,mean_decision_latency_ns,mean_p95_decision_latency_ns"
        };
        foreach (var group in results.GroupBy(x => x.Algorithm))
        {
            var rows = group.ToArray();
            var reward = Stats(rows.Select(x => x.CumulativeReward).ToArray());
            var regret = Stats(rows.Select(x => x.CumulativePseudoRegret).ToArray());
            lines.Add(string.Join(",", group.Key, rows.Length, rows[0].Horizon,
                F(reward.Mean), F(reward.Sd), F(reward.Mean - reward.Margin), F(reward.Mean + reward.Margin),
                F(regret.Mean), F(regret.Sd), F(regret.Mean - regret.Margin), F(regret.Mean + regret.Margin),
                F(rows.Average(x => x.OptimalActionRate)), F(rows.Average(x => x.MeanDecisionLatencyNs)),
                F(rows.Average(x => x.P95DecisionLatencyNs))));
        }
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    private static (double Mean, double Sd, double Margin) Stats(double[] values)
    {
        var mean = values.Average();
        var variance = values.Length > 1
            ? values.Sum(value => (value - mean) * (value - mean)) / (values.Length - 1)
            : 0.0;
        var sd = Math.Sqrt(variance);
        return (mean, sd, 1.96 * sd / Math.Sqrt(values.Length));
    }

    private static string F(double value) => value.ToString("G17", CultureInfo.InvariantCulture);

    private sealed record EnvironmentData(double[][] Parameters, double[][] Contexts, double[][] Rewards);

    private sealed class RunMetrics(int horizon)
    {
        public double CumulativeReward { get; set; }
        public double CumulativePseudoRegret { get; set; }
        public int OptimalSelections { get; set; }
        public double[] Latencies { get; } = new double[horizon];

        public ContextualRunResult ToResult(int seed, int runHorizon, string algorithm)
        {
            var sorted = (double[])Latencies.Clone();
            Array.Sort(sorted);
            return new ContextualRunResult(seed, runHorizon, algorithm, CumulativeReward,
                CumulativePseudoRegret, (double)OptimalSelections / runHorizon, sorted.Average(),
                sorted[Math.Clamp((int)Math.Ceiling(sorted.Length * 0.95) - 1, 0, sorted.Length - 1)]);
        }
    }

    private sealed record ContextualRunResult(int Seed, int Horizon, string Algorithm,
        double CumulativeReward, double CumulativePseudoRegret, double OptimalActionRate,
        double MeanDecisionLatencyNs, double P95DecisionLatencyNs);
}
