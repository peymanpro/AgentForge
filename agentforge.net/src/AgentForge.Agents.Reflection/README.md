# AgentForge.Agents.Reflection

An independently implemented bounded Reflection and iterative refinement agent.

## Algorithm

1. IReflectionGenerationStrategy produces an initial candidate.
2. IReflectionEvaluator assesses the candidate against application-defined acceptance criteria.
3. Rejected feedback is passed, together with the prior candidate, to the next generation attempt.
4. The agent retains the highest-scoring candidate when scores are provided.
5. If the evaluator does not provide comparable scores, the agent retains the latest evaluated candidate.
6. The run stops when a candidate is accepted, the configured no-improvement rule fires, or the attempt/time budget is exhausted.

## Design boundary

Generation and evaluation are separate injected collaborators. The agent does not assume a model can reliably judge its own output.

Scores are meaningful only within the evaluator that produced them. Scores from different evaluators are not comparable. Acceptance criteria and evaluation quality remain application responsibilities.

## Limits and outcomes

The agent enforces an attempt limit and overall timeout, propagates cancellation, and preserves the best candidate and attempt history on generation or evaluation failure.
