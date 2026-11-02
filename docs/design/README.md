# Design Notes

This directory contains focused design proposals for AgentForge. A design note explores an engineering decision before it becomes a stable public contract.

Design notes are not automatically accepted specifications or API guarantees. Their status must be checked before implementation relies on them.

## Current proposals

| Document | Topic | Status |
|---|---|---|
| [Model Provider Contract](model-provider-contract.md) | Baseline model interaction and optional provider capabilities | Proposal; validate with prototypes |

## Design note lifecycle

1. **Problem and constraints:** describe the engineering problem and goals.
2. **Options:** compare viable designs and explain trade-offs.
3. **Recommendation:** state a preferred direction without presenting it as final prematurely.
4. **Prototype:** test the design with a narrow working example where appropriate.
5. **Review:** consider impact across .NET, TypeScript/NestJS, Python, and future implementations.
6. **Decision:** accept, revise, or reject the proposal. If a lasting architectural decision is made, record it in `docs/decisions/`.

A design note should be updated or superseded when implementation evidence invalidates its assumptions.
