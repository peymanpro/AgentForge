namespace AgentForge.Abstractions.Execution;

/// <summary>
/// Describes a normalized failure without requiring callers to inspect provider-specific exceptions.
/// </summary>
/// <param name="Category">The provider-neutral failure category.</param>
/// <param name="Code">A stable, machine-readable failure code.</param>
/// <param name="Message">A concise diagnostic message that must not contain secrets or sensitive payloads.</param>
/// <param name="IsTransient">Indicates whether retry may be appropriate under an explicit policy.</param>
public sealed record AgentFailure(
    AgentFailureCategory Category,
    string Code,
    string Message,
    bool IsTransient = false)
{
    /// <summary>
    /// Validates the failure code and safe diagnostic message.
    /// </summary>
    /// <exception cref="ArgumentException">The code or message is empty or whitespace.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Code))
        {
            throw new ArgumentException("A failure code is required.", nameof(Code));
        }

        if (string.IsNullOrWhiteSpace(Message))
        {
            throw new ArgumentException("A diagnostic message is required.", nameof(Message));
        }
    }
}
