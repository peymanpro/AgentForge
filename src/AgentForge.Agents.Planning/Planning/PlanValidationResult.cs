namespace AgentForge.Agents.Planning.Planning;

/// <summary>
/// Represents the outcome of validating a plan before any step is executed.
/// </summary>
public sealed class PlanValidationResult
{
    private PlanValidationResult(IReadOnlyList<PlanValidationIssue> issues)
    {
        Issues = issues;
    }

    /// <summary>Gets a value indicating whether the plan passed validation.</summary>
    public bool IsValid => Issues.Count == 0;

    /// <summary>Gets the immutable validation issues.</summary>
    public IReadOnlyList<PlanValidationIssue> Issues { get; }

    /// <summary>Creates a successful validation result.</summary>
    public static PlanValidationResult Valid() =>
        new(Array.Empty<PlanValidationIssue>());

    /// <summary>Creates a failed validation result.</summary>
    public static PlanValidationResult Invalid(IEnumerable<PlanValidationIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);
        var materialized = issues.ToArray();
        if (materialized.Length == 0)
        {
            throw new ArgumentException("At least one validation issue is required.", nameof(issues));
        }

        return new PlanValidationResult(Array.AsReadOnly(materialized));
    }
}
