namespace AgentForge.Agents.UtilityBased.Domain.Policies;

/// <summary>
/// Defines how a Utility-Based agent resolves equal maximum utility scores.
/// </summary>
public enum UtilityTieBreakPolicy
{
    /// <summary>Keep the earliest candidate among those with the maximum score.</summary>
    FirstCandidate,

    /// <summary>Keep the latest candidate among those with the maximum score.</summary>
    LastCandidate,

    /// <summary>Return a no-action decision when multiple candidates share the maximum score.</summary>
    RejectAmbiguous
}