namespace AgentForge.Agents.Planning.Planning;

/// <summary>
/// Describes one structural or semantic plan validation issue.
/// </summary>
public sealed record PlanValidationIssue(string Code, string Path, string Message);
