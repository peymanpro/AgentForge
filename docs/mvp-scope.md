# Current Product Baseline

This document records what exists today and what remains part of the implementation program. AgentForge is a library; examples and documentation support the developer experience but are not the product goal.

## 1. Implemented module slices

The current repository contains these experimental modules and capabilities:

- `AgentForge.Abstractions`: generic agent execution contract, normalized results, context, run options, and event hooks.
- `AgentForge.Models`: provider-neutral chat request/response contracts.
- `AgentForge.Tools`: tool schema, registry, validation, authorization, and execution.
- `AgentForge.Agents.SimpleReflex`: separate Domain/Application/public projects and a fluent builder.
- `AgentForge.Agents.ModelBasedReflex`: separate Domain/Application/public projects and a fluent builder.
- `AgentForge.Agents.ReAct`: existing experimental implementation; Clean Architecture and fluent API migration pending.
- `AgentForge.Agents.Planning`: existing experimental implementation; Clean Architecture and fluent API migration pending.
- `AgentForge.Agents.Reflection`: existing experimental implementation; Clean Architecture and fluent API migration pending.
- `AgentForge.Retrieval`: authorization-aware lexical retrieval baseline.
- `AgentForge.RAG`: context assembly with source provenance.
- `AgentForge.Orchestration`: bounded sequential and conditional workflow composition.

These modules do not yet represent a complete or production-ready agent platform.

## 2. Architecture migration work

- Migrate existing LLM agent modules to the same Domain/Application/Infrastructure/Public API conventions.
- Add module-level fluent builders.
- Extract only genuinely shared runtime concerns into shared packages.
- Add compile-time project boundaries and architecture tests.
- Prevent cross-module concrete dependencies.

## 3. Agent coverage program

See [Agent Catalog](agent-catalog.md) and [Roadmap](../ROADMAP.md). The remaining planned coverage includes classical agent types, symbolic/search/planning algorithms, optimization, reinforcement learning, additional LLM patterns, specialist agents, and multi-agent topologies.

## 4. Capability coverage program

Memory, persistent sessions, provider adapters, structured output, streaming, embeddings, multimodal integration, vector/hybrid retrieval, reranking, evaluation, security, observability, protocols, durable workflows, and deployment support remain to be built or expanded.

## 5. Definition of complete

A module is complete only when:
- It has a public fluent builder and common execution handle.
- Its Domain and Application boundaries follow dependency rules.
- Required external capabilities are represented by ports.
- Infrastructure exists only where necessary.
- Build-time configuration is validated.
- Its key behavior, failure paths, cancellation, and limits are tested.
- A runnable usage example exists.
- Full-solution CI is green.

## 6. Release policy

No NuGet package has been published. NuGet release remains a later decision after APIs, dependency policies, compatibility, security, documentation, and support expectations are reviewed.
