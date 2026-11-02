# AgentForge Roadmap

This roadmap describes intended direction, not promised release dates. Work may change as the architecture is validated and contributors join.

## Phase 0 — Project foundation

- [x] Create the public monorepo.
- [x] Establish project vision, terminology, and contribution principles.
- [x] Draft the shared specification.
- [ ] Review the architecture against existing agent frameworks.
- [ ] Define measurable acceptance criteria for the first implementation.
- [ ] Establish issue labels, proposal process, and maintainer governance.

## Phase 1 — Complete .NET MVP

The first complete implementation is expected to be .NET / C#. The proposed scope and acceptance criteria are documented in [MVP Scope](docs/mvp-scope.md).

- [ ] Define core agent, execution-context, result, and event contracts.
- [ ] Design provider-neutral model contracts with optional capabilities.
- [ ] Implement tool definitions, validation, registration, and policy-controlled execution.
- [ ] Implement a genuinely inspectable ReAct agent.
- [ ] Implement a bounded Planning / Plan-and-Execute agent.
- [ ] Implement a Reflection / Iterative Refinement agent with an explicit evaluator contract.
- [ ] Add basic conversation context, retrieval abstraction, and RAG composition.
- [ ] Add bounded sequential and conditional workflow composition.
- [ ] Define cancellation, timeouts, execution budgets, and failure semantics.
- [ ] Add deterministic fakes, focused tests, runnable examples, and algorithm-level documentation.
- [ ] Review package boundaries and implement independent NuGet release workflows.

The MVP is complete only when the approved scope is implemented, tested, documented, and runnable. The goal is a coherent initial library set, not every conceivable agent pattern.

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
