# AgentForge Roadmap

This roadmap describes intended direction, not promised release dates. Work may change as the architecture is validated and contributors join.

## Phase 0 — Project foundation

- [x] Create the public monorepo.
- [x] Establish project vision, terminology, and contribution principles.
- [x] Draft the shared specification.
- [ ] Review the architecture against existing agent frameworks.
- [ ] Define measurable acceptance criteria for the first implementation.
- [ ] Establish issue labels, proposal process, and maintainer governance.

## Phase 1 — First coherent implementation

The first implementation is expected to be .NET / C#.

- [ ] Define core execution and model abstractions.
- [ ] Design provider-neutral model contracts with optional capabilities.
- [ ] Implement a genuinely inspectable ReAct agent.
- [ ] Implement tool definition, validation, invocation, and error handling.
- [ ] Define execution context, cancellation, budgets, and tracing.
- [ ] Add focused unit and integration tests.
- [ ] Add runnable examples and algorithm-level documentation.
- [ ] Define package boundaries and NuGet release workflow.

The first milestone is complete only when its published scope is implemented, tested, documented, and runnable. It does not mean that every conceivable agent pattern has been implemented.

## Phase 2 — Additional language implementations

- [ ] Establish the TypeScript / NestJS implementation.
- [ ] Establish the Python implementation.
- [ ] Design idiomatic APIs for each ecosystem.
- [ ] Add language-specific tests and examples.
- [ ] Add shared behavioral conformance cases where they are meaningful.
- [ ] Set up independent CI and package publishing workflows.
- [ ] Identify and onboard Language Maintainers.

Language implementations may progress at different rates. Feature parity must be reported accurately rather than implied.

## Phase 3 — Expand agent capabilities

Candidate areas, subject to design review and demonstrated need:

- Planning and plan execution.
- Reflection and iterative refinement.
- Retrieval and retrieval-augmented generation.
- Memory strategies.
- Routing and delegation.
- Supervisor and multi-agent orchestration.
- Additional tool and protocol integrations, including MCP where appropriate.

Each candidate must first be classified correctly as an agent, capability, pattern, architecture, protocol integration, or workflow.

## Phase 4 — Community ecosystem

- [ ] Publish a language implementation starter guide.
- [ ] Establish conformance and compatibility reporting.
- [ ] Welcome additional language implementations.
- [ ] Build a contributor and maintainer review process.
- [ ] Add integration examples and evaluation benchmarks.
- [ ] Define stable support and deprecation policies.

## Release principles

- No package is considered released merely because source code exists in the repository.
- Each package must have a documented purpose, public API, tests, examples, versioning policy, and release workflow.
- Each language implementation owns its build and release lifecycle.
- Changes in one language should not trigger unrelated releases in another language.
- Stable compatibility claims require tests and published evidence.
