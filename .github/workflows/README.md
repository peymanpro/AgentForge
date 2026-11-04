# GitHub Actions Workflows

This repository contains one .NET solution. GitHub Actions automation is centralized here and validates the root solution.

| Workflow | Purpose |
|---|---|
| `agentforge-ci.yml` | Restore, build, test, pack, and verify the complete AgentForge .NET solution |
| `publish-nuget.yml` | On an explicitly marked release commit that updates `Directory.Build.props`, rebuild, test, verify, and publish all distributable NuGet packages through NuGet Trusted Publishing |

## NuGet release process

1. Update the central `<Version>` in `Directory.Build.props` for each release.
2. Ensure the PR title starts with `release:`. The publishing workflow runs only after that PR is merged to `main` and the version file is part of the push.
3. Configure the NuGet Trusted Publishing policy for:
   - Repository owner: `peymanpro`
   - Repository: `AgentForge`
   - Workflow file: `publish-nuget.yml`
   - Environment: leave empty (the workflow does not use a GitHub Environment).
4. Add the NuGet.org profile name as the GitHub Actions repository variable `NUGET_USER` (Settings → Secrets and variables → Actions → Variables).
5. The workflow uses OIDC through `NuGet/login@v1`; no long-lived NuGet API key is stored in GitHub.

The workflow publishes all 23 packages in dependency order: shared foundations, internal Domain/Application dependencies, and then the 10 installable agent modules. It verifies package metadata and embedded agent READMEs before publishing, then checks each package version on NuGet.org.

The workflow can be safely retried after a partial publish because package pushes use `--skip-duplicate`. Never reuse a published package version for changed contents; increment the central version for every release.

## Security policy

CI and release jobs use least-privilege permissions. Only the release job receives `id-token: write`, and the NuGet credential is short-lived and generated immediately before package publication.
