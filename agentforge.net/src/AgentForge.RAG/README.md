# AgentForge.RAG

A basic provider-neutral retrieval-augmented generation pipeline.

## Pipeline

1. Receive a question and application retrieval identity.
2. Call IAuthorizedRetriever so application policy establishes the tenant and document scope.
3. Assemble returned chunks into provenance-preserving JSON reference blocks.
4. Enforce a hard context character budget and record omitted chunk IDs.
5. Send the question and reference block to IChatModel.
6. Return the answer together with retrieved, included, and omitted source records and provider usage when available.

## Trust boundary

Retrieved excerpts are untrusted data, not instructions. The built-in evidence policy keeps them in user context rather than promoting them to privileged system instructions.

The pipeline does not execute model-proposed tool calls. It does not claim claim-level citation verification or a guarantee that generated text is factually supported.

## Extension points and limitations

Applications provide the authorized retriever, model, and optionally a context assembler. The included JSON context assembler is simple and deterministic.

This initial module does not include document ingestion, chunking, embeddings, vector database adapters, hybrid search, reranking, durable source snapshots, or citation verification.
