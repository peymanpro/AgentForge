using AgentForge.Agents.Learning.Domain.Models;
using AgentForge.Agents.Learning.Policies;
using System.Diagnostics;
using System.Globalization;

var full = args.Contains("--full", StringComparer.OrdinalIgnoreCase);
var outputDirectory = GetOption(args, "--output") ?? Path.Combine(Environment.CurrentDirectory, "artifacts", "bandit-benchmarks");
var seeds = full ? Enumerable.Range(1, 30).ToArray() : [1, 7];
var horizon = full ? 10_000 : 500;
Directory.CreateDirectory(outputDirectory);

var scenarios = from arms in new[] { 5, 10, 20 }
                from difficulty in new[] { "easy", "moderate", "difficult" }
                select new Scenario(arms, difficulty, CreateProbabilities(arms, difficulty));

var results = new List<RunResult>();
foreach (var scenario in scenarios)
foreach (var seed in seeds)
{
    results.Add(RunEpsilonGreedy(scenario, seed, horizon));
    results.Add(RunThompsonSampling(scenario, seed, horizon));
}

var rawPath = Path.Combine(outputDirectory, "stationary-bandit-runs.csv");
var summaryPath = Path.Combine(outputDirectory, "stationary-bandit-summary.csv");
await File.WriteAllTextAsync(rawPath, Csv.WriteRaw(results), new System.Text.UTF8Encoding(false));
await File.WriteAllTextAsync(summaryPath, Csv.WriteSummary(results), new System.Text.UTF8Encoding(false));
Console.WriteLine($"Completed {results.Count} runs; horizon={horizon}; seeds={seeds.Length}.");
Console.WriteLine($"Raw results: {rawPath}");
Console.WriteLine($"Summary: {summaryPath}");

static string? GetOption(string[] arguments, string name)
{
    var index = Array.FindIndex(arguments, item => string.Equals(item, name, StringComparison.OrdinalIgnoreCase));
    return index >= 0 && index + 1 < arguments.Length ? arguments[index + 1] : null;
}

static double[] CreateProbabilities(int armCount, string difficulty)
{
    var (best, second) = difficulty switch
    {
        "easy" => (0.80, 0.60),
        "moderate" => (0.65, 0.60),
        _ => (0.55, 0.53)
    };
    var probabilities = Enumerable.Repeat(0.15, armCount).ToArray();
    probabilities[0] = best;
    probabilities[1] = second;
    return probabilities;
}

static RunResult RunEpsilonGreedy(Scenario scenario, int seed, int horizon)
{
    var policyRandom = new Random(unchecked(seed * 7919 + scenario.ArmCount * 31));
    var rewardRandom = new Random(unchecked(seed * 104729 + scenario.ArmCount * 17));
    var policy = new EpsilonGreedyLearningPolicy<int>(0.1, policyRandom.NextDouble, policyRandom.Next);
    var state = new LearningState<int>();
    var regret = 0.0;
    var rewardTotal = 0.0;
    var optimalSelections = 0;
    var latency = new double[horizon];
    var bestExpected = scenario.Probabilities.Max();

    for (var round = 0; round < horizon; round++)
    {
        var estimates = Enumerable.Range(0, scenario.ArmCount).Select(action =>
        {
            var statistics = state.GetStatistics(action);
            return new ActionEstimate<int>(action, statistics.ObservationCount, statistics.AverageReward);
        }).ToArray();
        var start = Stopwatch.GetTimestamp();
        var selection = policy.Select(estimates, state.TotalObservations);
        latency[round] = Stopwatch.GetElapsedTime(start).TotalNanoseconds;

        var action = selection.Action;
        var expected = scenario.Probabilities[action];
        var reward = rewardRandom.NextDouble() < expected ? 1.0 : 0.0;
        rewardTotal += reward;
        regret += bestExpected - expected;
        if (Math.Abs(expected - bestExpected) < 1e-12) optimalSelections++;
        state = state.Observe(action, reward);
    }

    return RunResult.Create(scenario, seed, horizon, "EpsilonGreedy", rewardTotal, regret, optimalSelections, latency);
}

static RunResult RunThompsonSampling(Scenario scenario, int seed, int horizon)
{
    var policyRandom = new Random(unchecked(seed * 15485863 + scenario.ArmCount * 43));
    var rewardRandom = new Random(unchecked(seed * 104729 + scenario.ArmCount * 17));
    var alpha = Enumerable.Repeat(1.0, scenario.ArmCount).ToArray();
    var beta = Enumerable.Repeat(1.0, scenario.ArmCount).ToArray();
    var regret = 0.0;
    var rewardTotal = 0.0;
    var optimalSelections = 0;
    var latency = new double[horizon];
    var bestExpected = scenario.Probabilities.Max();

    for (var round = 0; round < horizon; round++)
    {
        var start = Stopwatch.GetTimestamp();
        var action = 0;
        var bestSample = double.NegativeInfinity;
        for (var arm = 0; arm < scenario.ArmCount; arm++)
        {
            var sample = RandomDistributions.SampleBeta(alpha[arm], beta[arm], policyRandom);
            if (sample > bestSample)
            {
                bestSample = sample;
                action = arm;
            }
        }
        latency[round] = Stopwatch.GetElapsedTime(start).TotalNanoseconds;

        var expected = scenario.Probabilities[action];
        var reward = rewardRandom.NextDouble() < expected ? 1.0 : 0.0;
        rewardTotal += reward;
        regret += bestExpected - expected;
        if (Math.Abs(expected - bestExpected) < 1e-12) optimalSelections++;
        if (reward > 0) alpha[action]++; else beta[action]++;
    }

    return RunResult.Create(scenario, seed, horizon, "ThompsonSampling", rewardTotal, regret, optimalSelections, latency);
}

internal sealed record Scenario(int ArmCount, string Difficulty, double[] Probabilities);

internal sealed record RunResult(
    int ArmCount, string Difficulty, int Seed, int Horizon, string Algorithm,
    double CumulativeReward, double CumulativePseudoRegret, double OptimalActionRate,
    double MeanDecisionLatencyNs, double P95DecisionLatencyNs)
{
    public static RunResult Create(Scenario scenario, int seed, int horizon, string algorithm,
        double reward, double regret, int optimalSelections, double[] latency)
    {
        Array.Sort(latency);
        return new RunResult(scenario.ArmCount, scenario.Difficulty, seed, horizon, algorithm,
            reward, regret, (double)optimalSelections / horizon, latency.Average(),
            latency[Math.Clamp((int)Math.Ceiling(latency.Length * 0.95) - 1, 0, latency.Length - 1)]);
    }
}

internal static class RandomDistributions
{
    // Marsaglia-Tsang gamma sampler; Beta(a,b) = Gamma(a) / (Gamma(a) + Gamma(b)).
    public static double SampleBeta(double a, double b, Random random)
    {
        var x = SampleGamma(a, random);
        var y = SampleGamma(b, random);
        return x / (x + y);
    }

    private static double SampleGamma(double shape, Random random)
    {
        if (shape < 1.0)
        {
            var u = Math.Max(double.Epsilon, random.NextDouble());
            return SampleGamma(shape + 1.0, random) * Math.Pow(u, 1.0 / shape);
        }
        var d = shape - 1.0 / 3.0;
        var c = 1.0 / Math.Sqrt(9.0 * d);
        while (true)
        {
            var x = SampleStandardNormal(random);
            var v = 1.0 + c * x;
            if (v <= 0) continue;
            v = v * v * v;
            var u = random.NextDouble();
            if (u < 1.0 - 0.0331 * x * x * x * x ||
                Math.Log(Math.Max(double.Epsilon, u)) < 0.5 * x * x + d * (1.0 - v + Math.Log(v)))
                return d * v;
        }
    }

    private static double SampleStandardNormal(Random random)
    {
        var u1 = Math.Max(double.Epsilon, random.NextDouble());
        var u2 = random.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }
}

internal static class Csv
{
    public static string WriteRaw(IEnumerable<RunResult> results)
    {
        var lines = new List<string>
        {
            "arm_count,difficulty,seed,horizon,algorithm,cumulative_reward,cumulative_pseudo_regret,optimal_action_rate,mean_decision_latency_ns,p95_decision_latency_ns"
        };
        lines.AddRange(results.Select(x => string.Join(",",
            x.ArmCount, x.Difficulty, x.Seed, x.Horizon, x.Algorithm,
            F(x.CumulativeReward), F(x.CumulativePseudoRegret), F(x.OptimalActionRate),
            F(x.MeanDecisionLatencyNs), F(x.P95DecisionLatencyNs))));
        return string.Join(Environment.NewLine, lines) + Environment.NewLine;
    }

    public static string WriteSummary(IEnumerable<RunResult> results)
    {
        var groups = results.GroupBy(x => new { x.ArmCount, x.Difficulty, x.Horizon, x.Algorithm });
        var lines = new List<string>
        {
            "arm_count,difficulty,horizon,algorithm,seeds,mean_reward,sd_reward,reward_ci95_low,reward_ci95_high,mean_regret,sd_regret,regret_ci95_low,regret_ci95_high,mean_optimal_action_rate,mean_decision_latency_ns,mean_p95_decision_latency_ns"
        };
        foreach (var group in groups)
        {
            var rows = group.ToArray();
            var reward = Stats(rows.Select(x => x.CumulativeReward).ToArray());
            var regret = Stats(rows.Select(x => x.CumulativePseudoRegret).ToArray());
            lines.Add(string.Join(",",
                group.Key.ArmCount, group.Key.Difficulty, group.Key.Horizon, group.Key.Algorithm, rows.Length,
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
        var variance = values.Length > 1 ? values.Sum(x => (x - mean) * (x - mean)) / (values.Length - 1) : 0.0;
        var sd = Math.Sqrt(variance);
        return (mean, sd, 1.96 * sd / Math.Sqrt(values.Length));
    }

    private static string F(double value) => value.ToString("G17", CultureInfo.InvariantCulture);
}
