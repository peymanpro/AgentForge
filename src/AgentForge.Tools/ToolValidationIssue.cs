namespace AgentForge.Tools;

/// <summary>
/// Describes one tool argument validation issue.
/// </summary>
/// <param name="Path">The JSON path of the invalid value, or an empty string for a root-level issue.</param>
/// <param name="Message">A safe diagnostic description of the validation issue.</param>
public sealed record ToolValidationIssue(string Path, string Message);
