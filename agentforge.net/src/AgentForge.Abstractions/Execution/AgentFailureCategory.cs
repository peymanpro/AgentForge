namespace AgentForge.Abstractions.Execution;

/// <summary>
/// Provides a provider-neutral category for an agent execution failure.
/// </summary>
public enum AgentFailureCategory
{
    /// <summary>Input or run configuration was invalid.</summary>
    InvalidInput,

    /// <summary>A required model or runtime capability is unavailable.</summary>
    UnsupportedCapability,

    /// <summary>Authentication or authorization failed.</summary>
    AuthorizationFailed,

    /// <summary>An application policy denied the requested operation.</summary>
    PolicyDenied,

    /// <summary>A model provider operation failed.</summary>
    ModelProviderFailure,

    /// <summary>A tool operation failed.</summary>
    ToolFailure,

    /// <summary>A retrieval operation failed.</summary>
    RetrievalFailure,

    /// <summary>A generated plan, tool input, or other structured value failed validation.</summary>
    ValidationFailure,

    /// <summary>An evaluation operation failed.</summary>
    EvaluationFailure,

    /// <summary>An external side effect may have started but its completion is unknown.</summary>
    UnknownExternalOutcome,

    /// <summary>An unexpected internal failure occurred.</summary>
    InternalFailure
}
