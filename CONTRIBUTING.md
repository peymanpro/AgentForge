# Contributing to AgentForge

AgentForge is a .NET / C# project for building and studying agent-engineering systems. Contributions to algorithms, agents, capabilities, runtime infrastructure, tests, samples, documentation, and design are welcome.

This repository is intentionally .NET-only. Contributions should use the existing solution and C# conventions; do not add TypeScript, NestJS, Python, or a second language implementation here.

## Current project stage

AgentForge is an actively evolving engineering foundation. Public APIs are experimental, the project is not yet published to NuGet, and package publication is not part of the current development phase.

For a substantial new agent family or architectural capability, begin with a GitHub issue or design proposal. Agree on classification, problem statement, intended behavior, boundaries, tests, and learning value before adding a project.

## What can be contributed?

- Classical agent architectures and decision algorithms.
- LLM-based agent patterns and specialized agents.
- Multi-agent coordination architectures.
- Model-provider contracts and optional capabilities.
- Tool definitions, validation, authorization, and execution.
- State, memory, retrieval, and RAG.
- Workflow orchestration, human approval, and durable execution.
- Evaluation, observability, security, and reliability.
- Deterministic tests, runnable samples, explanations, and learning exercises.

See [the agent catalog](docs/agent-catalog.md), [capability map](docs/capability-map.md), and [learning path](docs/learning-path.md).

## Design principles

1. **Classify before implementing.** State whether the proposal is an agent, algorithm, pattern, capability, protocol, workflow, or multi-agent architecture.
2. **Solve a concrete problem.** Explain the user scenario and why the module belongs in AgentForge.
3. **Make the mechanism inspectable.** Core decision logic should be visible and documented.
4. **Keep modules composable.** Depend on public abstractions; avoid unnecessary coupling to other agents.
5. **Keep providers replaceable.** Do not leak vendor SDK types into provider-neutral contracts.
6. **Make execution bounded.** Define stopping conditions, cancellation, timeouts, and resource limits.
7. **Secure side effects.** Validate and authorize tools independently of model decisions.
8. **Test behavior, not names.** Cover success, edge cases, provider/tool failure, cancellation, and budget exhaustion.
9. **Teach through implementation.** Explain the algorithm, trade-offs, failure modes, and include a deterministic example.
10. **Avoid speculative projects.** A new source project needs a meaningful dependency or conceptual boundary.

## Requirements for a new agent module

A proposal for a new agent should include:

- Problem statement and intended task domain.
- Classification and relation to existing approaches.
- Algorithm or control loop, preferably with a lifecycle diagram.
- Required model/capability contracts, if any.
- State and memory requirements.
- Tool authority and side-effect profile.
- Termination, cancellation, and resource-budget semantics.
- Failure and partial-result behavior.
- A deterministic test strategy.
- A runnable sample and learning notes.
- Comparison with existing AgentForge modules to prevent duplication.

A catalog entry may be added before implementation, but it must be labeled Planned or Research. Do not create empty source projects to imply support.

## Repository structure

- `src/`: implemented libraries and runtime modules.
- `tests/`: focused automated test projects.
- `samples/`: runnable examples.
- `docs/`: specifications, concepts, learning material, architecture, and design notes.
- `.github/workflows/`: CI and repository automation.

Keep source projects in `AgentForge.sln` when they are intended to be buildable modules.

## Pull request checklist

- [ ] The change addresses a documented issue or explains its motivation.
- [ ] Agent/algorithm/capability/protocol/workflow classification is accurate.
- [ ] The scope is focused and avoids unrelated refactoring.
- [ ] Tests cover normal and relevant failure paths.
- [ ] Cancellation and execution limits are handled where applicable.
- [ ] Public behavior and limitations are documented.
- [ ] A runnable deterministic sample is included where useful.
- [ ] Security, privacy, and dependency implications are considered.
- [ ] The solution builds and tests pass.
- [ ] No secrets, credentials, private data, or generated build artifacts are included.

## Build and test locally

```bash
dotnet restore AgentForge.sln
dotnet build AgentForge.sln --configuration Release --no-restore
dotnet test AgentForge.sln --configuration Release --no-build --no-restore
```

## API stability and releases

Do not assume an experimental API is stable. Do not publish a package or change package publishing automation as part of an ordinary feature contribution. NuGet releases require a separate explicit maintainer decision after API, dependency, security, documentation, and compatibility review.

Thank you for helping make agent engineering understandable and testable.
