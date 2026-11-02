namespace AgentForge.Retrieval;

/// <summary>
/// Describes the outcome of one retrieval operation.
/// </summary>
public enum RetrievalStatus
{
    /// <summary>One or more matching chunks were returned.</summary>
    Success,

    /// <summary>The authorized search completed but found no matching chunks.</summary>
    NoResults,

    /// <summary>The application policy denied the retrieval operation.</summary>
    Denied,

    /// <summary>The retrieval operation failed.</summary>
    Failed
}
