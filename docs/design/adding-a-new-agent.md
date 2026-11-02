# Adding a New Agent Module

This document defines the implementation process for adding a new independently usable agent type.

## 1. Classify the component

Decide whether the proposal is a complete agent architecture, an agent pattern, an algorithm, a capability, a protocol, a workflow, or a multi-agent topology.

A* is a search algorithm. MCP is a protocol. RAG is a capability/pipeline. A workflow is a composition mechanism. None should be called an autonomous agent merely because it is used by one.

## 2. Define the module boundary

A new agent type normally receives its own public module, for example:

- `AgentForge.Agents.GoalBased`
- `AgentForge.Agents.BDI`
- `AgentForge.Agents.Reflexion`
- `AgentForge.MultiAgent.Supervisor`

Record:
- Problem and target application scenario.
- Inputs, outputs, state, goals, percepts, and actions.
- Decision algorithm/control loop.
- Termination and acceptance conditions.
- Required ports and optional capabilities.
- Resource budgets, cancellation, failure, and partial progress.
- State ownership and persistence.
- Security, permissions, and side-effect analysis.
- Why the module is distinct from existing modules.

## 3. Apply Clean Architecture

**Domain** contains core rules and concepts with no infrastructure dependency.

**Application** implements the agent use case/control loop and defines ports for models, tools, memory, retrieval, or external services as needed.

**Infrastructure** contains adapters that implement those ports. Omit this layer when the architecture has no external I/O requirement.

**Public API/composition root** owns the fluent builder, validates configuration, composes the selected adapters, and returns an AgentHandle.

Do not reference another agent module's concrete implementation. Compose through the shared agent contract.

## 4. Design the fluent API

The builder should express the minimum required architecture-specific configuration. It should:
- Expose required strategies explicitly.
- Use typed method signatures.
- Provide a small common vocabulary for metadata, limits, and optional capabilities.
- Validate required options in Build().
- Freeze configuration after Build().
- Avoid hidden remote calls or writes during Build().
- Return a handle that supports RunAsync(input, cancellationToken).

Do not add generic methods for features the agent does not support. A rule-based agent should not expose WithModel; a provider-backed ReAct agent should.

## 5. Implement deterministic behavior first

Where possible, prove the algorithm with deterministic fakes and fixtures before adding live model/provider adapters. Tests should demonstrate why the agent selects an action and how it stops.

Relevant comparisons may include:
- Goal satisfaction versus utility maximization.
- ReAct versus plan-and-execute.
- Reflection versus retry.
- Supervisor delegation versus a fixed workflow.
- A* versus uninformed search.

## 6. Test the module

Each module needs:
- Domain unit tests.
- Application use-case tests.
- Builder validation tests.
- Architecture/dependency tests.
- Integration tests for actual external adapters.
- Success, no-op, invalid input, failure, cancellation, and budget cases.
- State/concurrency tests where relevant.

## 7. Document and integrate

The module README should describe purpose, supported architecture, builder API, dependencies, lifecycle, limits, failure semantics, security, and known limitations.

Add:
- The project(s) to the solution.
- The module to the agent catalog with an honest status.
- A roadmap entry.
- A runnable sample.
- Design notes for non-obvious choices.

Only mark the module Implemented after all gates and CI pass.
