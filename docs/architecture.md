# AgentForge Architecture

**Scope:** One modular .NET / C# solution.

AgentForge uses a layered, capability-oriented architecture. The goal is not to create a project for every noun. A source project is justified when it creates an independently understandable, testable, reusable, or dependency-isolated boundary.

## 1. Repository structure

```text
AgentForge/
├── AgentForge.sln
├── Directory.Build.props
├── global.json
├── src/
│   ├── AgentForge.Abstractions/
│   ├── AgentForge.Models/
│   ├── AgentForge.Tools/
│   ├── AgentForge.Agents.*
│   ├── AgentForge.Retrieval/
│   ├── AgentForge.RAG/
│   ├── AgentForge.Memory.*              (planned)
│   ├── AgentForge.Orchestration/
│   ├── AgentForge.MultiAgent.*          (planned)
│   ├── AgentForge.Evaluation.*          (planned)
│   ├── AgentForge.Observability.*       (planned)
│   ├── AgentForge.Security.*            (planned)
│   └── AgentForge.Protocols.*           (planned)
├── tests/
├── samples/
└── docs/
```

The entries marked planned are architectural directions, not existing projects. Create them only after the behavior and dependencies are specified.

## 2. Logical layers

### Contracts and runtime foundation

- **Abstractions:** agent contracts, run context, results, failures, events, cancellation, and limits.
- **Core runtime (planned where a separate boundary is justified):** execution services, common policies, state lifecycle, budget accounting, and runtime composition.
- **Models:** normalized model requests/responses and optional provider capabilities.

### Agent implementations

Agent projects contain their own decision logic and depend on only the contracts and capabilities they need.

Candidate families:
- `AgentForge.Agents.Classical`
- `AgentForge.Agents.ReAct`
- `AgentForge.Agents.Planning`
- `AgentForge.Agents.Reflection`
- `AgentForge.Agents.Reasoning`
- `AgentForge.Agents.ReinforcementLearning`
- `AgentForge.Agents.MultiAgent` or smaller architecture-specific modules where dependency boundaries justify them.

### Capabilities

- **Tools:** schemas, registration, validation, authorization, execution, and side-effect policies.
- **Memory:** short-term context, summaries, semantic/episodic/entity/procedural memory, stores, retention, and consolidation.
- **Retrieval:** lexical, dense, hybrid, filtered, reranked, and graph-based retrieval strategies.
- **RAG:** explicit retrieval/context/generation pipelines with source provenance.
- **Evaluation:** evaluators, datasets, judges, and regression suites.
- **Observability:** lifecycle events, traces, metrics, correlation, and privacy controls.
- **Security:** trust boundaries, policy enforcement, guardrails, approvals, and audit semantics.

### Orchestration and protocols

- **Orchestration:** sequential, conditional, graph, parallel, loop, durable, and human-in-the-loop workflows.
- **Multi-agent coordination:** delegation, messages, roles, shared state, negotiation, consensus, and termination.
- **Protocols:** provider adapters, MCP, and agent-to-agent interoperability. Protocol modules do not own agent reasoning.

## 3. Dependency direction

```text
Application
  ├── chooses Agent architecture(s)
  ├── supplies model provider(s)
  ├── configures tools and authorization policy
  ├── selects memory/retrieval/evaluation/observability
  └── composes agents with workflows or multi-agent architecture
           |
           v
       Agent runtime
           |
           v
     Minimal abstractions
```

Rules:
- Provider adapters implement model contracts; agents do not reference provider SDKs directly.
- Agent modules depend on public abstractions and selected capabilities.
- Orchestration depends on agent contracts, not concrete internal classes.
- Memory and retrieval remain separate from model transport.
- Protocol adapters translate communication; they do not bypass tool authorization.
- Application code owns credentials, tenant identity, deployment choices, and business permissions.

## 4. Adding a new agent family

Before creating a project:
1. Classify the proposal: agent, architecture, algorithm, pattern, capability, protocol, or workflow.
2. Explain the problem and target scenario.
3. Identify the algorithm and lifecycle.
4. List required contracts and optional capabilities.
5. Define state, side effects, termination, cancellation, and budgets.
6. Show why the existing projects cannot host it cleanly.
7. Prototype the smallest deterministic behavior.
8. Add tests, docs, and a sample before marking the catalog entry implemented.

New agent families should be additive. Avoid a single generic class with a growing enum and large switch statement for every agent type.

## 5. Testing layers

- **Unit tests:** decision rules, transitions, validators, policies, and algorithms.
- **Contract tests:** shared lifecycle and model/tool capability behavior.
- **Integration tests:** real adapters, stores, and protocol boundaries with test infrastructure.
- **Scenario tests:** complete tasks with deterministic models and tools.
- **Evaluation tests:** quality, groundedness, safety, latency, and cost measured against controlled cases.
- **Regression tests:** preserve known behavior when prompts, models, algorithms, or data change.

Use deterministic fakes for algorithm tests. Live LLM tests are supplementary and must not be the only evidence that an algorithm works.

## 6. Current versus planned boundaries

The source tree currently implements abstractions, model contracts, tools, ReAct, Planning, Reflection, retrieval, RAG, and sequential/conditional orchestration. Other families remain catalogued and planned. Do not infer implementation from a planned name.
