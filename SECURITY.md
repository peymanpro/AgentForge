# Security Policy

Security is an important part of AgentForge, especially because agent systems may interact with external models, tools, data sources, and execution environments.

## Reporting a vulnerability

Please do **not** disclose suspected vulnerabilities in a public issue, discussion, or pull request.

Report a vulnerability privately to the repository owner or a project maintainer through their GitHub profile. Include, where possible:

- A concise description of the affected component and impact.
- The affected repository path, version, or commit.
- Reproduction steps or a proof of concept that does not expose real secrets or personal data.
- Any suggested mitigation.

Please allow maintainers reasonable time to investigate and coordinate a fix before publicly disclosing the issue.

## Supported versions

AgentForge is currently in its foundation stage and has not established a released-version support policy. Once language implementations publish stable releases, each implementation should document its supported versions and security-update policy.

## Security expectations

Contributors should:

- Never commit API keys, credentials, tokens, or private data.
- Validate and constrain tool inputs and outputs.
- Treat model output and retrieved content as untrusted input.
- Consider authorization, least privilege, isolation, timeouts, and resource limits when executing tools.
- Avoid logging secrets or sensitive prompts and responses by default.
- Document security-relevant assumptions and limitations.

These are project expectations, not a claim that the current repository or future implementations have already been security-audited.
