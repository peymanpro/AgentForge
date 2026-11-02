# Design Notes

Design notes document engineering choices before they become stable public APIs. They are specific to the AgentForge .NET solution.

A proposal is not an API guarantee. Its status must be updated when a design is implemented, revised, or rejected.

## Runtime and model foundations

| Document | Responsibility | Current status |
|---|---|---|
| [Agent Execution Contract](agent-execution-contract.md) | Agent run lifecycle, context, results, events, cancellation, budgets, and failure semantics | Design proposal; implemented foundation is experimental |
| [Model Provider Contract](model-provider-contract.md) | Provider-neutral model contracts and optional capabilities | Design proposal; baseline contract implemented experimentally |
| [Tool Execution Contract](tool-execution-contract.md) | Tool schemas, validation, authorization, bounded execution, and side effects | Design proposal; baseline implementation is experimental |

## Agent algorithms

| Document | Responsibility | Current status |
|---|---|---|
| [ReAct Execution](react-agent-execution.md) | Decision/action/observation lifecycle and stopping behavior | Implemented experimentally |
| [Planning Execution](planning-agent-execution.md) | Structured plan generation, validation, execution, and failure policy | Implemented experimentally |
| [Reflection Execution](reflection-agent-execution.md) | Evaluation, feedback, bounded refinement, and acceptance | Implemented experimentally |

## Knowledge and orchestration

| Document | Responsibility | Current status |
|---|---|---|
| [Retrieval and RAG](retrieval-rag.md) | Retrieval boundaries, authorization, provenance, context assembly, and grounding | Baseline implemented experimentally |
| [Workflow Orchestration](workflow-orchestration.md) | Bounded sequential/conditional composition and dataflow | Implemented experimentally |

## How to use a design note

1. Read the problem and constraints.
2. Compare alternatives and trade-offs.
3. Check the proposed contracts and threat boundaries.
4. Inspect the implementation and tests, if marked implemented.
5. Record any final architectural decision in `docs/decisions/`.
6. Update the note when implementation evidence changes the design.

Do not treat an experimental design note as a stable API contract.
