# Retrieval and RAG Composition — Design Note

**Status:** Proposal; implementation contract not yet approved  
**Initial target:** .NET / C#  
**Cross-language scope:** Shared semantics; idiomatic implementations

## 1. Purpose

AgentForge's MVP needs a basic way to retrieve relevant information and supply it to model-backed tasks.

Retrieval-augmented generation (RAG) is a composition of retrieval and generation. Retrieval is a capability or pipeline, not automatically an autonomous agent. It should be possible to use retrieval from a ReAct agent, Planning step, workflow, or ordinary application code without forcing those components to depend on a vector database or one embedding provider.

The design must keep retrieval, indexing, ranking, context assembly, and generation responsibilities explicit.

## 2. Design goals

The initial retrieval layer should:

1. Define a provider-neutral retrieval contract.
2. Preserve source identity and provenance with retrieved content.
3. Support deterministic fake retrievers for tests.
4. Allow different retrieval implementations without changing agent algorithms.
5. Apply application authorization and metadata filters before content is exposed to a model.
6. Keep embeddings, vector stores, keyword search, and reranking as replaceable strategies or adapters.
7. Bound the number and size of retrieved items.
8. Preserve enough provenance for an application to cite or inspect sources where appropriate.
9. Treat retrieved content as untrusted data, not system instructions.
10. Remain idiomatic in .NET and composable with the rest of the solution.

## 3. Conceptual responsibilities

Keep these concerns separate:

- **Document ingestion:** accepts or imports source material and prepares it for indexing.
- **Chunking:** splits source material into retrieval units while preserving source metadata.
- **Embedding:** converts supported content into vectors through an optional embedding model.
- **Indexing/storage:** stores searchable documents, vectors, or terms.
- **Retrieval:** selects candidate items for a query.
- **Filtering:** applies tenant, authorization, metadata, and application constraints.
- **Ranking:** orders candidates using a defined scoring or reranking strategy.
- **Context assembly:** selects and formats a bounded set of items for a model call.
- **Generation:** uses the model to produce a response based on the task and supplied context.

The MVP does not need to implement every ingestion, indexing, and storage backend. It needs clear contracts and one simple reference/test path.

## 4. Retrieval contract

A retrieval request should contain:
- A query or search input.
- An optional maximum result count.
- Explicit filter criteria.
- Optional scope or namespace.
- A cancellation token or equivalent mechanism.

A retrieval result should preserve:
- A stable item or chunk identifier.
- The retrieved content.
- Source/document identity.
- Provenance metadata such as title, URI, page, section, or chunk index when available.
- Optional relevance score and score semantics.
- Safe metadata required for display or application policy.

A possible .NET-shaped sketch is:

```csharp
public interface IRetriever
{
    Task<RetrievalResult> RetrieveAsync(
        RetrievalRequest request,
        CancellationToken cancellationToken = default);
}
```

This is illustrative. Exact names, types, filters, paging, streaming, and package placement are not approved.

The contract should not require every retriever to return a numeric relevance score. If a score is present, its meaning and direction must be documented. Scores from different retrieval implementations must not be assumed comparable.

## 5. Content and provenance

A retrieved item should separate content from provenance.

Candidate fields include:
- ItemId
- DocumentId
- Content
- SourceUri or application-defined source reference
- Title
- ChunkIndex
- PageOrSection
- ContentType
- Metadata
- Optional Score and ScoreSemantics

The source URI may be absent for internal, ephemeral, or access-controlled sources. The result should not fabricate provenance.

A retriever should preserve the identity of the source document and chunk when possible. Context assembly must not discard provenance if the application needs citations, source inspection, or auditability.

## 6. Authorization and data isolation

Retrieval is a data-access boundary.

The application/runtime must be able to supply the principal, tenant, namespace, or authorization context required by the data source.

Authorization filters must be enforced by a trusted retrieval adapter or data-access layer. They must not rely solely on model-generated filters or post-processing after unauthorized content has already been exposed to the agent.

The design should support:
- Tenant and namespace isolation.
- Document-level access controls where applicable.
- Metadata constraints.
- Source allowlists or scopes.
- Safe handling of deleted or revoked documents.

If an adapter cannot enforce a required security constraint, it must report that limitation rather than silently return broader results.

Retrieved text may contain prompt injection, instructions, or malicious content. It must be treated as untrusted evidence/data. It must not override system instructions, tool permissions, or application policies.

## 7. Retrieval strategies

The base retrieval contract should not prescribe a single search algorithm.

Possible implementations include:
- Keyword or lexical search.
- Vector similarity search.
- Hybrid retrieval.
- Metadata-filtered lookup.
- Application-defined search.
- Reranking as a separate optional stage.

The first implementation may use a deterministic in-memory retriever or a small reference adapter. It should not require a production vector database.

A reranker should be an optional collaborator with an explicit input/output contract. It should not be hidden inside an IRetriever unless the adapter clearly documents that ranking is integrated.

## 8. Basic RAG composition

The MVP RAG pipeline should be an explicit composition:

```text
                  User question
                       |
                 Query preparation
                       |
                    Retriever
                       |
              Authorization/filtering
                       |
                Candidate results
                       |
             Optional ranking stage
                       |
                Context assembly
                       |
               Bounded model request
                       |
                 Generated answer
                       |
              Answer + source metadata
```

The initial pipeline should:
1. Accept the task/question and explicit retrieval options.
2. Retrieve a bounded set of items.
3. Apply required authorization and metadata constraints.
4. Optionally rerank when a configured strategy exists.
5. Assemble a model context within a configured size budget.
6. Invoke a supplied model adapter.
7. Return the answer with the source references used for context.

The pipeline should not silently retrieve an unbounded number of chunks or include the entire index.

## 9. Context assembly

Context assembly should be a separate responsibility.

It should consider:
- Maximum item count.
- Maximum content size or token budget where reliable tokenization is available.
- Duplicate or near-duplicate results.
- Source diversity if required by the use case.
- Provenance preservation.
- Ordering and truncation policy.

The context builder should not assume token counts are exact unless it uses the target model's tokenizer. A conservative character/byte budget may be useful as a generic fallback, but it is not equivalent to a token budget.

The assembler should report which items were included, omitted, or truncated so the application can inspect the evidence set.

## 10. Generation and evidence handling

The generation step must receive retrieved content as explicitly marked reference context, not merged into privileged system instructions.

The answer result should preserve the set of retrieved source references that were made available to the model. This does not prove that every source supports every claim.

If citation or claim-level evidence alignment is implemented later, it should be a distinct capability with its own verification strategy. The basic RAG pipeline must not claim that merely attaching source links guarantees factual grounding.

The application should be able to inspect retrieved sources independently of the natural-language answer.

## 11. Failure and empty-result semantics

The pipeline should distinguish:
- No results found.
- All candidates filtered by authorization or metadata constraints.
- Retrieval provider failure.
- Invalid query or filter configuration.
- Ranking failure.
- Context budget exhausted or no content fit.
- Model generation failure.
- Cancellation or timeout.

No results should not be silently converted into a fabricated answer. The configured policy may ask the model to respond using only the question and state that no supporting material was retrieved, or the application may return an explicit no-results result.

An authorization-filtered empty result must not reveal whether inaccessible documents exist unless the application explicitly allows that disclosure.

## 12. State and persistence

The retrieval capability should not own long-term user memory or agent conversation history by default.

Index storage, document ingestion, synchronization, deletion, and retention are responsibilities of the selected data source or a separate indexing subsystem.

The MVP RAG run may keep its selected evidence set in memory for the duration of one execution. Durable evidence snapshots and reproducibility across index changes are future capabilities.

## 13. Testing strategy

Use a deterministic in-memory retriever and a fake model.

Minimum tests:

1. A query returns the expected set of chunks.
2. Result identity and provenance are preserved.
3. A maximum result limit is enforced.
4. Tenant or authorization scope is applied before content is exposed.
5. Unauthorized content is never sent to the model.
6. A metadata filter behaves consistently.
7. Missing relevance scores are handled.
8. Score ordering is applied only when score semantics are documented.
9. Duplicate results are handled by the configured context policy.
10. Context size limits are enforced.
11. Truncated or omitted items remain identifiable in diagnostics.
12. Empty retrieval does not create unsupported evidence.
13. Retrieval failure is distinguishable from an empty result.
14. Cancellation and timeout are propagated.
15. Retrieved prompt-injection content cannot change tool permissions or system policy.
16. The answer result preserves the source references supplied to the model.
17. Default logs do not expose retrieved content or sensitive metadata.

Tests must not depend on a live embedding service, paid model API, or external vector database for the basic retrieval contract.

## 14. API sketch

A possible composition shape is:

```csharp
var rag = RagPipeline.Create()
    .WithRetriever(retriever)
    .WithContextAssembler(contextAssembler)
    .WithModel(chatModel)
    .WithMaxResults(6)
    .Build();

var result = await rag.RunAsync(
    "Summarize the relevant policy requirements.",
    retrievalContext,
    cancellationToken);
```

This is illustrative only. Names, result types, builder design, security context, and package boundaries are not approved.

The RAG pipeline should remain optional. Applications may call the retriever directly or provide retrieval as a capability to an agent.

## 15. Alternatives considered

### A RAG Agent as a mandatory autonomous agent type

**Not selected as the default.** Basic retrieval-augmented generation is a pipeline/capability composition. Agentic retrieval may be introduced separately when an explicit decision loop or iterative retrieval behavior exists.

### A mandatory vector database and embedding provider

**Rejected.** This would make the core retrieval capability unnecessarily dependent on infrastructure and provider choices.

### Combine retrieval, ranking, context assembly, and generation in one opaque class

**Not selected.** It makes authorization, evaluation, source provenance, and replacement of individual strategies difficult.

### Small contracts with an explicit basic RAG composition

**Recommended for prototyping.** It provides practical functionality while preserving independent testing and integration boundaries.

## 16. Open design questions

1. Should the base contract retrieve one query at a time or support a batch of query variants?
2. How should authorization context be represented without making it a vendor-specific or application-wide service locator?
3. Should the shared retrieval result standardize chunk metadata or use an extensible provenance envelope?
4. Should the first context budget use a character limit, a caller-provided token counter, or both?
5. What minimal evidence relationship should be returned by basic RAG, and which behaviors require a separate citation/grounding capability?
6. Should the first reference retriever be an in-memory implementation or only a deterministic test fake?
7. Which retrieval behaviors belong in the shared specification versus implementation-specific adapters?

## 17. Decision status

This note recommends **a provider-neutral retrieval contract, explicit provenance, authorization-aware filtering, a bounded context assembler, and a basic RAG composition**.

It is a proposal, not an approved API or implementation claim. Validate it with deterministic in-memory retrieval and a fake model before promoting any contract into the shared specification.
