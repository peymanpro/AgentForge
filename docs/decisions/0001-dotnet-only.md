# ADR 0001: Make AgentForge a Single .NET Repository

- **Status:** Accepted
- **Date:** 2026-11-02
- **Decision owners:** AgentForge maintainers

## Context

AgentForge initially began as a multi-language monorepo with separate .NET, NestJS/TypeScript, and Python directories. The project direction has changed: its primary purpose is to become one coherent .NET agent-engineering library.

Keeping unrelated language roots, release concepts, and cross-language specifications in the same repository adds maintenance and conceptual overhead without serving the current goal.

## Decision

AgentForge is a single .NET / C# repository.

- Move the .NET solution, source, tests, samples, and build configuration to the repository root.
- Remove the NestJS and Python directories.
- Replace cross-language specifications and governance with .NET-specific engineering contracts.
- Keep the modular source-project structure.
- Maintain a broad catalog of classical, LLM-based, specialized, and multi-agent approaches.
- Add source modules incrementally when each has a justified boundary, working implementation, tests, documentation, and sample.
- Keep APIs experimental while the architecture is evolving.
- Do not publish NuGet packages in the current development phase.

## Consequences

### Positive

- The repository has one coherent language, toolchain, solution, CI path, and contribution workflow.
- Contributors can navigate the project without a nested implementation directory.
- All modules, contracts, tests, and integrations can follow one consistent C# toolchain.
- Agent families can be added as modular projects without implying cross-language parity.

### Trade-offs

- TypeScript/NestJS and Python implementations are no longer part of this repository.
- Some prior cross-language design notes must be removed or rewritten.
- The project must be careful not to turn broad catalog coverage into premature source-project proliferation.

## Revisit conditions

Revisit this decision only if there is a concrete, separately governed need to change the repository's single-.NET scope. A new language implementation should not be added casually.
