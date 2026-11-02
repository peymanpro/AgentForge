# AgentForge MVP Scope

**Status:** Proposal for review  
**Target baseline:** AgentForge for .NET / C#  
**Related documents:** [Shared specification](../AGENTFORGE_SPEC.md), [Architecture](architecture.md), [Roadmap](../ROADMAP.md)

## 1. Purpose

The first AgentForge release should be a useful, coherent set of modular agent-engineering libraries—not merely a demo, and not an attempt to implement every known agent pattern.

The MVP must let a developer:
1. Supply a model integration of their choice.
2. Define and safely expose tools.
3. Build and run more than one genuinely implemented agent pattern.
4. Compose agents and reusable capabilities into a bounded workflow.
5. Inspect, test, and understand the execution behavior.
6. Use the packages without depending on an unnecessary collection of unrelated integrations.

The initial complete implementation is .NET / C#. NestJS/TypeScript and Python remain first-class planned implementations in the monorepo and will follow the shared specification through their own idiomatic designs.

The project is not reducing its long-term scope because other frameworks exist. This MVP is a deliberate first delivery milestone; additional agents, capabilities, integrations, and languages can be added incrementally.

## 2. Product promise

**Build, compose, and understand real AI agents through modular, inspectable, provider-independent libraries.**

The project combines two goals:
- **Practical use:** components should be useful in real applications and have predictable contracts.
- **Engineering transparency:** developers should be able to study the algorithm, lifecycle, tests, and trade-offs behind each component.

The MVP is not a production-readiness claim for every possible deployment. Security, reliability, performance, and operational claims must be earned by implementation and evidence.

## 3. Proposed .NET MVP scope

The following is the candidate feature set for the first complete .NET milestone.

### 3.1 Foundation and execution contracts

The implementation should provide the minimum shared foundation needed by agents and workflows:

- Agent input, result, and execution-context contracts.
- Explicit run options and configuration.
- Cancellation propagation.
- Bounded execution through iteration, step, or time budgets where relevant.
- Consistent, distinguishable error outcomes.
- Structured execution events for diagnostics.
- Dependency injection and async behavior that follow .NET conventions.
- Public abstractions kept small; avoid speculative universal base classes.

The core must not absorb optional integrations merely because they may be useful later.

### 3.2 Model integration abstraction

Applications must be able to provide their own model implementation.

The baseline contract should support the model operation needed by the first agents. Optional capabilities should be separate and discoverable, such as:

- Tool/function calling.
- Structured output.
- Streaming.
- Embeddings.
- Multimodal input or output.

An agent must handle an unsupported optional capability explicitly. The base contract must not force all providers to implement features they do not support.

At least one deterministic fake model should be available for tests and examples, so the core library can be evaluated without credentials or paid API calls.

### 3.3 Tool system

The initial tool system should cover:

- Tool identity and description.
- Typed inputs and outputs or equivalent schemas.
- Input validation before invocation.
- Tool registration and discovery.
- Bounded, asynchronous execution.
- Cancellation and timeout handling.
- Distinguishable validation, authorization, and execution failures.
- An application-controlled policy boundary for permissions and side effects.

A model's request to invoke a tool is not authorization to execute it. The application must be able to control which tools are available and whether a requested operation is permitted.

Retries, concurrency, and idempotency must be explicit policies rather than invisible defaults.

### 3.4 Initial agent implementations

The MVP should include a small but meaningful set of independently implemented agent patterns:

#### ReAct Agent

An inspectable loop that alternates between model decisions, approved tool execution, observations, and a final outcome.

It must have:
- A clear iteration lifecycle.
- Explicit termination rules.
- Maximum-step or equivalent limits.
- Tool error handling.
- Cancellation support.
- Tests that verify the sequence and stopping behavior.

#### Planning / Plan-and-Execute Agent

A component that separates plan generation from execution of plan steps.

It must define:
- The plan representation and validation rules.
- How steps are executed.
- How failed or blocked steps are represented.
- Whether and when replanning is allowed.
- Step and time budgets.
- The final outcome and partial-failure behavior.

Planning must be a real execution strategy, not merely a prompt asking a model to write a numbered list.

#### Reflection / Iterative Refinement Agent

A component that evaluates an initial result and uses feedback to produce a revised result under bounded conditions.

It must define:
- The evaluator contract.
- The feedback representation.
- The maximum number of refinement attempts.
- A stopping or acceptance condition.
- Behavior when evaluation fails or no improvement is observed.

Reflection must not be conflated with a generic retry loop.

These three are the proposed initial agent implementations, not the complete universe of agents that AgentForge may eventually support.

### 3.5 Reusable capabilities

The MVP should provide only the capability infrastructure needed to make the first agents useful:

- **Conversation/context state:** a basic, explicit way to carry relevant interaction context through a run.
- **Retrieval abstraction:** a provider-neutral contract for retrieving relevant items, with a simple reference implementation or test fake.
- **RAG composition:** an explicit way to retrieve context and supply it to a model-backed operation, with sources or provenance kept available where applicable.
- **Structured output support:** capability-aware handling rather than assuming every model returns schema-constrained data.

Memory strategies, database-specific vector stores, embedding providers, and advanced retrieval algorithms may be added as separate modules after their requirements are defined. A retrieval pipeline is a capability, not automatically an autonomous agent.

### 3.6 Basic orchestration

The MVP should support composition of independently built agents and capabilities through public contracts.

The initial workflow surface should be intentionally limited to well-defined composition patterns, such as:
- Sequential execution.
- Explicit handoff of outputs between steps.
- Conditional routing where the condition and outcome are observable.
- Propagation of cancellation, execution budgets, and failures.

A workflow must define what happens when a step fails and whether later steps continue. Do not promise durable, distributed, resumable graph execution in the MVP unless that behavior is implemented and tested as a separate design.

Supervisor, hierarchical, swarm, and advanced multi-agent coordination patterns remain follow-on work.

## 4. Documentation, samples, and tests

Every MVP module must include:

- A short problem statement and classification.
- An explanation of its algorithm or execution lifecycle.
- Public input and output contracts.
- Configuration and dependency guidance.
- Stopping, cancellation, and failure behavior.
- Security and resource-limit considerations.
- A minimal runnable example.
- Unit tests and relevant integration tests.

Tests must use deterministic fakes for model and tool behavior where possible. Tests should cover normal execution, invalid input, model failure, tool failure, unsupported capabilities, cancellation, and budget exhaustion.

Examples should be runnable without requiring a real provider key when a fake implementation is sufficient. Provider-specific samples may be supplied separately.

## 5. Candidate .NET package map

Package boundaries must be finalized from the actual dependency graph and user scenarios. The following names are provisional candidates, not an instruction to create a package for every class.

| Candidate package | Responsibility |
|---|---|
| `AgentForge.Abstractions` | Stable contracts shared by core modules and adapters |
| `AgentForge.Core` | Execution context, lifecycle, results, events, and common runtime |
| `AgentForge.Tools` | Tool definitions, validation, registration, and execution policies |
| `AgentForge.Agents.ReAct` | ReAct algorithm and its implementation |
| `AgentForge.Agents.Planning` | Plan-and-execute algorithm and its implementation |
| `AgentForge.Agents.Reflection` | Evaluation and iterative-refinement algorithm |
| `AgentForge.Retrieval` | Retrieval contracts and basic composition |
| `AgentForge.Orchestration` | Initial bounded workflow composition |

If two responsibilities have no meaningful independent installation, dependency, or versioning need, they should remain together. Package count is not a quality metric.

The public package names, versioning strategy, and dependency graph must be reviewed before the first NuGet publication.

## 6. Multi-language delivery strategy

The shared specification is the semantic reference, not the .NET source tree.

The delivery sequence is:

1. Finalize the behavior and conformance scenarios for the first .NET vertical slice.
2. Implement and test the .NET MVP modules.
3. Publish .NET packages only after the release criteria below are met.
4. Implement the same specified behaviors idiomatically in TypeScript/NestJS.
5. Implement the same specified behaviors idiomatically in Python.
6. Add other languages through community proposals and maintainers.

The TypeScript and Python directories are established as independent projects in the monorepo, but they should not contain empty placeholder packages that claim unsupported features. Their status must distinguish planned, in progress, preview, and stable support.

Each language controls its own API, source, tests, examples, dependencies, packaging, and release pipeline. A change in one language must not publish packages for another.

## 7. Explicitly deferred from the MVP

The following are valid future directions, but should not be added simply to make the first release look larger:

- MCP client/server integrations.
- Agent-to-agent protocols.
- Browser and computer-use automation.
- Voice and real-time multimodal agents.
- Autonomous coding agents and arbitrary code execution.
- Supervisor, hierarchical, swarm, competitive, and large-scale multi-agent systems.
- Durable distributed workflow execution and checkpoint storage.
- Production database-specific vector store integrations.
- A large provider-adapter catalogue.
- Domain-specific agent collections for every industry.

A deferred feature may move into scope when there is a concrete user scenario, an approved specification, a clear module boundary, and a test strategy.

## 8. MVP acceptance criteria

The .NET MVP is complete only when all applicable criteria are met:

### Functional

- [ ] An application can supply a model implementation without changing agent logic.
- [ ] Optional model capabilities are represented and unsupported features fail explicitly.
- [ ] Tools can be registered, validated, permission-checked, and executed.
- [ ] ReAct completes a tool-using task and stops according to defined conditions.
- [ ] Planning creates and executes a validated plan with bounded behavior.
- [ ] Reflection evaluates and revises output with bounded attempts and a clear stop condition.
- [ ] Basic retrieval can be composed into a model-backed workflow.
- [x] Basic sequential and conditional orchestration works through public contracts.

### Reliability and safety

- [ ] Cancellation is propagated through model and tool operations.
- [ ] Execution budgets are enforced.
- [ ] Tool authorization is controlled by application policy, not model output.
- [ ] Failures are distinguishable and documented.
- [ ] Sensitive prompts, results, credentials, and tool data are not logged by default.
- [ ] Tests cover normal, failure, cancellation, and limit-exhaustion paths.

### Developer experience and delivery

- [x] Each module has algorithm-level documentation.
- [x] There is a runnable deterministic workflow sample; dedicated samples for every integration remain follow-up work.
- [ ] The .NET solution builds reproducibly.
- [ ] Automated tests and quality checks pass.
- [ ] Package boundaries and dependencies are reviewed.
- [ ] NuGet metadata, README files, license, and release notes are complete.
- [ ] CI and release workflows publish only the intended .NET packages.
- [ ] Compatibility and support status are documented honestly.

A feature must not be marked complete because a class or folder exists. The acceptance evidence is working behavior, tests, documentation, and a reproducible build.

## 9. Open design questions

The following should be resolved with focused design notes and small prototypes before freezing public APIs:

1. What is the minimum model request/response contract for ReAct, and which features are optional capability interfaces?
2. How should tool schemas be represented and validated in .NET without forcing one serialization technology on every provider?
3. What is the public distinction between an agent run, a workflow run, an event stream, and a final result?
4. How are execution budgets composed when one workflow invokes multiple agents?
5. What is the minimum retrieval contract, and how should provenance and source metadata travel through the pipeline?
6. Should the first public NuGet release publish every candidate module separately, or group modules that do not yet justify independent package lifecycles?
7. Which language-neutral conformance cases can be specified without imposing a runtime serialization format?

## 10. Change control

This document is a proposal. Changing the proposed MVP scope requires:
- A concrete use case or engineering reason.
- An impact assessment on package boundaries and other languages.
- Updated acceptance criteria.
- A corresponding specification or architecture change when public behavior is affected.

The aim is not to freeze the roadmap permanently. The aim is to keep expansion intentional and testable.
