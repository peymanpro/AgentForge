# AgentForge for .NET

**Status: In progress — experimental foundation.**

This directory contains the idiomatic .NET / C# implementation of AgentForge. The initial implementation is being built as small, composable projects with provider-neutral contracts and explicit execution boundaries.

The APIs are **not stable**. No NuGet packages have been published.

## Requirements

- .NET SDK 10.0.403 or a later patch in the .NET 10 SDK feature band.
- A supported .NET 10 runtime for running built applications.

.NET 10 is the initial LTS baseline. Its target framework and support policy will be reviewed as the .NET release lifecycle evolves.

## Current projects

| Project | Responsibility | Status |
|---|---|---|
| `AgentForge.Abstractions` | Generic agent execution contract, run context, limits, and normalized outcomes | Experimental |
| `AgentForge.Tools` | Tool declarations, validation, immutable registry, application authorization, and execution | Experimental |
| `AgentForge.Models` | Provider-neutral chat messages, model requests/responses, and optional native tool-calling capability | Experimental |
| `AgentForge.Agents.ReAct` | Bounded ReAct loop with replaceable decision strategy and policy-controlled tools | Experimental |
| `AgentForge.Agents.Planning` | Structured plan validation and sequential execution through registered step handlers | Experimental |
| `AgentForge.Agents.Reflection` | Bounded candidate generation, independent evaluation, and iterative refinement | Experimental |

No vendor-specific provider adapter is included yet. Applications will be able to implement `IChatModel` and, where supported, `IToolCallingChatModel` using their preferred provider SDK or HTTP integration.

## Repository structure

```text
agentforge.net/
├── AgentForge.sln
├── Directory.Build.props
├── global.json
├── src/
│   ├── AgentForge.Abstractions/
│   ├── AgentForge.Agents.ReAct/
│   ├── AgentForge.Agents.Planning/
│   ├── AgentForge.Agents.Reflection/
│   ├── AgentForge.Models/
│   └── AgentForge.Tools/
└── tests/
    ├── AgentForge.Abstractions.Tests/
    ├── AgentForge.Agents.ReAct.Tests/
    ├── AgentForge.Agents.Planning.Tests/
    ├── AgentForge.Agents.Reflection.Tests/
    ├── AgentForge.Models.Tests/
    └── AgentForge.Tools.Tests/
```

## Build and test

Run these commands from this directory:

```bash
dotnet restore AgentForge.sln
dotnet build AgentForge.sln --configuration Release --no-restore
dotnet test AgentForge.sln --configuration Release --no-build --no-restore
```

The repository-level workflow at `.github/workflows/agentforge-net-ci.yml` runs the same validation for relevant changes.

## Design constraints

- Keep model contracts provider-neutral and represent optional provider capabilities explicitly.
- A model-generated tool call is a proposal, not permission to execute.
- Planning uses structured plans, validates step types and output references before execution, and stops on the first failed step.
- Reflection separates candidate generation from evaluation, preserves the best known candidate, and enforces an explicit attempt limit.
- ReAct runs are bounded by model-turn, tool-invocation, step, and timeout limits.
- Tool input validation and application authorization happen before tool execution.
- Keep each project independently understandable and testable.
- Do not publish a package until its API, tests, documentation, and versioning policy are ready.
- Do not treat the .NET implementation as the reference architecture for other languages.

See the [shared specification](../AGENTFORGE_SPEC.md), [MVP scope](../docs/mvp-scope.md), and [design proposals](../docs/design/README.md).
