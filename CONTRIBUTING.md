# Contributing to AgentForge

Thank you for considering a contribution. AgentForge is intended to be a community-driven, multi-language engineering ecosystem. Contributions to specifications, implementations, tests, documentation, examples, and project operations are welcome.

Before contributing, please read the [Code of Conduct](CODE_OF_CONDUCT.md), [project concepts](docs/concepts.md), and [shared specification](AGENTFORGE_SPEC.md).

## Current project stage

AgentForge is in its foundation and design stage. The specifications and implementation plans are evolving; public APIs and package names are not stable, and no language implementation should be treated as production-ready until it is explicitly released as such.

For substantial work, open or discuss an issue before investing in a large implementation. This helps maintain consistency and prevents duplicated effort.

## Ways to contribute

- Clarify or improve the shared terminology and specifications.
- Propose an agent, capability, protocol integration, or architecture.
- Implement a feature in an existing language implementation.
- Propose a new language implementation.
- Add tests, examples, benchmarks, or documentation.
- Review changes for correctness, security, maintainability, and idiomatic design.

## Design principles for contributions

1. **Solve a clear problem.** Explain the use case and why the proposed component belongs in AgentForge.
2. **Use precise terminology.** Do not describe a protocol, capability, pattern, workflow, or multi-agent architecture as an agent unless it has agent behavior of its own.
3. **Keep implementations language-native.** Follow the idioms and ecosystem conventions of the target language.
4. **Keep the model provider replaceable.** Avoid introducing an unnecessary hard dependency on a specific model vendor.
5. **Make behavior testable.** Include tests for normal execution, edge cases, failures, cancellation, and stopping conditions where applicable.
6. **Document trade-offs.** Explain dependencies, limitations, safety implications, and behavioral decisions.
7. **Avoid speculative abstractions.** Introduce shared abstractions when they solve a demonstrated cross-module problem, not merely to anticipate every possible future feature.
8. **Keep package boundaries meaningful.** A source module does not automatically need a separate distributable package.

## Proposing a new language

A new language implementation is welcome. Before creating a substantial implementation:

1. Open a proposal describing the language, target ecosystem, intended maintainers, package strategy, and expected users.
2. Explain how the implementation will satisfy the relevant parts of the shared specification.
3. Identify language-specific design decisions rather than copying another implementation mechanically.
4. Outline testing, examples, CI, release, and maintenance plans.
5. Coordinate with the maintainers before presenting the implementation as officially supported.

A directory or proof of concept does not, by itself, mean that a language is officially supported.

## Language maintainers

Each active language implementation should have one or more Language Maintainers. They are responsible for reviewing changes in that implementation, maintaining its quality and release process, documenting deviations or unsupported capabilities, and coordinating specification changes that affect compatibility.

Maintainer status is based on sustained, demonstrated contribution and an explicit maintainer decision. It is not automatically granted by creating a directory or opening a pull request.

## Pull request checklist

Before opening a pull request, make sure that:

- [ ] The change addresses a documented issue or explains its motivation.
- [ ] The scope is focused and does not include unrelated refactoring.
- [ ] Tests are added or updated.
- [ ] Relevant tests and formatting/lint checks pass locally.
- [ ] Public behavior and limitations are documented.
- [ ] Specification changes are included when observable behavior or shared contracts change.
- [ ] Security, privacy, and dependency implications have been considered.
- [ ] The pull request description explains the problem, approach, and trade-offs.

Do not include secrets, credentials, private user data, or generated build artifacts.

## Compatibility and specifications

Changes that affect behavior shared across languages should update [AGENTFORGE_SPEC.md](AGENTFORGE_SPEC.md) or the relevant specification document. Implementations should document their conformance status and any intentional limitations.

The specification describes semantic behavior and conformance expectations. It does not require identical public APIs or internal architectures in every language.

## Communication

Use GitHub Issues for proposals, questions, and public engineering discussions. Use private reporting channels for security vulnerabilities as described in [SECURITY.md](SECURITY.md).

Please be respectful, specific, and constructive in reviews and discussions. Thank you for helping build AgentForge.
