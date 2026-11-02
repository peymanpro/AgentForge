# GitHub Actions Workflows

GitHub Actions discovers workflow definitions only from this repository-level directory. Nested workflow subdirectories are not supported.

Language-specific workflows are intentionally separate:

| Workflow | Owned implementation | Trigger scope |
|---|---|---|
| `agentforge-net-ci.yml` | .NET / C# | `agentforge.net/**` and relevant shared contracts/fixtures |
| `agentforge-nestjs-ci.yml` | TypeScript / NestJS | `agentforge.nestjs/**` and relevant shared contracts/fixtures |
| `agentforge-python-ci.yml` | Python | `agentforge.python/**` and relevant shared contracts/fixtures |

## Isolation rules

- Use repository-root-relative path filters.
- A language-only source change should not run unrelated package release jobs.
- Shared specification or conformance-fixture changes should trigger the relevant affected language checks.
- Keep CI separate from package publication.
- Release jobs must publish only packages belonging to the corresponding language implementation.
- Prefer trusted publishing/OIDC where the package registry supports it.
- Use explicit permissions with least privilege.
- Do not place registry credentials in source files or commit secrets.

The workflow files are physically centralized because of GitHub Actions' repository layout requirements. Build scripts, package metadata, tests, and release responsibilities remain owned by their respective language implementations.
