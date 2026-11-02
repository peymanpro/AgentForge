# Adding a New Agent to AgentForge

This guide defines the engineering and learning checklist for a new agent family.

## 1. Classify the proposal

Choose the most accurate category:
- Complete agent architecture.
- Agent algorithm or decision strategy.
- LLM agent pattern.
- Multi-agent coordination architecture.
- Capability, protocol, or workflow.

An algorithm may be reusable inside an agent without being a complete agent. A protocol or workflow is not automatically an agent.

## 2. Write the design before the project

Record:
- Problem and target scenario.
- State, environment, goals, actions, observations, and assumptions.
- Algorithm or control loop.
- Inputs, outputs, and state transitions.
- Required and optional model/tool/memory/retrieval capabilities.
- Termination, cancellation, and resource budgets.
- Failure, partial progress, and recovery behavior.
- Security, privacy, and side-effect analysis.
- Relationship to existing AgentForge modules.

## 3. Prove the algorithm deterministically

Before using a live model, express the decision logic with a deterministic environment or fake model/tool. The tests should show why the algorithm makes a particular decision and when it stops.

Where relevant, compare it to a baseline:
- A* versus uninformed search.
- Utility-based versus goal-only action selection.
- ReAct versus plan-and-execute.
- Reflection versus a simple retry.
- One agent versus a coordinated team.

## 4. Choose a project boundary

Create a new `src/AgentForge.*` project only when at least one is true:
- It has an independent public contract and user scenario.
- It needs distinct dependencies or runtime capabilities.
- It is reusable without the other agent modules.
- Keeping it separate materially improves comprehension or test isolation.

Do not create empty projects or generic strategy registries that hide the actual algorithm.

## 5. Implement and teach

Each module should include:
- Public contracts and options.
- The main algorithm implementation.
- Explicit stopping and failure semantics.
- Unit and scenario tests.
- A deterministic runnable sample.
- A README describing purpose, algorithm, lifecycle, configuration, limits, safety, and known limitations.
- A design note and a learning exercise.

## 6. Integrate without coupling

The new agent should compose through public contracts. It must not require unrelated agents to know its internal state or decision logic. Cross-cutting concerns belong in reusable runtime or capability modules only after their common semantics are demonstrated.

## 7. Update the catalog and roadmap

Mark the entry Implemented (experimental) only after tests, sample, docs, and solution integration pass. Record known limitations. Keep the API status honest.
