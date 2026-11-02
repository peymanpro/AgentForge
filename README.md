# AgentForge

**One ecosystem. Shared specifications. Independent implementations. Community driven.**

AgentForge is an open-source, multi-language ecosystem for building, composing, and orchestrating AI agents. It aims to make agent engineering understandable, modular, extensible, and practical across programming languages.

> **Project status:** Early foundation. The repository is being structured and the specifications are under design. APIs are not stable and no packages have been released yet.

## Why AgentForge?

Agent frameworks often combine agent patterns, tools, memory, retrieval, model integrations, and orchestration in ways that can make their internal behavior difficult to inspect or replace.

AgentForge is being designed around a few engineering principles:

- **Understandable implementations:** agent behavior should be implemented in inspectable, testable modules—not hidden behind names or thin wrappers.
- **Model independence:** developers bring their own model provider and integration.
- **Composable architecture:** agents, tools, capabilities, and orchestration should have clear boundaries.
- **Language-native design:** each implementation should feel natural in its own ecosystem.
- **Shared behavioral specifications:** implementations should agree on documented behavior without being forced into identical internal architectures or APIs.
- **Community ownership:** language experts are invited to build and maintain implementations for their ecosystems.

## The ecosystem

The repository is organized as a monorepo. Each language implementation is an independent project with its own source code, dependencies, tests, examples, documentation, and release process.

| Implementation | Location | Package ecosystem | Status |
|---|---|---|---|
| .NET / C# | [agentforge.net](agentforge.net/) | NuGet | Planned |
| TypeScript / NestJS | [agentforge.nestjs](agentforge.nestjs/) | npm | Planned |
| Python | [agentforge.python](agentforge.python/) | PyPI | Planned |
| Other languages | Community-proposed | Language-specific | Welcome |

The initial implementation work is expected to begin with .NET. NestJS and Python are part of the planned ecosystem, not ports that inherit .NET-specific design decisions. Additional languages can be proposed by the community.

## What belongs in AgentForge?

Agent engineering includes several related but distinct concepts. AgentForge will document and keep these boundaries explicit:

- **Agents:** components with a defined execution loop or decision-making responsibility.
- **Patterns:** approaches to controlling an agent, such as ReAct or plan-and-execute.
- **Capabilities:** reusable functions such as retrieval, memory, tool execution, or structured output.
- **Protocols and integrations:** ways to connect systems, tools, or model providers.
- **Architectures:** ways to compose multiple agents, such as supervisor-based or hierarchical systems.
- **Workflows:** explicit sequences or graphs of operations, which may or may not contain autonomous agents.

Not every capability, protocol, pattern, or workflow is itself an agent. See [the concepts guide](docs/concepts.md).

## Architecture at a glance

```text
                         AgentForge
                              |
              Shared specifications and principles
                              |
          +-------------------+-------------------+
          |                   |                   |
      .NET / C#          TypeScript             Python
          |                / NestJS                |
   Independent code    Independent code      Independent code
   tests and packages  tests and packages    tests and packages
          |                   |                   |
        NuGet                npm                 PyPI
```

The shared specifications describe observable behavior, concepts, and conformance expectations. They do not prescribe one internal design for every language.

Each implementation may define idiomatic APIs, dependency-injection patterns, asynchronous execution models, and packaging boundaries appropriate to its ecosystem.

## Bring your own model

AgentForge is intended to be model-provider agnostic. Applications should be able to supply their own model integration.

The specifications will distinguish a small set of baseline model operations from optional capabilities such as streaming, tool calling, structured output, and embeddings. An implementation should not require every provider to support every optional capability.

## Contribute in your language

We invite developers, AI engineers, researchers, and maintainers to help build AgentForge in the programming languages they know best.

You can contribute by:

- Improving the shared specifications and documentation.
- Implementing or reviewing an agent or capability.
- Starting a language implementation that does not exist yet.
- Maintaining tests, examples, integrations, or release automation.
- Reviewing cross-language behavioral compatibility.

A language implementation should have a **Language Maintainer** (or a small maintainer team) responsible for its technical quality, releases, and compatibility with the shared specifications. Maintainers are selected through the project's contribution and review process; a directory's existence does not imply that a language is officially supported.

Start with [CONTRIBUTING.md](CONTRIBUTING.md) and the [language implementation guide](docs/language-implementations.md).

## Project principles

1. **Real implementations over labels.** A module must solve a documented problem and expose behavior that can be tested.
2. **Clear boundaries.** Agents, capabilities, patterns, protocols, and orchestration must not be conflated.
3. **Independent implementations.** No language is the reference architecture for all the others.
4. **Specification before parity claims.** Cross-language compatibility must be demonstrated through documented requirements and tests.
5. **Small, justified packages.** A module does not automatically need its own distributable package; package boundaries should reflect reuse, dependency isolation, and independent versioning.
6. **Quality before breadth.** New modules and languages are added when they can be implemented, documented, tested, and maintained responsibly.

## Documentation

- [Vision](docs/vision.md)
- [Architecture decision: multi-language ecosystem](docs/decisions/0001-multilanguage-ecosystem.md)
- [Architecture](docs/architecture.md)
- [Concepts and terminology](docs/concepts.md)
- [Language implementation guide](docs/language-implementations.md)
- [Shared specification](AGENTFORGE_SPEC.md)
- [Roadmap](ROADMAP.md)
- [Contributing](CONTRIBUTING.md)
- [Security policy](SECURITY.md)
- [Code of Conduct](CODE_OF_CONDUCT.md)

## License

AgentForge is released under the [MIT License](LICENSE).
