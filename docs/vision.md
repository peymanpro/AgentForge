# AgentForge Product Vision

AgentForge is a modular .NET library that makes it easier to construct, configure, run, and compose different kinds of agents.

The product is not a tutorial, a collection of prompt templates, or a single generic LLM wrapper. It is a library of independently usable agent modules, each built around a specific architecture or control strategy.

## Product promise

A developer should be able to:

1. Select the architecture that matches the problem.
2. Add only that agent module and the capabilities it needs.
3. Configure it through a discoverable fluent API.
4. Run it through a consistent execution contract.
5. Compose it with other agents and workflows without depending on their internal classes.
6. Replace models, tools, memory, retrieval, and infrastructure through explicit ports.

For a deterministic agent, the user should not need an LLM. For an LLM agent, the user should not be forced to use one provider. For a specialized agent, the user should not need to manually rebuild a common architecture from low-level primitives.

## Product boundaries

AgentForge has four related but distinct areas:

- **Agent modules:** complete agent architectures and patterns such as Simple Reflex, BDI, ReAct, Reflection, Supervisor, and Research Agent.
- **Algorithms:** reusable search, planning, game-playing, optimization, and learning procedures.
- **Capabilities:** model access, tools, memory, retrieval, RAG, evaluation, and observability.
- **Composition:** workflows and multi-agent coordination that combine independently built components.

A catalog entry is not automatically a standalone agent. An algorithm or capability receives its own package when it has an independently useful contract.

## Design commitments

- One module per agent type or independently meaningful architecture.
- Clean Architecture dependency direction within each module.
- A public composition root with a module-specific fluent builder.
- Shared, minimal execution contracts.
- Optional model, tools, memory, and retrieval integrations.
- Explicit state ownership and lifecycle.
- Bounded execution, cancellation, and normalized failure outcomes.
- Security and permissions at the boundary where effects occur.
- No concrete dependency from one agent module to another.
- Working tests and examples for every implemented module.

## What AgentForge is not

- Not an educational course or learning laboratory.
- Not a requirement to use an LLM for every agent.
- Not a mandatory provider SDK.
- Not a monolithic agent class with a large set of strategy flags.
- Not a workflow engine disguised as an autonomous agent.
- Not a claim that every architecture in the catalog is already implemented.

## Long-term scope

The intended scope includes classical agent architectures, symbolic and search-based decision systems, LLM patterns, specialist agents, multi-agent topologies, and the capabilities needed to run them reliably. The breadth is tracked in the [Agent Catalog](agent-catalog.md) and [Roadmap](../ROADMAP.md).
