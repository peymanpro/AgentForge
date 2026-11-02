# Classical Agent Modules

This design note describes the module boundaries for the classical agent architectures in AgentForge. Each architecture is an independently usable module; the word "classical" is a taxonomy, not a single runtime package.

## 1. Product boundary

Classical agents are deterministic or policy-driven agent implementations that do not inherently require language models. They share the execution contract but own separate decision logic and state semantics.

An agent proposes an action. The application/environment adapter remains responsible for applying external effects and enforcing application permissions.

## 2. Initial module split

| Module | Input | Core decision | State ownership |
|---|---|---|---|
| `AgentForge.Agents.SimpleReflex` | Current percept | First matching ordered condition-action rule | No retained state |
| `AgentForge.Agents.ModelBasedReflex` | Previous state + current percept | Update state, then evaluate ordered rules | Explicitly passed in and returned |

Each module contains its own Domain and Application projects plus a public API/composition-root project. Neither requires an Infrastructure project because neither needs provider, database, tool, or transport access.

## 3. Simple Reflex

The Domain layer defines condition-action rules and the result of a decision. The Application layer evaluates the rules in declared order. The public builder exposes:

- `Create<TPercept,TAction>()`
- `When(name, condition, actionFactory)` or `When(name, condition, action)`
- `WithTimeProvider(...)`
- `Build()`

The first matching rule wins. If no rule matches, the agent returns a successful no-action outcome.

## 4. Model-Based Reflex

The Domain layer defines an input containing the latest percept and previous state, plus the updated-state output. Application calls the state updater, then evaluates ordered rules against the newly computed state.

The public builder exposes:

- `Create<TPercept,TState,TAction>()`
- `WithStateUpdater(...)`
- `When(name, condition, actionFactory)` or `When(name, condition, action)`
- `WithTimeProvider(...)`
- `Build()`

The agent instance does not retain mutable per-user state. The host chooses whether to store, persist, reconstruct, or discard the returned state.

## 5. Shared engineering rules

- No model/provider dependency for deterministic decision modules.
- No concrete dependency between the Simple Reflex and Model-Based Reflex modules.
- No external action execution inside the decision use case.
- Rule order, no-action behavior, cancellation, invalid options, and failures are explicit.
- Builder configuration is validated before a run starts.
- Each module has independent tests and a runnable example.

## 6. Next classical modules

The next independent modules are Goal-Based, Utility-Based, Rule-Based/Production-System, Learning Agent, Reactive/Subsumption, Deliberative, Hybrid, BDI, Finite-State, Behavior Tree, and Logic-Based. Search, planning, game-playing, optimization, and reinforcement-learning algorithms remain reusable algorithm packages; complete agents compose those strategies when their control architecture requires them.

See [Agent Module Architecture](../agent-modules.md) for the common module template and [Roadmap](../../ROADMAP.md) for the implementation sequence.
