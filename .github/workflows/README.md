# GitHub Actions Workflows

This repository contains one .NET solution. GitHub Actions automation is centralized here and validates the root solution.

| Workflow | Purpose |
|---|---|
| `agentforge-ci.yml` | Restore, build, and test the complete AgentForge .NET solution |

## Release policy

There is no package-publishing workflow. NuGet publication is not part of the current development phase. Add release automation only after an explicit release decision and a review of API stability, package boundaries, versioning, and support policy.

CI should use least-privilege permissions, avoid exposing secrets, and test the same solution developers build locally.
