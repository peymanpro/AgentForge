# AgentForge

**A modular .NET 10 library for building, running, and composing distinct agent architectures.**

AgentForge gives each independently useful agent architecture its own module and configuration API. Deterministic agents stay independent of LLM providers; model-driven agents use explicit model, tool, memory, and retrieval capabilities.

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/)
[![CI](https://github.com/peymanpro/AgentForge/actions/workflows/agentforge-ci.yml/badge.svg?branch=main)](https://github.com/peymanpro/AgentForge/actions/workflows/agentforge-ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

> **Status:** Experimental. Public APIs and module boundaries may change. NuGet packages have not yet been published.

## Contents

- [Why AgentForge](#why-agentforge)
- [Agents and Quick Starts](#agents-and-quick-starts)
  - [Simple Reflex](src/AgentForge.Agents.SimpleReflex/README.md#quick-start)
  - [Model-Based Reflex](src/AgentForge.Agents.ModelBasedReflex/README.md#quick-start)
  - [ReAct](src/AgentForge.Agents.ReAct/README.md#quick-start)
  - [Plan-and-Execute](src/AgentForge.Agents.Planning/README.md#quick-start)
  - [Reflection](src/AgentForge.Agents.Reflection/README.md#quick-start)
- [Installation](#installation)
- [Architecture](#architecture)
- [Development](#development)
- [License](#license)

## Why AgentForge

Different problems require different decision architectures. A rule-driven controller, a stateful decision-maker, a tool-using LLM agent, and a multi-agent supervisor should not be forced into one generic agent loop.

AgentForge separates:

- **Agent architectures** — independently usable modules with focused public APIs.
- **Algorithms** — reusable search, planning, optimization, and learning methods.
- **Capabilities** — optional models, tools, memory, retrieval, evaluation, security, and observability.
- **Composition** — workflows and multi-agent coordination built on shared execution contracts.

This keeps dependencies explicit and lets applications choose an architecture without importing unrelated infrastructure.

## Agents and Quick Starts

Each link opens the **Quick Start** section in that agent's own README. That document is the source of truth for its usage, behavior, dependencies, and limitations.

| Agent | Purpose | Quick Start |
|---|---|---|
| **Simple Reflex** | Selects an action using ordered condition-action rules. | [Build and run](src/AgentForge.Agents.SimpleReflex/README.md#quick-start) |
| **Model-Based Reflex** | Updates an explicit state from a percept, then selects an action. | [Build and run](src/AgentForge.Agents.ModelBasedReflex/README.md#quick-start) |
| **ReAct** | Alternates model decisions, authorized tool calls, and observations. | [Build and run](src/AgentForge.Agents.ReAct/README.md#quick-start) |
| **Plan-and-Execute** | Validates a structured plan and runs registered step handlers. | [Build and run](src/AgentForge.Agents.Planning/README.md#quick-start) |
| **Reflection** | Generates, evaluates, and refines candidates within bounded attempts. | [Build and run](src/AgentForge.Agents.Reflection/README.md#quick-start) |

The [Agent Catalog](docs/agent-catalog.md) lists planned and research-stage architectures. Catalog entries are not claims of implemented modules.

## Installation

AgentForge is not yet available on NuGet. Until package releases begin, add a project reference to the public module you need:

\`\`\`bash
dotnet add path/to/YourApp.csproj reference src/AgentForge.Agents.SimpleReflex/AgentForge.Agents.SimpleReflex.csproj
\`\`\`

Replace the project path with the module used by your application. Add only the capability projects required by that agent.

## Architecture

Each agent module follows Clean Architecture where appropriate:

- **Domain** contains the agent's concepts and rules.
- **Application** implements its decision process and use cases.
- **Infrastructure** is added only for required external adapters.
- **Public API** provides the module's composition root and user-facing construction API.

Agent modules share a small execution contract and do not depend on one another's concrete implementations. See [Architecture](docs/architecture.md), [Module Design](docs/agent-modules.md), and [Fluent API](docs/fluent-api.md).

## Development

Requirements: [.NET SDK 10.0.403](global.json).

\`\`\`bash
dotnet restore AgentForge.sln
dotnet build AgentForge.sln --configuration Release --no-restore
dotnet test AgentForge.sln --configuration Release --no-build --no-restore
\`\`\`

## License

MIT. See [LICENSE](LICENSE).
