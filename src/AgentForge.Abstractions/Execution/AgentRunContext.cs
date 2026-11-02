namespace AgentForge.Abstractions.Execution;

/// <summary>
/// Identifies one agent run and optionally links it to a parent run.
/// </summary>
public sealed record AgentRunContext
{
    private AgentRunContext(Guid runId, Guid? parentRunId, DateTimeOffset createdAt)
    {
        if (runId == Guid.Empty)
        {
            throw new ArgumentException("A run identifier cannot be empty.", nameof(runId));
        }

        if (parentRunId == Guid.Empty)
        {
            throw new ArgumentException("A parent run identifier cannot be empty.", nameof(parentRunId));
        }

        RunId = runId;
        ParentRunId = parentRunId;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Gets the unique identifier for this run.
    /// </summary>
    public Guid RunId { get; }

    /// <summary>
    /// Gets the parent run identifier, when this run is nested.
    /// </summary>
    public Guid? ParentRunId { get; }

    /// <summary>
    /// Gets the time at which this context was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; }

    /// <summary>
    /// Creates a new run context.
    /// </summary>
    /// <param name="parentRunId">The parent run identifier, if this is a nested run.</param>
    /// <param name="createdAt">An optional creation time, primarily useful for deterministic tests.</param>
    /// <returns>A new context with a generated run identifier.</returns>
    public static AgentRunContext Create(
        Guid? parentRunId = null,
        DateTimeOffset? createdAt = null)
    {
        return new AgentRunContext(Guid.NewGuid(), parentRunId, createdAt ?? DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Creates a child context linked to this run.
    /// </summary>
    /// <param name="createdAt">An optional creation time, primarily useful for deterministic tests.</param>
    /// <returns>A new context whose parent identifier is this run's identifier.</returns>
    public AgentRunContext CreateChild(DateTimeOffset? createdAt = null)
    {
        return Create(RunId, createdAt);
    }
}
