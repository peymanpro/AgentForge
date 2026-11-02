# AgentForge .NET Engineering Specification

**Status:** Draft — evolving with implementation  
**Scope:** AgentForge's .NET / C# architecture, behavioral contracts, and engineering expectations

This document describes the concepts and observable behaviors that AgentForge modules should follow. It is specific to this repository's .NET implementation. It does not define a cross-language API.

## 1. Product boundary

AgentForge is a modular .NET library and an educational engineering codebase for classical agents, LLM-based agents, agent capabilities, workflows, and multi-agent systems.

The repository contains one .NET solution. Source projects, tests, samples, build configuration, documentation, and automation are maintained as one C# project.

## 2. Normative language

**MUST**, **MUST NOT**, **SHOULD**, **SHOULD NOT**, and **MAY** express requirement strength. This specification is a draft; public APIs remain experimental until explicitly stabilized.

## 3. Domain vocabulary

The following categories MUST remain distinct:

- **Agent:** an entity or software component that perceives relevant state and selects or performs actions toward a goal under a defined control loop or policy.
- **Agent model / architecture:** a structural model of how an agent perceives, reasons, decides, learns, and acts; examples include reflex, goal-based, utility-based, BDI, reactive, deliberative, and hybrid architectures.
- **Agent algorithm:** a concrete procedure used for search, planning, learning, optimization, or decision selection; examples include A*, minimax, Q-learning, and Monte Carlo Tree Search. An algorithm is not automatically a complete agent.
- **Agent pattern:** a reusable control strategy used by an agent; examples include ReAct, plan-and-execute, reflection, and routing.
- **Capability:** a reusable function available to an agent or workflow; examples include memory, retrieval, tool execution, structured output, and evaluation.
- **Tool:** an explicitly declared operation with an input/output contract, validation, side-effect profile, and execution policy.
- **Protocol / integration:** a communication contract with an external model, tool server, agent, or service; examples include provider APIs, MCP, and agent-to-agent protocols.
- **Workflow:** an explicit sequence or graph of operations. A workflow can coordinate agents without being autonomous itself.
- **Multi-agent architecture:** a system in which multiple agents coordinate, delegate, collaborate, compete, or negotiate.
- **Runtime:** shared machinery for execution context, cancellation, budgets, events, state, and lifecycle handling.

A module MUST NOT be called an agent merely because it calls an LLM or appears inside an agent workflow.

## 4. Modularity and dependency direction

- Shared contracts MUST remain minimal and technology-neutral where practical.
- Agent implementations SHOULD depend on abstractions and only the capabilities they use.
- Provider adapters MUST isolate vendor SDK dependencies from agent algorithms.
- Orchestration MUST use public contracts rather than private implementation details of agents.
- Optional integrations MUST NOT become mandatory dependencies of the base runtime without a demonstrated need.
- A new source project MUST have a meaningful responsibility, tests, documentation, and dependency rationale.
- New agent families MUST be addable without modifying unrelated agent implementations.

## 5. Model-provider independence

Applications SHOULD supply their own model integration. The baseline model contract SHOULD remain small. Optional capabilities MAY include tool calling, structured output, streaming, embeddings, multimodal input/output, token counting, and provider-specific metadata.

An agent MUST detect unsupported capabilities explicitly. It MUST NOT silently assume every provider supports every feature.

Provider-specific SDKs MUST remain outside provider-neutral contracts.

## 6. Agent execution

Every iterative agent MUST document and enforce the limits relevant to its behavior, including:
- Step, iteration, tool-call, model-call, token, time, or cost budgets.
- Termination and acceptance conditions.
- Cancellation propagation.
- Partial-result and failure behavior.
- State mutation and persistence boundaries.
- Side effects and required permissions.

Agent execution SHOULD emit structured lifecycle metadata. Sensitive prompts, responses, user input, credentials, retrieved content, and tool payloads MUST NOT be logged by default.

## 7. Tool trust boundary

A model-generated tool request is a proposal, not permission to execute.

Before a tool runs, the runtime/application MUST be able to:
1. Resolve the requested tool from an explicit registry.
2. Validate its input against its declared contract.
3. Apply authentication, authorization, and policy checks.
4. Enforce time, concurrency, and resource limits.
5. Record a privacy-safe execution outcome.

Tool side effects, idempotency, retry behavior, and compensation semantics MUST be explicit.

## 8. State, memory, and retrieval

The runtime MUST distinguish transient run state from persistent memory. Memory strategies SHOULD be replaceable and have explicit retention, privacy, tenant, and deletion semantics.

Retrieval MUST apply scope and authorization constraints before exposing retrieved content. Retrieved text, documents, and external observations MUST be treated as untrusted data, not as instructions.

RAG is a composition of retrieval and model-backed generation, not automatically an autonomous agent. Source provenance SHOULD remain available through context assembly and final outputs where the application needs it.

## 9. Orchestration and multi-agent systems

A workflow or multi-agent architecture MUST document:
- Its topology and control policy.
- Data passed between steps or agents.
- Delegation and authority boundaries.
- Stop, timeout, cancellation, and failure behavior.
- Aggregate resource-budget semantics.
- Human approval requirements for consequential actions.

Parallelism, retries, durable checkpoints, resumption, dynamic graph mutation, and distributed scheduling MUST be treated as explicit capabilities, not assumed properties of a workflow API.

## 10. Evaluation and observability

Evaluation MUST distinguish correctness, relevance, task completion, groundedness, safety, latency, cost, and robustness rather than collapsing them into one unexplained score.

Observability SHOULD support correlation IDs, lifecycle events, traces, metrics, and diagnostic metadata. Sensitive content MUST be opt-in and redacted by default.

Agent implementations SHOULD be tested with deterministic models, tool fakes, controlled clocks, and repeatable fixtures wherever practical.

## 11. Security and resilience

Implementations MUST treat model output, retrieved content, tool output, and external protocol messages as untrusted.

The architecture SHOULD support least privilege, explicit trust boundaries, bounded execution, secret isolation, input validation, output validation, prompt-injection-aware data handling, and auditable authorization decisions.

Cancellation is cooperative; documentation MUST state where downstream systems may continue side effects despite cancellation.

## 12. Documentation and completion evidence

Each module MUST explain:
- The problem and classification of the component.
- The algorithm, architecture, or capability it implements.
- Public contracts, configuration, dependencies, and extension points.
- Execution lifecycle and state transitions.
- Failure, cancellation, stopping, and resource limits.
- Security/privacy implications and known limitations.
- Tests, a runnable example, and trade-offs.

A feature MUST NOT be marked complete because a type or folder exists. Completion requires working behavior, tests, documentation, and reproducible build evidence.

## 13. Evolution

A design change that alters observable behavior SHOULD include rationale, alternatives, compatibility impact, tests, and migration notes. Experimental APIs may evolve before stabilization. NuGet publication and semantic-version guarantees require a separate release decision.
