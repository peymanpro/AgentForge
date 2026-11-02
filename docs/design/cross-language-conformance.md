# Cross-Language Behavioral Conformance — Design Note

**Status:** Proposal; test format and conformance levels are not approved  
**Scope:** .NET, TypeScript/NestJS, Python, and future language implementations

## 1. Problem

AgentForge intends to provide independent, idiomatic implementations in multiple programming languages.

Independence is important: a Python implementation should not be forced to use .NET architecture, and TypeScript should not be a mechanical translation of C#. But independent implementations can drift in observable behavior. A ReAct run might stop differently, a tool denial might be reported as success, or a plan validator might accept a malformed plan in one language and reject it in another.

The project needs a way to verify shared semantics without requiring identical APIs, internal data structures, or runtime implementations.

## 2. Design goals

The conformance approach should:

1. Test behavior defined by the shared specification.
2. Allow each language to use idiomatic APIs and internal architectures.
3. Be deterministic and independent of live model-provider behavior.
4. Make compatibility claims specific to a feature and specification version.
5. Distinguish required behavior from optional capabilities.
6. Document known differences and unsupported behavior.
7. Avoid turning test fixtures into an unnecessary runtime network protocol.
8. Be practical for community maintainers to run locally and in CI.

## 3. Core principle: compare semantics, not implementation details

Conformance should not require:
- Identical class or method names.
- Identical exception types.
- Identical internal state representations.
- Identical package boundaries.
- Byte-for-byte identical output when providers or serialization libraries differ.
- The same model provider, tokenizer, or model-generated prose.

It should verify:
- The same defined input conditions.
- The same documented state transitions.
- The same termination and failure semantics.
- The same authorization and validation boundaries.
- Equivalent normalized outcomes for the behaviors the specification marks as shared.

A language implementation may document an intentional difference where the specification permits one. It may not silently change a behavior that the specification requires.

## 4. Test layers

Conformance should be organized into separate layers.

### Layer A — Shared semantic scenarios

These test the behavior of an AgentForge component using deterministic fake collaborators.

Examples:
- ReAct stops after a final response.
- ReAct does not execute a denied tool.
- A tool validation error occurs before the tool implementation is invoked.
- Planning validates the entire plan before executing the first step.
- Planning reports partial completion when a step fails under stop-on-failure.
- Reflection preserves the best candidate when a later revision is worse.
- Every bounded execution path honors cancellation and configured limits.

This is the main cross-language conformance layer.

### Layer B — Model adapter contract tests

These test an individual provider adapter's normalization behavior:
- Request message roles and content are mapped correctly.
- Normalized responses preserve supported content and finish reasons.
- Tool-call requests are mapped without executing tools.
- Optional capabilities are advertised only when implemented.
- Unsupported features fail explicitly.
- Errors and cancellation are normalized consistently.

These tests are adapter-specific and may require provider mocks or controlled provider test environments.

### Layer C — Tool adapter contract tests

These test the boundary between declared tools and executable application operations:
- Input validation occurs before execution.
- Authorization is applied to each invocation.
- Cancellation and timeouts are propagated.
- Side-effect and retry policies are honored.
- Results and failures are normalized.
- Sensitive values are not emitted in default diagnostic events.

### Layer D — Language-native quality tests

Each implementation keeps its normal language-specific test suite:
- Unit tests.
- Integration tests.
- Static analysis and linting.
- Formatting and type checks.
- Package build and installation tests.
- Documentation and example tests.

Passing shared semantic scenarios does not replace these tests.

## 5. Deterministic scenario fixtures

A shared scenario fixture is a test description, not a production API or transport protocol.

A candidate structure may include:

```json
{
  "scenario_id": "react.denied-tool-is-not-executed",
  "spec_version": "0.1-draft",
  "component": "react",
  "description": "A tool request denied by policy must not invoke the tool.",
  "given": {
    "model_decisions": [
      {
        "type": "tool_call",
        "tool_name": "delete_record",
        "arguments": {
          "record_id": "example-1"
        }
      }
    ],
    "tool_policy": {
      "delete_record": "deny"
    }
  },
  "when": {
    "task": "Delete the requested record."
  },
  "then": {
    "terminal_status": "failed_or_blocked",
    "tool_invocations": 0,
    "required_events": [
      "tool.requested",
      "tool.authorization_denied"
    ]
  }
}
```

This is illustrative. The final fixture schema must define exact status names, required fields, event semantics, and versioning before it is used as an authoritative conformance suite.

Fixtures should contain deterministic inputs and expected observable behavior. They should avoid real credentials, personal data, live provider responses, or unstable model-generated prose.

## 6. Test harness responsibilities

Each language implementation should provide a native test harness that can:
- Load shared scenario fixtures.
- Configure a deterministic fake model or decision strategy.
- Register deterministic fake tools and policies.
- Execute the implementation under test.
- Normalize observable outcomes to the fixture semantics.
- Report pass, fail, skip, or unsupported with reasons.

The harness should not require a shared runtime library across languages. It is acceptable for .NET, TypeScript, and Python to implement separate harnesses that interpret the same fixture format.

## 7. ReAct conformance scenarios

Candidate shared scenarios include:

| Scenario | Required observable behavior |
|---|---|
| Immediate final response | Run completes without tool invocation |
| Valid tool round trip | Tool executes once and its observation is available to the next model decision |
| Unknown tool | Unknown tool is rejected; no unrelated tool executes |
| Denied tool | Authorization denial prevents invocation |
| Invalid arguments | Validation occurs before the tool implementation runs |
| Tool failure | Failure is represented distinctly from success |
| Model turn limit | Run stops with a limit-exhausted status |
| Tool-call limit | No additional tool is executed after the configured limit |
| Cancellation | Cancellation reaches the active model or tool operation |
| Unsupported decision capability | Run reports the unsupported capability explicitly |

Provider-specific request shapes are outside this shared agent-behavior layer.

## 8. Planning conformance scenarios

Candidate shared scenarios include:

| Scenario | Required observable behavior |
|---|---|
| Valid sequential plan | Steps execute in declared order |
| Invalid plan | No step executes before full-plan validation succeeds |
| Duplicate step ID | Plan is rejected |
| Invalid output reference | Plan is rejected before execution |
| Stop on failure | Later steps do not execute after a failed step |
| Partial result | Completed step outputs remain identifiable after a later failure |
| Step limit | Plan exceeding the configured maximum is rejected |
| Cancellation | Cancellation propagates to the active step |
| ReAct step composition | Only the step's explicitly assigned tools are available |

## 9. Reflection conformance scenarios

Candidate shared scenarios include:

| Scenario | Required observable behavior |
|---|---|
| Initial candidate accepted | No refinement is requested |
| Rejected then accepted | A revision is generated and evaluated |
| Max attempts | Run stops at the configured attempt limit |
| Best candidate | A worse later revision does not replace the best candidate |
| Inconclusive evaluation | Run does not silently mark the result accepted |
| Evaluator failure | Failure is distinguishable from rejection |
| No progress | Configured no-progress policy terminates the loop |
| Cancellation | Cancellation reaches generation or evaluation |
| No score | A valid evaluator can operate without a numeric score |

## 10. Fixture versioning

Every authoritative fixture should identify:
- Scenario identifier.
- Specification version.
- Component and behavior area.
- Required or optional status.
- Expected observable semantics.
- Any relevant capability prerequisites.

Fixtures should evolve with the specification. Breaking changes to a required behavior should update the specification version and include migration notes where needed.

An implementation should report which specification and fixture versions it has tested against.

## 11. Conformance status

Do not use a single ambiguous label such as "fully compatible" for an entire language implementation.

A conformance report should be feature-specific. Candidate statuses:

- **Not implemented:** feature is not available.
- **In progress:** implementation exists but is not yet validated.
- **Conformant:** required scenarios for the named specification version pass.
- **Partially conformant:** some required scenarios pass; gaps are listed.
- **Not applicable:** feature or capability is intentionally outside the implementation's scope.
- **Unsupported:** implementation explicitly does not support the required capability.

Optional capabilities must be reported separately from required base behavior.

A conformance claim should link to the tested commit, specification version, scenario suite version, and CI result where practical.

## 12. What must not be compared

The shared suite should not attempt to certify:
- Model intelligence or response quality across providers.
- Tokenization equivalence.
- Pricing or latency equivalence.
- The exact wording of natural-language answers.
- Hidden chain-of-thought or private reasoning traces.
- Database or vector-store retrieval quality using uncontrolled data.
- Security properties that require a separate threat model and security assessment.

Those topics need their own reproducible evaluation methods. They must not be inferred from behavioral conformance tests.

## 13. CI strategy

Each language implementation should run:
1. Its native unit and integration tests.
2. The relevant shared conformance scenarios.
3. Package build and installation validation.
4. Lint, formatting, static analysis, and documentation checks.

The root repository may validate fixture structure and specification metadata. It should not require every language implementation to run on every unrelated source change.

A change to a shared required behavior should trigger or require conformance checks for affected implementations. A change isolated to one language should not release another language's packages.

## 14. Governance

Changes to required conformance scenarios should be reviewed alongside the relevant specification change.

A new scenario should include:
- The behavior it protects.
- The specification clause it maps to.
- Why the behavior is observable and language-neutral.
- Whether it is required or optional.
- Deterministic setup and expected outcome.

Maintainers should be able to challenge a fixture that accidentally dictates internal design rather than shared semantics.

## 15. Open design questions

1. Should fixtures use JSON, YAML, or a small language-neutral schema with JSON serialization?
2. Should the first conformance suite run from repository root or be vendored/copied into each implementation?
3. How should optional capabilities be represented in fixtures?
4. Should expected outcomes be compared as a minimal semantic projection rather than exact event sequences?
5. How should feature-specific conformance reports be published and versioned?
6. What policy determines whether a scenario is required or advisory?
7. How should a specification change be coordinated across maintainers when implementations lag?

## 16. Decision status

This note recommends **deterministic, versioned, behavior-level conformance scenarios interpreted by independent language-native test harnesses**.

It is a proposal. Before becoming mandatory, the team should prototype one ReAct scenario in .NET and a second language to confirm that the fixture format tests semantics without forcing identical internal structures or public APIs.
