# Agent Engineering Capability Map

Agents become reliable systems only when their surrounding capabilities are engineered deliberately. This map records the major areas that AgentForge should teach and eventually support.

## 1. Model integration

- Provider-neutral message, request, response, usage, and finish-reason contracts.
- Provider adapters and model selection/routing.
- Tool/function calling.
- Structured/constrained output and schema validation.
- Streaming and incremental events.
- Embeddings, reranking, token counting, and multimodal input/output.
- Rate-limit awareness, retries, provider errors, timeout, and cancellation.

## 2. Prompt and instruction engineering

- System/developer/user/tool message boundaries.
- Prompt templates, variables, versioning, and testing.
- Few-shot examples and demonstrations.
- Output constraints and parsing.
- Prompt composition and prompt injection boundaries.
- Prompt evaluation, change tracking, and regression.

## 3. Execution runtime

- Agent run identity and correlation.
- Input, output, result, failure, and partial-result contracts.
- Transient state and explicit transitions.
- Cancellation, timeouts, model/tool/step budgets, and aggregate budgets.
- Lifecycle events and nested execution.
- Dependency injection, factories, and runtime composition.

## 4. Tool engineering

- Tool identity, description, typed schema, validation, and registration.
- Discovery and selection.
- Authentication, authorization, policy evaluation, and least privilege.
- Side-effect classification, idempotency, retries, concurrency, and compensation.
- Sandboxing, isolation, secrets, and audit events.
- Human confirmation for high-impact operations.

## 5. State and memory

- Conversation history and working memory.
- Sliding-window and token-aware context management.
- Summarization and compaction.
- Episodic, semantic, entity, and procedural memory.
- Short-term versus long-term storage.
- Memory write/read policies, consolidation, relevance, decay, and forgetting.
- Tenant isolation, retention, deletion, privacy, and provenance.

## 6. Retrieval and knowledge

- Ingestion, document parsing, normalization, chunking, and metadata.
- Lexical retrieval (for example BM25).
- Dense/vector retrieval and embedding lifecycle.
- Hybrid retrieval and score fusion.
- Metadata, tenant, access-control, and temporal filters.
- Query rewriting, decomposition, multi-query, and HyDE-style transformation.
- Reranking, contextual compression, deduplication, and diversity.
- Graph retrieval and knowledge graphs.
- Evaluation of recall, precision, ranking, and retrieval latency.

## 7. RAG and grounded generation

- Retrieval/query planning and context assembly.
- Source IDs, provenance, citations, and traceability.
- Evidence-aware answer generation.
- Groundedness/faithfulness evaluation.
- Context-window and token-budget management.
- Corrective, adaptive, self-evaluating, and agentic RAG patterns.
- Handling contradictory, stale, untrusted, or insufficient evidence.

## 8. Reasoning, planning, and learning

- Goal/state/action/reward representations.
- Search spaces, heuristics, cost functions, and admissibility.
- Constraint solving and classical planning.
- Utility functions and multi-objective decisions.
- Bayesian reasoning and uncertainty.
- Bandits, MDPs, reinforcement learning, policy/value functions, exploration.
- LLM planning, reflection, tree/graph search, and replanning.
- Feedback loops and learning from outcomes.

## 9. Workflow and multi-agent coordination

- Sequential, conditional, parallel, loop, map-reduce, and graph execution.
- Typed state and explicit dataflow.
- Retries, timeouts, error routing, compensation, and termination.
- Checkpoints, persistence, resumption, pause, and human approval.
- Agent roles, messages, delegation, shared state, conflict resolution, negotiation, and consensus.
- Fair scheduling, global resource budgets, and coordination observability.

## 10. Evaluation and testing

- Unit, contract, integration, scenario, and regression tests.
- Deterministic model/tool fakes and recorded fixtures.
- Golden datasets and reproducible evaluation cases.
- Task completion, correctness, relevance, groundedness, and calibration.
- Tool selection/argument correctness.
- Safety, robustness, refusal, and prompt-injection resistance.
- Model-as-judge design and judge validation.
- Latency, throughput, token/cost metrics, and statistical comparison.

## 11. Security, privacy, and governance

- Threat models and trust boundaries.
- Untrusted model outputs, retrieved content, external tools, and messages.
- Prompt injection and indirect prompt injection.
- Authorization, role/attribute-based policies, tenant isolation.
- Secret management and data minimization.
- PII redaction, retention/deletion, consent, and audit.
- Human-in-the-loop approvals and escalation.
- Sandboxed execution and supply-chain security.

## 12. Observability and production operations

- Correlated structured events, spans, and traces.
- Model/tool/retrieval/workflow visibility.
- Redaction and configurable content capture.
- Latency, error, token, cost, and resource metrics.
- Caching, rate limits, concurrency, backpressure, and circuit breakers.
- Retry/idempotency and failure recovery.
- Deployment, scaling, queueing, health checks, and incident response.

## 13. Protocols and modalities

- Model-provider APIs and structured tool protocols.
- MCP client/server interoperability.
- Agent-to-agent communication.
- Browser and computer-use tool interfaces.
- Voice turn-taking, speech-to-text, text-to-speech, and real-time streaming.
- Image, audio, video, and multimodal context.
- Event-driven integrations and external systems.

## How to use this map

A capability becomes a source module only when its public boundary and dependency requirements are clear. A topic may first exist as a design note, reference sample, or learning exercise. Do not create an empty project merely to represent a concept.
