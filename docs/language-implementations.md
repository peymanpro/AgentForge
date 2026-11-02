# Language Implementation Guide

This guide explains how to propose and maintain an AgentForge implementation in a programming language.

## 1. Design goals

A language implementation should:
- Follow the shared semantic specifications.
- Use idiomatic APIs and conventions for its ecosystem.
- Keep dependencies and optional integrations explicit.
- Provide inspectable agent algorithms.
- Include tests, practical examples, and implementation-specific documentation.
- Maintain an independent build, versioning, and publishing lifecycle.

Do not mechanically port types or architectural patterns from another language.

## 2. Suggested directory structure

The exact structure is language-specific. A starting point may look like:

```text
agentforge.<language>/
├── README.md
├── src-or-packages/
├── tests/
├── examples/
└── docs/
```

Use the conventions of the language ecosystem. For example, a .NET implementation may use a solution and projects, a TypeScript implementation may use a workspace, and a Python implementation may use a standard Python packaging layout.

## 3. GitHub Actions location and isolation

GitHub only discovers workflow definitions in the repository-root `.github/workflows/` directory. Nested directories under `.github/workflows/` are not supported, so a workflow stored at `agentforge.net/.github/workflows/ci.yml` will not run as a GitHub Actions workflow.

AgentForge should use separate root-level workflow files, for example:

- `.github/workflows/agentforge-net-ci.yml`
- `.github/workflows/agentforge-nestjs-ci.yml`
- `.github/workflows/agentforge-python-ci.yml`

Each language workflow should use repository-root-relative path filters for its own implementation directory. Shared specification or conformance-fixture changes should trigger the relevant language checks. Release workflows should remain separate from ordinary CI and publish only the intended language packages.

This physical centralization does not make the implementations dependent on one another. It is a GitHub Actions repository constraint; logical ownership and release isolation remain language-specific.

See [GitHub's workflow documentation](https://docs.github.com/en/actions/concepts/workflows-and-actions/workflows) and [workflow syntax for path filters](https://docs.github.com/en/actions/reference/workflows-and-actions/workflow-syntax).

## 4. Minimum readiness checklist

Before an implementation is presented as active or supported, it should have:

- A named maintainer or maintainer team.
- A documented installation and setup path.
- At least one working, documented end-to-end example.
- Automated tests for the implemented behavior.
- A documented conformance matrix.
- A clear list of limitations and unsupported capabilities.
- A reproducible build and test process.
- A release and security-maintenance plan.

A placeholder directory or proof of concept should be labeled accordingly.

## 5. Model integrations

Model integrations should be replaceable and should expose provider-specific capabilities honestly. Do not claim that an adapter supports tool calling, streaming, structured output, embeddings, or multimodal inputs unless those features are implemented and tested.

Credentials should be supplied by the consuming application or a secure configuration mechanism, never committed to the repository.

## 6. Package and release strategy

Choose package boundaries based on actual user needs, dependency isolation, and independent versioning. Avoid publishing one package per class or creating many packages before their boundaries are understood.

Each language implementation owns its:
- Package metadata and naming.
- Versioning policy.
- Build and test workflows.
- Release automation and registry configuration.
- Changelog and release notes.
- Compatibility and deprecation policy.

Publishing should be isolated so that a change to one language does not release packages for other languages.

Where supported, prefer trusted publishing or OIDC-based registry authentication over long-lived publish tokens. Registry-specific setup must be documented and verified before enabling releases.

## 7. Shared behavioral conformance

Implementations should add tests for shared behavior where that behavior is defined by the specification. Tests may use language-neutral fixtures, test vectors, or equivalent scenarios.

Conformance does not require identical public APIs or identical internal state representation. Differences that affect observable behavior must be documented.

## 8. Maintainer responsibilities

Language Maintainers review implementation-specific changes, keep tests and examples healthy, own releases, document unsupported features, and coordinate shared specification updates.

Maintainers should avoid silently diverging from shared requirements. When a specification is unclear or unsuitable for a language, open a discussion or specification proposal rather than making an undocumented interpretation.
