# Contextual Bandit Design

## Goal

Extend the Learning Agent family with a contextual bandit that chooses among the currently available actions using a feature vector describing the current decision context. Unlike the existing stationary, context-free bandit, the same action may receive different expected rewards in different contexts.

## Algorithm: LinUCB (disjoint action models)

The first contextual implementation uses disjoint LinUCB. Each action maintains its own linear reward model:

- A_a = lambda I + sum_t x_t x_t^T
- b_a = sum_t r_t x_t
- theta_a = A_a^-1 b_a
- score(a|x) = x^T theta_a + alpha * sqrt(x^T A_a^-1 x)

The selected action maximizes the upper-confidence score. The host supplies the feature vector and finite reward; the library does not perform feature extraction or execute the action.

## State model

- Context is a finite, non-empty vector of finite doubles with a fixed dimension per learner.
- Each action has an immutable sufficient-statistics snapshot: symmetric positive-definite matrix A and vector b.
- The state is immutable and serializable by the host through public read-only data; no persistence dependency is introduced.
- Observe(action, context, reward) returns a new state and applies the rank-one update to A and b.
- The regularization parameter lambda must be finite and strictly positive; alpha must be finite and non-negative.
- Candidate actions are supplied per decision. Unseen actions are naturally explored by their high uncertainty; no special forced-first rule is required.

## Numerical implementation

Do not explicitly compute a matrix inverse. Solve the symmetric positive-definite linear systems using a Cholesky factorization. Validate dimensions, finite inputs, symmetry, and positive definiteness at construction/update boundaries. Reject malformed or numerically invalid state rather than silently producing NaN or infinity.

## API boundary

Implement this as a separate ContextualLearningAgent<TAction> and contextual state/input/output types in the existing Learning module. Keep LearningAgent<TAction>, LearningState<TAction>, and ILearningPolicy<TAction> source-compatible. The host remains responsible for authorization, execution, reward evaluation, and persistence.

## Explicit limitations

- Disjoint linear reward models; no shared/generalized parameter model across actions.
- Context features and their scaling are application-owned.
- No delayed-feedback correlation, non-stationary forgetting, batch updates, or general MDP/RL support.
- Linear expected reward assumption; quality depends on meaningful features and appropriate regularization.
