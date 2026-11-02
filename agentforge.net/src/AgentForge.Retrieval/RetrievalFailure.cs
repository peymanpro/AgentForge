namespace AgentForge.Retrieval;

/// <summary>
/// Represents a sanitized retrieval failure.
/// </summary>
public sealed record RetrievalFailure(string Code, string Message);
