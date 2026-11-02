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

## Contextual bandits

The runner also compares the production disjoint LinUCB selector with a benchmark-only Bayesian Linear Thompson Sampling implementation on a stationary disjoint linear reward model.

The environment has 5 actions and 5 features. Each action has an independently generated unknown parameter vector. Contexts are seeded Gaussian feature vectors with an intercept; rewards are linear expected values plus Gaussian noise (standard deviation 0.2). Each policy receives the same contexts and action-specific reward streams for a given seed. The LinUCB exploration coefficient is fixed at 0.4; Thompson Sampling uses a Gaussian posterior with ridge regularization 1.0 and the known observation-noise standard deviation. These are fixed benchmark settings, not tuned optima.

For each action, the posterior precision is A = λI + Σxxᵀ and the reward vector is b = Σrx. The posterior mean is A⁻¹b and covariance is σ²A⁻¹. Samples use a Cholesky factorization; no external numerical package is required.

The runner writes contextual-bandit-runs.csv and contextual-bandit-summary.csv. The full run uses 30 seeds and 10,000 rounds; CI uses 2 seeds and 300 rounds for this contextual family. The context-free and contextual results are intentionally reported separately and must not be combined into one ranking.
