# Architecture Overview

**Status:** Initial architectural direction; subject to validation.

## 1. Architectural model

AgentForge separates shared meaning from language-specific implementation.

```text
                 Shared AgentForge specifications
                             |
          +------------------+------------------+
          |                  |                  |
      .NET / C#         TypeScript             Python
          |              / NestJS                |
   Idiomatic design   Idiomatic design    Idiomatic design
          |                  |                  |
   Independent tests  Independent tests  Independent tests
   and release flow   and release flow   and release flow
          |                  |                  |
        NuGet                npm                PyPI
```

The shared specification is a semantic contract. It does not define one universal class hierarchy or dictate identical APIs.

## 2. Logical responsibilities

The following are logical areas, not a mandate to create one package for each item:

- **Abstractions:** public contracts for models, agents, tools, results, and execution context.
- **Core execution:** shared lifecycle and infrastructure used by an implementation.
- **Agents:** independently understandable implementations of particular agent algorithms or patterns.
- **Tools:** tool definitions, validation, dispatch, and execution policies.
- **Retrieval and memory:** capabilities that agents may use.
- **Orchestration:** coordination of agents and workflow steps.
- **Provider integrations:** optional adapters for model providers and external systems.
- **Observability and evaluation:** diagnostics, traces, and ways to assess behavior.

A responsibility may be a source module, package, or subproject depending on the target language and dependency graph.

## 3. Dependency direction

The intended dependency direction is toward stable, minimal contracts.

- Agent implementations depend on the contracts and capabilities they actually need.
- Provider integrations implement model contracts; core agent logic does not depend on a specific vendor SDK.
- Orchestration depends on public agent contracts, not internal agent classes.
- Optional integrations should not become mandatory dependencies of the base implementation.
- Application code remains responsible for supplying credentials, deployment configuration, and application-specific policies.

The concrete dependency graph must be justified separately in each language implementation.

## 4. Model adapter design

The model abstraction should not assume that all providers expose identical features.

The baseline contract should cover the minimum operation needed by the first supported agents. Capabilities such as streaming, tool calling, structured output, and embeddings should be discoverable and optional.

This avoids two failure modes: a lowest-common-denominator API that hides useful provider features, and a large interface that every provider must implement even when it cannot support those features.

## 5. Agent module design

An agent module should make its algorithm visible. Its documentation should answer:

- What problem does it solve?
- What control loop or algorithm does it implement?
- What does it depend on?
- What state does it maintain?
- When does it stop?
- How does it respond to failures and cancellation?
- What are its safety and resource constraints?
- How can its behavior be tested?

Shared infrastructure may be reused, but agent-specific decision logic should not be hidden inside a generic strategy switch or represented by a class name alone.

## 6. Module and package boundaries

Not every source module needs an independently published package. Separate packages are justified when they provide meaningful independent installation, dependency isolation, reuse, or versioning.

The initial package map is deliberately provisional. It will be defined after the first implementation's dependency graph and user-facing use cases are understood.

## 7. Monorepo isolation

Each language directory owns its:
- Source and package metadata.
- Tests and examples.
- Build and lint configuration.
- CI workflows.
- Versioning and release process.
- Language-specific documentation.

CI should detect which language implementation or shared documents changed and run the relevant checks. Publishing should be scoped to the affected implementation and should require an intentional release event or validated release condition.

The root repository owns shared specifications, project governance, and cross-language documentation.

## 8. Cross-language conformance

Compatibility should be tested at observable behavior boundaries. For example, conformance cases may specify:
- How an agent handles a tool request.
- Whether it respects an iteration limit.
- How cancellation is surfaced.
- How unsupported model capabilities are reported.
- How execution events or final outcomes are represented.

A conformance fixture is not necessarily a runtime transport protocol. Language-native APIs and internal representations may differ.

## 9. Decisions intentionally deferred

The following decisions should be made after benchmarking and implementation evidence:
- Exact .NET solution and project boundaries.
- Exact npm and PyPI package names.
- Whether each agent is a separate distributable package.
- The complete model request/response type system.
- A shared serialization format, if one is actually needed.
- Stable conformance-suite format and versioning.
- Stable support, deprecation, and compatibility policies.

Deferring these decisions is intentional: it avoids locking the project into abstractions before real requirements are known.
