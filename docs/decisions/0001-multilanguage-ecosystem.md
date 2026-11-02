# ADR 0001: Multi-language ecosystem with shared specifications

- **Status:** Accepted
- **Date:** 2026-11-02
- **Decision owners:** AgentForge project maintainers
- **Scope:** Repository organization, language implementations, shared behavior, and community contributions

## Context

AgentForge is intended to be an open-source ecosystem of modular agent-engineering libraries across multiple programming languages.

The project began with .NET, TypeScript/NestJS, and Python as planned implementations. The community should also be able to propose and maintain implementations for other languages.

A single-language reference architecture would make other implementations ports of the first language rather than independent projects. At the same time, completely unrelated implementations would fragment the ecosystem and make shared behavior difficult to understand or validate.

The project therefore needs a clear boundary between what is shared and what remains language-specific.

Existing frameworks and libraries are useful references for learning, comparison, and identifying proven engineering approaches. Their existence is not a reason to abandon AgentForge or to define the project's purpose only in opposition to them.

## Decision

AgentForge will be developed as a **multi-language ecosystem of independent, modular implementations governed by shared behavioral specifications and community principles**.

The decision has the following parts.

### 1. One ecosystem, multiple implementations

The root repository owns:
- Project vision and governance.
- Shared terminology and specifications.
- Cross-language conformance scenarios where appropriate.
- Contribution, security, and community policies.
- Ecosystem-wide documentation and roadmap.

Each language implementation owns:
- Idiomatic source code and public APIs.
- Internal architecture and dependency choices.
- Tests and runnable examples.
- Build, packaging, versioning, and release workflows.
- Implementation-specific documentation and support policy.

Initial planned implementations are:
- `agentforge.net` — .NET / C#.
- `agentforge.nestjs` — TypeScript / Node.js, with NestJS integration.
- `agentforge.python` — Python.

Other languages may be proposed and added through the community process.

### 2. Shared semantics, not identical internals

The shared specifications define concepts, externally observable behavior, and conformance expectations.

They do **not** require:
- Identical class names or method signatures.
- Identical internal algorithms where language or ecosystem needs justify a different design.
- One language's dependency-injection or asynchronous model to be copied by another.
- Every implementation to support every optional capability at the same time.

An implementation must document its support status, intentional differences, and limitations. Compatibility claims must be supported by tests or other reviewable evidence.

### 3. Real modular implementations

A component should be introduced to solve a defined engineering problem. Its documentation should classify it accurately as an agent, pattern, capability, tool, protocol integration, workflow, or architecture.

An agent implementation should expose its actual behavior, execution lifecycle, stopping conditions, failure paths, and relevant state transitions. A class name or thin wrapper around a generic dispatcher is not sufficient evidence of an implemented agent algorithm.

Modules and distributable packages are not necessarily the same thing. Package boundaries should be chosen according to independent reuse, dependency isolation, versioning, and user value.

### 4. Provider independence

AgentForge should allow applications to supply their own model integrations. Baseline model operations and optional provider capabilities must be distinguished.

An implementation must not imply that all providers support optional features such as streaming, tool calling, structured output, embeddings, or multimodal input. Unsupported capabilities must be represented and handled explicitly.

### 5. Independent delivery and releases

Each language implementation has its own build, test, package, and release lifecycle.

A change to one language must not trigger unrelated package releases for another language. Where supported, trusted publishing and short-lived identity-based authentication should be preferred over long-lived publishing secrets.

A package should be published only when its implementation, tests, documentation, versioning, and release process justify publication.

### 6. Community-driven language ownership

The project welcomes language experts and maintainers from the wider developer community.

An active language implementation should have an explicitly recognized Language Maintainer or maintainer team. Maintainers are responsible for implementation quality, tests, documentation, releases, and reporting compatibility with shared specifications.

Creating a folder or proof of concept does not automatically make an implementation officially supported.

## Consequences

### Positive consequences

- Developers can use AgentForge in the language and ecosystem they already work with.
- Each implementation can be idiomatic rather than a mechanical port.
- Shared specifications can make cross-language behavior understandable and testable.
- Independent release pipelines reduce unnecessary coupling.
- Community maintainers can own implementation-specific decisions.
- Source code and tests can serve as practical learning material for agent engineering.

### Costs and trade-offs

- Maintaining multiple implementations increases review, testing, and release work.
- Shared specifications require careful versioning and governance.
- Cross-language conformance tests must avoid enforcing accidental implementation details.
- Feature parity will take time and must not be claimed prematurely.
- Maintainer onboarding and continuity become important project responsibilities.

These costs are accepted as part of the multi-language ecosystem goal. The project will manage them through incremental delivery and explicit support status rather than attempting simultaneous feature parity.

## Constraints and deferred decisions

This ADR does not yet decide:
- The exact package/project boundaries within any language.
- The full list of initial agents or capabilities.
- The exact model abstraction or tool API.
- The conformance fixture format.
- Stable API, support, or deprecation guarantees.
- Whether a particular language should be promoted from planned to officially supported.

Those decisions require focused design proposals and working examples.

## Validation criteria

This decision will be considered successfully implemented when:

1. At least two language implementations can implement a shared, documented behavior without copying the same internal architecture.
2. Shared conformance scenarios can validate selected observable behaviors.
3. Each implementation can build, test, and release independently.
4. A contributor can propose a new language implementation using documented project guidance.
5. Maintainers can clearly communicate implementation status, supported features, and known limitations.

## Revisit conditions

Revisit this ADR if independent implementations become too costly to maintain, if shared specifications create more friction than value, or if evidence from real users and contributors indicates that the ecosystem structure should change.

Any revision should preserve the project's commitment to accurate documentation, inspectable implementations, and respectful community governance.
