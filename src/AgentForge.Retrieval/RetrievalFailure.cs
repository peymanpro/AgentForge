namespace AgentForge.Retrieval;

/// <summary>
/// Represents a sanitized retrieval failure.
/// </summary>
/// <param name="Code">A stable machine-readable failure code.</param>
/// <param name="Message">A safe diagnostic message without sensitive provider details.</param>
public sealed record RetrievalFailure(string Code, string Message);
