# Foundation Scope and Completion Criteria

This document records the existing .NET foundation and the standards for expanding it into a comprehensive agent-engineering toolkit. It is not a NuGet release plan.

## 1. Current baseline

The repository currently contains nine experimental source modules:

- Abstractions and run lifecycle.
- Provider-neutral chat model contracts.
- Tool definitions, validation, authorization, and execution.
- ReAct agent.
- Plan-and-execute agent.
- Reflection-based refinement agent.
- Authorization-aware lexical retrieval.
- RAG context assembly with source provenance.
- Bounded sequential/conditional orchestration.

This is a useful starting slice, not a complete agent-engineering platform.

## 2. Completion philosophy

A feature is complete only when:
- Its category and problem are clearly stated.
- Its algorithm or behavior is implemented, not merely wrapped.
- Its public contracts and dependencies are explicit.
- Success, edge cases, failure, cancellation, and limits are tested.
- Its security and privacy implications are documented.
- A deterministic example demonstrates the behavior.
- Module-level documentation explains the mechanism and trade-offs.
- The entire solution builds and tests reproducibly.

## 3. Foundation areas to strengthen

- Aggregate budgets for nested agents and workflows.
- Shared state and explicit state transitions.
- Dependency-injection and runtime composition.
- Structured model capabilities: streaming, structured output, embeddings, and multimodal.
- Tool schema and validation strategy.
- Privacy-safe tracing and model/tool call diagnostics.
- Consistent fake implementations and test fixtures.

## 4. Agent coverage

Use [Agent Catalog](agent-catalog.md) as the inventory. It contains classical, search/planning, optimization, reinforcement-learning, LLM, specialized, and multi-agent approaches. It separates implemented work from planned designs and research candidates.

The catalog is intentionally broader than the source tree. New projects should be created incrementally and only when each candidate has a concrete design and test plan.

## 5. Capabilities and production engineering

The long-term toolkit also needs state and memory, retrieval and knowledge, evaluation, observability, safety, cost and resource accounting, reliability, protocols, human approval, deployment, and specialized modalities. See [Capability Map](capability-map.md).

## 6. Release policy

No NuGet package has been published. There is no current objective to publish one. Development should prioritize a complete, coherent, well-tested, educational codebase. A future package release requires a separate decision after API stability, dependency, compatibility, security, documentation, and support policies have been reviewed.
