# AgentForge Shared Specification

**Status:** Draft — not yet stable  
**Specification version:** 0.1-draft  
**Scope:** Shared concepts and behavioral expectations across language implementations

This document establishes the initial shared vocabulary and engineering expectations for AgentForge. It is intentionally a draft. It must be reviewed and validated before implementations claim conformance.

## 1. Purpose

AgentForge is a multi-language ecosystem for engineering AI agents and related systems. This specification defines shared semantics and observable behavior while allowing each language implementation to use idiomatic APIs, internal architectures, and packaging conventions.

The specification is not a requirement to make all implementations structurally identical.

## 2. Normative language

The terms **MUST**, **MUST NOT**, **SHOULD**, **SHOULD NOT**, and **MAY** are to be interpreted as requirements levels when written in uppercase.

Because this specification is a draft, requirements may change before a stable version is approved.

## 3. Conceptual boundaries

Implementations and documentation MUST distinguish these categories:

- **Agent:** a component that owns a defined decision-making or execution loop toward a task or goal.
- **Agent pattern:** a reusable control or reasoning approach used by an agent, such as ReAct or plan-and-execute.
- **Capability:** a reusable function that an agent or workflow can use, such as retrieval, memory, tool execution, or structured output.
- **Tool:** an operation exposed to an agent through a defined input/output contract.
- **Protocol or integration:** a defined means of communication or interoperability with an external system.
- **Workflow:** an explicitly described sequence, graph, or set of coordinated steps; it does not have to be autonomous.
- **Architecture:** a composition or organization of agents and other components, such as supervisor-based or hierarchical arrangements.
- **Orchestrator:** a component responsible for coordinating multiple steps, agents, or capabilities according to a workflow or policy.

A component MUST NOT be called an agent solely because it uses an LLM or appears in an agent workflow.

## 4. Model-provider independence

An application SHOULD be able to provide its own model-provider integration.

A language implementation SHOULD define a small baseline model contract that supports the minimum operations required by the agents it implements. Additional features SHOULD be represented as optional capabilities rather than forcing every provider adapter to implement every feature.

Potential optional capabilities include:

- Streaming responses.
- Tool/function calling.
- Structured or constrained output.
- Embeddings.
- Multimodal input and output.
- Provider-specific metadata or controls.

An agent MUST handle an unsupported optional capability explicitly. It MUST NOT silently assume that every model supports every feature.

Provider-specific integrations MAY live in separate packages or modules when that improves dependency isolation.

## 5. Agent execution semantics

Each agent implementation MUST document:

- The problem and intended use cases.
- The algorithm or control pattern it implements.
- Required and optional dependencies.
- Inputs, outputs, and relevant state.
- Normal termination conditions.
- Limits such as iteration count, step budget, or time budget, where applicable.
- Failure behavior and cancellation behavior.
- Known limitations and security considerations.

Where an agent performs iterative execution, it SHOULD expose explicit stopping conditions and resource limits. It MUST NOT continue indefinitely by default.

Implementations SHOULD make important execution steps observable through structured events, traces, or equivalent diagnostics without requiring applications to expose sensitive prompts, model responses, or tool data.

## 6. Tool contracts and execution

Tools SHOULD have explicit names, descriptions, input schemas, and documented outputs. Implementations SHOULD validate inputs before invoking tools and SHOULD make tool failures distinguishable from successful results.

Tool execution is a trust boundary. Implementations MUST document relevant permissions and side effects. Applications SHOULD be able to apply authorization and execution policies independently of the model's decision to request a tool.

A model-generated tool request MUST NOT be treated as authorization by itself.

## 7. Execution context and resilience

Implementations SHOULD provide a consistent way to propagate execution context, cancellation, and relevant limits through an agent run.

Failures SHOULD be represented in a way that allows applications to distinguish at least:
- Invalid input or configuration.
- Unsupported capability.
- Model-provider failure.
- Tool failure.
- Policy or authorization rejection.
- Budget or stopping-condition exhaustion.
- Cancellation or timeout.

Exact exception types, result types, and API shapes remain language-specific.

## 8. Composability

An agent SHOULD be usable independently where practical and composable with workflows or orchestrators through documented contracts.

An orchestrator MUST NOT depend on private implementation details of a particular agent. It SHOULD coordinate agents through public contracts and explicit workflow configuration.

Multi-agent coordination MUST document delegation rules, termination behavior, error propagation, and resource limits.

## 9. Cross-language compatibility

Shared specifications define semantic expectations, not identical class names, method signatures, or internal data structures.

Where cross-language equivalence matters, the project SHOULD maintain language-neutral conformance cases. These may use JSON fixtures or another documented interchange format, but such fixtures do not automatically become a required runtime wire protocol.

An implementation SHOULD publish a conformance matrix that identifies:
- Implemented specification areas.
- Unsupported or partially supported requirements.
- Intentional language-specific differences.
- Test coverage for shared behavioral expectations.

Claims of parity MUST be supported by tests or documented evidence.

## 10. Modularity and packages

Implementations SHOULD keep responsibilities separate and dependencies explicit.

A module does not automatically require its own distributable package. Package boundaries SHOULD be based on independent reuse, dependency isolation, versioning needs, and user value.

Each language implementation MUST manage its own build, test, versioning, and release lifecycle. A change in one implementation SHOULD NOT trigger unrelated releases in another.

## 11. Security and privacy

Implementations MUST treat model output, retrieved content, and external tool data as untrusted input.

Implementations SHOULD support least-privilege tool access, input validation, bounded execution, and safe handling of secrets and sensitive data. Logs and traces SHOULD avoid recording secrets or sensitive content by default.

Security requirements for a specific integration or capability SHOULD be documented alongside that component.

## 12. Specification evolution

The specification is versioned independently from implementation package versions.

Changes that affect observable behavior SHOULD include:
- A rationale and use case.
- Compatibility impact.
- Updated conformance cases where applicable.
- Migration notes when needed.

A stable specification version and conformance process will be established before implementations make stable compatibility guarantees.
