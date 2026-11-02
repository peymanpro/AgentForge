# Learning Agent Design

## Scope

The module implements a small, testable stationary multi-armed bandit learner. It is a concrete first learning architecture, not general reinforcement learning.

## Responsibilities

1. The host supplies candidate actions and the current immutable learning-state snapshot.
2. The performance element selects an action through ILearningPolicy.
3. The host authorizes and executes the selected action.
4. The host-side critic evaluates the outcome and supplies a finite scalar reward.
5. The host calls LearningState.Observe and persists the returned snapshot when required.

The module never executes external actions and never assumes that a model-generated action is authorized.

## Initial algorithm

EpsilonGreedyLearningPolicy first tries candidate actions without observations. Once all candidates have observations, it explores with probability epsilon; otherwise it selects the candidate with the highest sample mean reward.

The sample mean is updated incrementally, keeping state compact: one observation count and one average reward per action.

## Boundaries

- The epsilon-greedy learner is stationary and context-free.
- The contextual learner is a separate disjoint LinUCB implementation with one linear reward model per action.
- Reward semantics, feature engineering/scaling, authorization, action execution, and persistence belong to the host.
- Both learning-state types are immutable, making state transitions explicit and testable.
- LinUCB uses Cholesky factorization and linear-system solves rather than explicitly computing matrix inverses.
- The contextual implementation assumes a linear expected reward model and fixed-dimensional finite feature vectors.
- Shared-parameter contextual models, non-stationary forgetting, delayed feedback, and general MDP/RL algorithms are outside the current scope.
