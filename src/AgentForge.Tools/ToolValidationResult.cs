namespace AgentForge.Tools;

/// <summary>
/// Represents the result of validating arguments for a tool.
/// </summary>
public sealed class ToolValidationResult
{
    private ToolValidationResult(bool isValid, IReadOnlyList<ToolValidationIssue> issues)
    {
        IsValid = isValid;
        Issues = issues;
    }

    /// <summary>
    /// Gets a value indicating whether the arguments passed validation.
    /// </summary>
    public bool IsValid { get; }

    /// <summary>
    /// Gets the validation issues. A valid result has no issues.
    /// </summary>
    public IReadOnlyList<ToolValidationIssue> Issues { get; }

    /// <summary>
    /// Creates a successful validation result.
    /// </summary>
    public static ToolValidationResult Valid()
    {
        return new ToolValidationResult(true, Array.Empty<ToolValidationIssue>());
    }

    /// <summary>
    /// Creates a failed validation result.
    /// </summary>
    /// <param name="issues">One or more validation issues.</param>
    /// <returns>A failed validation result.</returns>
    public static ToolValidationResult Invalid(IEnumerable<ToolValidationIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(issues);

        var materialized = issues.ToArray();
        if (materialized.Length == 0)
        {
            throw new ArgumentException("At least one validation issue is required.", nameof(issues));
        }

        if (materialized.Any(static issue => issue is null))
        {
            throw new ArgumentException("Validation issues cannot contain null values.", nameof(issues));
        }

        return new ToolValidationResult(false, Array.AsReadOnly(materialized));
    }
}
