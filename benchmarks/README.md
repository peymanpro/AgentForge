# AgentForge Benchmarks

This directory contains reproducible experiments for AgentForge algorithms. Benchmark-only implementations are intentionally kept outside production agent packages.

## Stationary context-free bandits

The first runner compares the production Epsilon-Greedy policy with a benchmark-only Beta-Bernoulli Thompson Sampling implementation.

Scenarios cover 5, 10, and 20 Bernoulli arms with easy, moderate, and difficult best-vs-runner-up reward gaps. Each policy receives the same seeded reward stream for a given scenario and seed. The full run uses 30 independent seeds and 10,000 decisions per run.

### Run

~~~bash
dotnet run --project benchmarks/AgentForge.Benchmarks -- --full
~~~

A short smoke run (2 seeds, 500 decisions) is used in CI:

~~~bash
dotnet run --project benchmarks/AgentForge.Benchmarks
~~~

Use --output <directory> to select the output directory. The runner writes:

- stationary-bandit-runs.csv: raw per-seed results.
- stationary-bandit-summary.csv: means, sample standard deviations, and normal-approximation 95% confidence intervals.

Metrics include cumulative observed reward, cumulative pseudo-regret (computed from expected arm rewards), optimal-action selection rate, and policy decision latency. Timing excludes reward sampling and state updates.

The current runner evaluates fixed epsilon=0.1 and Beta(1,1) priors; it does not perform hyperparameter tuning. Confidence intervals are descriptive normal approximations, not a substitute for paired statistical inference. Results from this stationary Bernoulli setup do not establish performance on contextual, non-stationary, adversarial, or general reinforcement-learning tasks.

The contextual LinUCB vs Linear Thompson Sampling experiment is a separate planned benchmark family and is not included in this runner yet.
