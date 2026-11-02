# Concepts and Terminology

Agent engineering uses overlapping terminology. AgentForge uses the distinctions below to keep architecture, documentation, and package boundaries precise.

## Agent

An agent owns a defined decision-making or execution loop toward a task or goal. It may use a model, tools, memory, or other capabilities, but none of those alone makes a component an agent.

## Agent pattern

A pattern describes a reusable approach to agent control or reasoning.

Examples include:
- **ReAct:** interleaves reasoning steps and actions/observations.
- **Plan-and-execute:** separates plan creation from execution of planned steps.
- **Reflection:** evaluates prior output or progress and uses feedback to revise subsequent work.

A pattern may be implemented by an agent. The pattern itself is not necessarily a standalone agent implementation.

## Capability

A capability is a reusable function available to an agent or workflow.

Examples:
- Retrieval.
- Memory.
- Tool execution.
- Structured output handling.
- Human approval or intervention.
- Evaluation.

Capabilities can be composed without creating an autonomous agent.

## Tool

A tool is an operation an agent can request through a defined contract. Tool execution may have side effects and must be subject to application-level trust and authorization controls.

## Protocol and integration

A protocol defines communication or interoperability rules. An integration connects AgentForge to an external implementation of a protocol or service.

For example, MCP is a protocol for connecting AI applications to external tools and context. It is not itself an agent. An MCP client or server integration may be useful to an agent.

## Workflow

A workflow is an explicit sequence or graph of steps. Some workflows use agents; others are deterministic pipelines. A workflow should not be labeled autonomous merely because one of its steps calls a model.

## Orchestration

Orchestration coordinates agents, tools, and workflow steps. It may handle routing, delegation, state transitions, retries, timeouts, and error propagation.

## Architecture

An architecture describes how components are organized or coordinated. Examples include single-agent, supervisor-based, hierarchical, and multi-agent designs.

## Domain application

A domain application applies agent capabilities to a specific problem area, such as research assistance, coding, customer support, or data analysis. A domain label does not necessarily imply a distinct agent algorithm.

## Why these distinctions matter

A clear taxonomy helps developers understand what they are installing and composing. It also avoids duplicate implementations, misleading names, and package boundaries based only on fashionable terminology.

When proposing a new component, describe the problem and classify it before deciding whether it should be an agent, pattern, capability, integration, workflow, or architecture.
