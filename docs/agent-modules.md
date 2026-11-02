# Agent Module Architecture

AgentForge is organized around independently usable agent modules. A user should be able to select one architecture, configure it through its public fluent API, and use it without importing unrelated agents or infrastructure.

## 1. Module unit

The public module package is the consumer-facing unit. Internally, it is composed of architectural layers:

```text
AgentForge.Agents.ReAct/
├── AgentForge.Agents.ReAct.csproj              # Public API + composition root
├── Builders/
├── DependencyInjection/
└── README.md

AgentForge.Agents.ReAct.Application/
├── AgentForge.Agents.ReAct.Application.csproj
├── UseCases/
└── Ports/

AgentForge.Agents.ReAct.Domain/
├── AgentForge.Agents.ReAct.Domain.csproj
├── Entities/
├── ValueObjects/
├── Policies/
└── Services/

AgentForge.Agents.ReAct.Infrastructure/          # Only when needed
├── AgentForge.Agents.ReAct.Infrastructure.csproj
├── Models/
├── Tools/
└── Persistence/
```

The exact folders should follow the domain. The structure is a guide, not a mandate to create empty projects.

### Domain

Contains the concepts and rules that define the agent architecture. It should not know about provider SDKs, HTTP, databases, tool execution implementations, or other concrete agents.

### Application

Contains the agent use cases and control loop. It defines ports for capabilities it requires, such as a model decision strategy, memory access, retrieval, or an execution boundary.

### Infrastructure

Implements those ports using provider SDKs, data stores, tool transports, clocks, queues, or external services. This layer is optional when the module has no external dependency.

### Public API and composition root

Provides the module-specific fluent builder, validated options, factory, optional DI extension, and public handle. It creates the Application use case and wires selected adapters. This is the only layer allowed to know the complete module object graph.

## 2. Dependency rules

| Layer | May depend on | Must not depend on |
|---|---|---|
| Domain | .NET base libraries | Application, Infrastructure, external providers, other agent modules |
| Application | Own Domain, AgentForge.Abstractions, capability contracts | Infrastructure implementation, provider SDK |
| Infrastructure | Own Application ports, Domain, selected capability/provider packages | Public API as a decision layer |
| Public API / composition root | Own Domain, Application, selected Infrastructure | Internal concrete types exposed as consumer requirements |

Agent modules MUST NOT depend on one another's concrete implementations. Multi-agent composition uses the common agent contract or public handle.

## 3. One architecture, one module

AgentForge separates individual agent types rather than placing several unrelated architectures into one "classical" or "LLM" module.

Initial module naming:

- `AgentForge.Agents.SimpleReflex`
- `AgentForge.Agents.ModelBasedReflex`
- `AgentForge.Agents.GoalBased`
- `AgentForge.Agents.UtilityBased`
- `AgentForge.Agents.Learning`
- `AgentForge.Agents.RuleBased`
- `AgentForge.Agents.BDI`
- `AgentForge.Agents.ReactiveSubsumption`
- `AgentForge.Agents.Deliberative`
- `AgentForge.Agents.Hybrid`
- `AgentForge.Agents.FiniteState`
- `AgentForge.Agents.BehaviorTree`
- `AgentForge.Agents.ReAct`
- `AgentForge.Agents.PlanAndExecute`
- `AgentForge.Agents.PlannerReplanner`
- `AgentForge.Agents.Reflection`
- `AgentForge.Agents.Reflexion`
- `AgentForge.Agents.SelfRefine`
- `AgentForge.Agents.Router`
- `AgentForge.Agents.TreeOfThoughts`
- `AgentForge.Agents.GraphOfThoughts`
- `AgentForge.Agents.CodeAct`

Specialized agent modules may include Research, Coding, Browser, ComputerUse, DataAnalysis, SQL, DocumentProcessing, CustomerSupport, Conversational, Voice, and Multimodal agents.

Multi-agent architecture modules may include Supervisor, Hierarchical, PlannerWorker, PeerCollaboration, GroupChat, Debate, CriticGenerator, Blackboard, ContractNet, Auction, Consensus, and Swarm.

The catalog determines which names are true agent modules and which are patterns, algorithms, capabilities, or workflows.

## 4. Shared packages

Agent modules use shared packages selectively:

- `AgentForge.Abstractions`: generic execution contracts, result types, contexts, events, and the common agent handle.
- `AgentForge.Runtime`: optional runtime services such as budgets, lifecycle, middleware, state management, and execution pipelines.
- `AgentForge.Models`: provider-neutral model contracts and normalized messages.
- `AgentForge.Tools`: tool contracts, schema validation, authorization, and execution.
- `AgentForge.Memory.*`: memory contracts and storage implementations.
- `AgentForge.Retrieval.*` and `AgentForge.RAG`: retrieval and grounded context capabilities.
- `AgentForge.Search.*`, `AgentForge.Planning.*`, `AgentForge.Optimization.*`, `AgentForge.ReinforcementLearning.*`: reusable algorithms rather than complete agents.
- `AgentForge.Orchestration` and `AgentForge.MultiAgent.*`: workflow and coordination capabilities.
- `AgentForge.Evaluation`, `AgentForge.Security`, `AgentForge.Observability`: production capabilities.
- `AgentForge.Providers.*` and `AgentForge.Protocols.*`: model, MCP, and agent-to-agent integrations.

A Simple Reflex module should depend only on abstractions. ReAct may depend on model and tool contracts. A Research Agent may optionally use memory, retrieval, and RAG. Dependencies follow the agent's actual needs.

## 5. Module implementation checklist

A module is ready to be marked implemented when it has:

1. A precise architecture/algorithm classification and target use case.
2. Domain entities and rules without infrastructure dependencies.
3. Application use cases with explicit ports and stopping behavior.
4. Infrastructure adapters only where external I/O is required.
5. A module-specific fluent builder with build-time validation.
6. A common execution handle.
7. Tests for success, no-op, invalid configuration, failure, cancellation, and budgets.
8. A runnable example and concise module README.
9. Architecture checks for dependency direction.
10. Solution integration and passing CI.

## 6. Why not one universal builder?

A universal builder with every possible method would allow invalid combinations and make simple agents carry irrelevant concepts. Each module therefore owns its fluent API. Shared builder primitives may standardize naming, validation, options, and execution, but the configuration surface remains specific to the agent's architecture.


## 7. Documentation ownership and discoverability

Documentation has two complementary levels:

- The repository-root README is the product entry point. It explains why AgentForge exists, helps users choose an architecture, summarizes the available and planned agents, and links to the detailed module documentation.
- Every public agent module MUST contain its own README.md in the module's public project directory. This README is the source of truth for that agent's behavior and usage.

A module README should cover, where relevant:

1. The problem it solves and the architecture or pattern it implements.
2. The decision loop or algorithm, including stop and acceptance conditions.
3. A minimal, executable fluent-API example.
4. Required and optional dependencies.
5. Domain, Application, optional Infrastructure, and public API responsibilities.
6. State ownership, concurrency expectations, and persistence boundaries.
7. Budgets, timeouts, cancellation, failures, and partial outcomes.
8. Tool permissions, trust boundaries, and side-effect behavior.
9. Known limitations and current implementation status.
10. Links to deeper design documents and runnable samples.

The root README MUST link to every agent module that currently exists. The Agent Catalog records planned and research candidates; it must not imply that a planned agent already has a module. When a module is added or its public API changes, update its module README and the root README in the same change.

Cross-cutting architecture decisions belong in the central docs/ tree. Avoid copying a complete module specification into the root README: the root document is the map, while the module README is the detailed guide for that module.
