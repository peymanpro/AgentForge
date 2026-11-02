# Learning Bandit Benchmark Protocol

## Purpose

This document defines a reproducible evaluation protocol for the learning algorithms in AgentForge. The benchmark is an engineering experiment, not a claim that one policy is universally best.

The benchmark keeps two problem families separate. Epsilon-Greedy and classical Thompson Sampling are evaluated on stationary, context-free multi-armed bandits. Disjoint LinUCB and (in a later implementation phase) Bayesian Linear Thompson Sampling are evaluated on contextual bandits. Results from these families must not be pooled into one leaderboard.

## Questions

1. How quickly does each policy identify high-reward actions?
2. How much cumulative reward and regret does it accumulate over a fixed horizon?
3. How sensitive are results to exploration parameters, reward noise, action count, and context dimension?
4. What is the decision-time and allocation overhead of the implementation?

## Benchmark families

### A. Stationary context-free bandits

Compare:
- AgentForge Epsilon-Greedy, including its initial preference for unobserved actions.
- Beta-Bernoulli Thompson Sampling, implemented in the benchmark harness initially; it is not yet an AgentForge production policy.

Environment:
- A fixed set of Bernoulli arms with known success probabilities.
- Include easy, moderate, and difficult gaps between the best and second-best arm.
- Vary the number of arms (for example, 5, 10, and 20).
- Use independent seeded environments and runs.

The expected reward of an arm is its Bernoulli probability. The per-round oracle is the arm with the highest probability.

### B. Contextual bandits

Compare:
- AgentForge disjoint LinUCB.
- Bayesian Linear Thompson Sampling, only after its posterior and sampling implementation is independently tested.
- An epsilon-greedy context-blind baseline may be included as a deliberately limited reference, but it must not be presented as a contextual algorithm.

Environment:
- Each action has a fixed unknown reward parameter vector.
- Context vectors are generated from documented seeded distributions and normalized consistently.
- Expected reward is the dot product of the action parameter vector and the current context; configurable observation noise is added.
- The oracle selects the action with the highest true expected reward for that context.

The first contextual benchmark uses a stationary linear reward model. Nonlinear and drifting environments are separate future experiments.

## Metrics

Report per run and aggregate across runs:
- **Cumulative reward:** sum of observed rewards.
- **Cumulative pseudo-regret:** sum of oracle expected reward minus selected-action expected reward. Use expected rewards, not noisy observations, in this calculation.
- **Average reward:** cumulative observed reward divided by horizon.
- **Regret per round:** cumulative pseudo-regret divided by horizon.
- **Best-action selection rate:** fraction of rounds selecting an oracle-optimal action. For ties, any optimal action counts.
- **Time to threshold:** first round at which a predeclared rolling-window regret or reward threshold is met; report as not reached when applicable.
- **Decision latency:** median and p95 policy-selection time, measured separately from environment generation and reward sampling.
- **Allocation / memory observations:** optional diagnostic measurements, never mixed into reward-quality metrics.

Report mean, standard deviation, and 95% confidence intervals across independent seeds. Include raw per-seed CSV/JSON output so aggregate charts can be reproduced.

## Experimental controls

- Use a fixed, published seed list and the same environment instances for policies within each family.
- Use separate seed sets for parameter tuning and final evaluation.
- Tune epsilon, LinUCB alpha, and Thompson prior settings on tuning seeds only. Publish the search grid and selected values.
- Keep horizon, action count, reward distribution, context distribution, and noise level identical within each comparison.
- Do not tune a policy on the final evaluation seeds.
- Warm up .NET code before timing. Benchmark selection latency without including random environment construction, reward generation, or console output.
- Run enough independent seeds to estimate uncertainty; the initial target is 30 seeds per scenario. Increase this if confidence intervals remain wide.
- Record .NET SDK/runtime version, OS, CPU, commit SHA, scenario configuration, and timestamp with every result.
- Never infer statistical significance from overlapping or non-overlapping error bars alone; include a suitable paired analysis over shared seeds when making inferential claims.

## Interpretation rules

- A policy can win on cumulative reward while losing on latency; report both.
- Context-blind Epsilon-Greedy is a baseline in contextual scenarios, not an algorithmically equivalent competitor to LinUCB.
- Results from stationary environments do not establish performance under non-stationarity, delayed feedback, adversarial rewards, or general reinforcement learning.
- Benchmark results do not automatically justify changing AgentForge defaults. Any default change requires repeatable evidence and a separate design decision.

## Implementation plan

1. Add a standalone benchmark runner that consumes deterministic scenario definitions and writes machine-readable per-run results.
2. Implement and test the stationary Bernoulli environment and Beta-Bernoulli Thompson Sampling baseline.
3. Adapt the existing Epsilon-Greedy policy to the same benchmark decision loop without changing production behavior.
4. Add the contextual linear environment and evaluate the existing LinUCB implementation.
5. Add Bayesian Linear Thompson Sampling only after its posterior update and sampling math have dedicated deterministic tests.
6. Publish scenario configurations, raw results, aggregate summaries, and reproducible commands.

The benchmark harness must remain outside production agent packages. Benchmark-only algorithms must be clearly labelled and must not silently become supported AgentForge APIs.
