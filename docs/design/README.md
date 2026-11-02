# Design Notes

This directory contains focused design proposals for AgentForge. A design note explores an engineering decision before it becomes a stable public contract.

Design notes are not automatically accepted specifications or API guarantees. Check each document's status before relying on it in an implementation.

## Current proposals

| Document | Topic | Status |
|---|---|---|
| [Model Provider Contract](model-provider-contract.md) | Baseline model interaction and optional provider capabilities | Proposal; validate with prototypes |
| [Tool Execution Contract](tool-execution-contract.md) | Tool declarations, validation, authorization, bounded execution, and results | Proposal; validate with prototypes |
| [ReAct Agent Execution](react-agent-execution.md) | Model decision, action, observation, budgets, and termination | Proposal; validate with prototypes |
| [Planning Agent Execution](planning-agent-execution.md) | Structured plan generation, validation, and bounded step execution | Proposal; validate with prototypes |
| [Reflection Agent Execution](reflection-agent-execution.md) | Evaluation, feedback, iterative refinement, and acceptance | Proposal; validate with prototypes |
| [Cross-Language Conformance](cross-language-conformance.md) | Shared behavioral scenarios and independent language-native test harnesses | Proposal; validate with prototypes |

## Design note lifecycle

1. **Problem and constraints:** describe the engineering problem and goals.
2. **Options:** compare viable designs and explain trade-offs.
3. **Recommendation:** state a preferred direction without presenting it as final prematurely.
4. **Prototype:** test the design with a narrow working example where appropriate.
5. **Review:** consider impact across .NET, TypeScript/NestJS, Python, and future implementations.
6. **Decision:** accept, revise, or reject the proposal. If a lasting architectural decision is made, record it in `docs/decisions/` and update the shared specification when needed.

A design note should be updated or superseded when implementation evidence invalidates its assumptions.
