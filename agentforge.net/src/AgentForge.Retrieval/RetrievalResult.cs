namespace AgentForge.Retrieval;

/// <summary>
/// Represents the normalized result of a retrieval operation.
/// </summary>
public sealed class RetrievalResult
{
    private RetrievalResult(
        RetrievalStatus status,
        IReadOnlyList<RetrievedChunk> items,
        RetrievalFailure? failure)
    {
        if (!Enum.IsDefined(status))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        if (status == RetrievalStatus.Success && items.Count == 0)
        {
            throw new ArgumentException("A successful retrieval must contain at least one item.", nameof(items));
        }

        if (status != RetrievalStatus.Success && items.Count > 0)
        {
            throw new ArgumentException("A non-successful retrieval cannot contain items.", nameof(items));
        }

        if (status is RetrievalStatus.Denied or RetrievalStatus.Failed && failure is null)
        {
            throw new ArgumentNullException(nameof(failure), "Denied and failed retrievals must explain the outcome.");
        }

        Status = status;
        Items = Array.AsReadOnly(items.ToArray());
        Failure = failure;
    }

    /// <summary>Gets the retrieval status.</summary>
    public RetrievalStatus Status { get; }

    /// <summary>Gets the retrieved items, ordered by the selected retrieval strategy.</summary>
    public IReadOnlyList<RetrievedChunk> Items { get; }

    /// <summary>Gets the sanitized failure, when applicable.</summary>
    public RetrievalFailure? Failure { get; }

    /// <summary>Creates a successful result containing one or more items.</summary>
    public static RetrievalResult Success(IEnumerable<RetrievedChunk> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        var materialized = items.ToArray();
        if (materialized.Length == 0)
        {
            return NoResults();
        }

        return new RetrievalResult(RetrievalStatus.Success, materialized, null);
    }

    /// <summary>Creates a no-results result.</summary>
    public static RetrievalResult NoResults() =>
        new(RetrievalStatus.NoResults, Array.Empty<RetrievedChunk>(), null);

    /// <summary>Creates a denied result.</summary>
    public static RetrievalResult Denied(string message = "The application policy denied this retrieval operation.") =>
        new(RetrievalStatus.Denied, Array.Empty<RetrievedChunk>(), new RetrievalFailure("retrieval.denied", message));

    /// <summary>Creates a failed result.</summary>
    public static RetrievalResult Failed(string code, string message) =>
        new(RetrievalStatus.Failed, Array.Empty<RetrievedChunk>(), new RetrievalFailure(code, message));
}
