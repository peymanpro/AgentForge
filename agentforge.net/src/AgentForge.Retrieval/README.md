# AgentForge.Retrieval

Provider-neutral retrieval contracts and authorization-aware retrieval composition for AgentForge .NET.

## Data and result contracts

DocumentChunk preserves chunk ID, parent document ID, tenant ID, content, title, source URI, chunk index, and metadata. RetrievedChunk carries the source chunk and a normalized score with explicit score semantics.

RetrievalRequest carries the query, result limit, and optional request filters. RetrievalScope carries the mandatory tenant, optional document allowlist, and mandatory metadata filters.

## Authorization boundary

AuthorizedRetriever asks the application-supplied IRetrievalAuthorizationPolicy for a mandatory scope before invoking an IRetriever. It fails closed on policy errors, missing scopes, and tenant mismatches.

Every data-source adapter must enforce the supplied RetrievalScope before returning content. The wrapper cannot make an untrusted adapter secure by itself.

## Reference implementation

InMemoryKeywordRetriever is a deterministic lexical baseline. It ranks documents by unique query-term overlap and enforces tenant, document, and metadata constraints.

It is not vector search, embedding similarity, hybrid retrieval, or a production indexing system.
