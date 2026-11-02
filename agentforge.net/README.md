# AgentForge for .NET

**Status: In progress — experimental foundation.**

This directory contains the idiomatic .NET / C# implementation of AgentForge. Its first project is the experimental `AgentForge.Abstractions` library, which establishes the initial generic agent execution contract and normalized run outcomes.

The API is **not stable**. This foundation is being validated through tests and GitHub Actions before additional runtime, tool, and agent modules are introduced. No NuGet package has been published.

## Requirements

- .NET SDK 10.0.403 or a later patch in the .NET 10 SDK feature band.
- A supported .NET 10 runtime for running built applications.

.NET 10 is selected as the active LTS baseline for the initial implementation. The project will review its target framework and support policy as the .NET release lifecycle evolves.

## Repository structure

```text
agentforge.net/
├── AgentForge.sln
├── Directory.Build.props
├── global.json
├── src/
│   └── AgentForge.Abstractions/
└── tests/
    └── AgentForge.Abstractions.Tests/
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

- Keep public contracts small and provider-neutral.
- Preserve idiomatic .NET design without treating it as the reference architecture for other languages.
- Do not publish a package until its API, tests, documentation, and versioning policy are ready.
- Keep implementation-specific agents in separate modules when independent use and dependency boundaries justify that separation.

See the [shared specification](../AGENTFORGE_SPEC.md), [MVP scope](../docs/mvp-scope.md), and [design proposals](../docs/design/README.md).
