# Reflection / Iterative Refinement Agent — Design Note

**Status:** Proposal; implementation contract not yet approved  
**Initial target:** .NET / C#  
**Cross-language scope:** Shared observable behavior; idiomatic implementations

## 1. Purpose

The Reflection / Iterative Refinement agent improves a candidate result through a bounded feedback loop.

It is not simply a model call repeated several times, nor a prompt that asks the same model to criticize its own answer without a defined evaluation contract.

A real implementation must separate:
1. Producing an initial candidate.
2. Evaluating that candidate against explicit criteria.
3. Converting evaluation findings into actionable feedback.
4. Producing a revision.
5. Deciding whether to accept, continue, or stop.

The evaluator may be deterministic, rule-based, model-based, or supplied by the application. The agent should not require one evaluation technology.

## 2. Scope

The MVP Reflection agent should:
- Accept a task and bounded refinement options.
- Generate an initial candidate through a supplied generation strategy.
- Evaluate the candidate through an explicit evaluator contract.
- Produce structured feedback.
- Request a revision using that feedback.
- Enforce a maximum number of evaluation/refinement attempts.
- Retain the best known candidate.
- Stop when acceptance criteria are met, progress stops, a limit is reached, or execution fails.
- Propagate cancellation and emit privacy-conscious events.

The MVP does not guarantee that a model-generated evaluation is correct. Evaluation quality depends on the evaluator and criteria supplied by the application.

## 3. Conceptual lifecycle

```text
            Task + constraints + criteria
                         |
                         v
                 Generate candidate
                         |
                         v
                   Evaluate result
                         |
                +--------+---------+
                |                  |
             Accepted          Not accepted
                |                  |
                v                  v
             Complete       Build actionable feedback
                                   |
                                   v
                            Refine candidate
                                   |
                                   v
                           Check attempt budget
                                   |
                                   v
                              Evaluate again
```

The loop must have an explicit upper bound. It must not continue indefinitely.

## 4. Responsibilities

The Reflection agent coordinates the lifecycle but delegates generation, evaluation, and refinement to explicit collaborators.

### Generation strategy

Produces the first candidate from the task and application instructions.

It should be replaceable and may use the baseline chat model or another application-provided generator.

### Evaluator

Assesses the candidate against explicit criteria and returns a normalized evaluation result.

The evaluator may be:
- Deterministic rules or validators.
- Domain-specific application code.
- A model-based evaluator.
- A combination of multiple evaluation methods.

An evaluator should not silently change the task or grant new permissions.

### Refinement strategy

Produces a revised candidate using the original task, current candidate, and structured feedback.

It should not execute application tools unless those tools are explicitly included through the application's normal tool and authorization contracts.

## 5. Evaluation contract

The evaluator should return structured information rather than unrestricted critique text alone.

A conceptual evaluation result may include:

- Acceptance decision.
- Optional score or dimension-specific measurements.
- A list of identified issues.
- Actionable feedback for refinement.
- Evaluation metadata safe for diagnostics.
- The criteria or evaluation version used.

A score should be optional. Not every domain has a meaningful numeric measure, and a model-generated score must not be presented as an objective probability or guarantee.

The evaluation contract must distinguish:
- **Accepted:** the candidate satisfies the configured acceptance condition.
- **Needs revision:** the candidate has actionable issues and another attempt may help.
- **Inconclusive:** the evaluator cannot reliably assess the candidate.
- **Evaluation failed:** the evaluator itself failed or returned an invalid result.

The agent must define how inconclusive and failed evaluations affect the final result. They must not be silently treated as acceptance.

## 6. Acceptance criteria

Acceptance is determined by an explicit policy or evaluator outcome.

The application should be able to define criteria such as:
- Required fields or schema validity.
- Presence of required evidence or sources.
- Domain-specific correctness checks.
- Minimum rule-based quality conditions.
- A threshold from a documented evaluator.

The agent should avoid a hidden default such as "accept when the model says it is good enough."

When model-based evaluation is used, the result is an assessment from that evaluator—not proof of correctness. Applications should use deterministic validation for hard requirements where possible.

## 7. Refinement semantics

A refinement attempt should receive:
- The original task and constraints.
- The current candidate.
- Structured evaluation feedback.
- Relevant context explicitly allowed by the application.

The revised candidate should be evaluated again. It should not automatically replace the best known candidate merely because it is newer.

The agent should track:
- Attempt number.
- Candidate identifier or version.
- Evaluation result.
- Whether the candidate was accepted.
- Whether it improved according to a configured comparison policy.
- Whether it became the best known candidate.

The feedback should be actionable and bounded. The agent should not need to expose private model reasoning to explain why a revision is requested.

## 8. Progress and stopping policy

The MVP should support at least:

- **Accepted:** return the accepted candidate.
- **Maximum attempts reached:** return the best candidate with a non-accepted or partial status.
- **No progress:** stop when a configured progress policy determines further refinement is not useful.
- **Cancelled or timed out:** stop and preserve the best available candidate.
- **Generation failure:** return a failure, preserving any earlier candidate if one exists.
- **Evaluation failure:** return an explicit evaluation failure or follow a configured bounded fallback policy.

A no-progress rule should be explicit. It may compare normalized scores when the evaluator provides comparable scores, or compare stable candidate hashes when exact repetition is meaningful. It must not assume that all quality improvements are numeric.

A new candidate must not be treated as better merely because it differs from the previous candidate.

## 9. Budgets and cancellation

The agent should enforce:
- Maximum refinement attempts.
- Overall elapsed time.
- Optional generation and evaluation call limits.
- Optional usage/cost budgets when reliable data is available.

Cancellation must propagate to generation and evaluation. A run-level timeout should cover the complete loop, not reset to a fresh unlimited timeout on every attempt.

The default maximum number of refinements must be finite and documented.

## 10. Result semantics

The final result should preserve:
- Best candidate.
- Latest candidate, if different.
- Evaluation history or a bounded summary of it.
- Acceptance state.
- Number of attempts.
- Terminal reason.
- Safe timing and usage metadata where available.

Possible terminal statuses include:
- Accepted.
- Not accepted within the attempt budget.
- No progress.
- Evaluation inconclusive.
- Generation or evaluation failed.
- Cancelled.
- Timed out.
- Resource budget exhausted.

The result should not label an unaccepted candidate as successful. Applications may choose how to display a partial candidate to their users.

## 11. State ownership and privacy

The agent owns state required for one refinement run:
- Original task and constraints.
- Current and best candidates.
- Evaluation results and feedback.
- Attempt count and budgets.
- Terminal status.

Long-term memory and cross-run learning are separate capabilities, not implicit responsibilities of Reflection.

Candidates and evaluation feedback may contain sensitive user or business information. They must not be recorded in logs by default. Structured events should capture status and timing without exposing content.

## 12. Testing strategy

Use deterministic fake generation strategies, evaluators, and refinement strategies.

Minimum tests:

1. Initial candidate is accepted and no refinement is requested.
2. A candidate is rejected, revised, and accepted on the next evaluation.
3. The maximum attempt count is enforced.
4. The best candidate is retained when a later revision is worse.
5. Evaluator returns inconclusive.
6. Evaluator fails or returns malformed output.
7. Generation fails after an initial candidate exists.
8. No-progress policy terminates a repeated or non-improving cycle.
9. Cancellation during generation is propagated.
10. Cancellation during evaluation is propagated.
11. Run timeout covers the entire loop.
12. A score is not required for a valid evaluator.
13. An evaluator cannot mark an invalid mandatory schema as accepted when deterministic validation is configured as a hard requirement.
14. The agent does not invoke tools unless explicitly supplied through a permitted strategy.
15. Events report the exact candidate/evaluation/refinement transitions.
16. Default logs do not expose candidate content or evaluation feedback.

## 13. API sketch

A possible use shape is:

```csharp
var agent = ReflectionAgent.Create()
    .WithGenerator(generator)
    .WithEvaluator(evaluator)
    .WithRefinementStrategy(refinementStrategy)
    .WithMaxAttempts(3)
    .WithAcceptancePolicy(acceptancePolicy)
    .Build();

var result = await agent.RunAsync(
    "Prepare a concise technical proposal.",
    cancellationToken);
```

This is illustrative only. Names, builder methods, result types, defaults, and package boundaries are not approved.

## 14. Alternatives considered

### Repeat the same prompt several times

**Rejected.** Repetition alone does not define an evaluation contract, feedback, acceptance condition, or improvement policy.

### Ask the model to critique itself and accept its own verdict

**Not selected as the default.** This can be one evaluator strategy, but it must return a normalized assessment and must not be treated as independent proof of correctness.

### Separate generation, evaluation, and refinement

**Recommended for prototyping.** This makes each responsibility replaceable and allows deterministic tests of the control loop.

## 15. Open design questions

1. Should evaluation feedback use a common issue model, free-form text, or both?
2. Should the evaluator own acceptance, or should a separate acceptance policy combine evaluator output with hard validation rules?
3. What is the default no-progress policy when scores are unavailable?
4. Should the best candidate always be returned when the run is cancelled or times out?
5. How should multiple evaluators be combined when their assessments conflict?
6. Should a refinement strategy be allowed to request tools, or should tool access remain an explicit external composition?
7. How should evaluation history be bounded to avoid unbounded context growth?

## 16. Decision status

This note recommends a **bounded generate–evaluate–refine loop with a replaceable evaluator, explicit acceptance criteria, and best-candidate preservation**.

It is a proposal, not an approved API or implementation claim. Validate it with deterministic fakes before promoting it into the shared specification.
