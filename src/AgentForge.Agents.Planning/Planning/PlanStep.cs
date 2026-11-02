namespace AgentForge.Agents.Planning.Planning;

/// <summary>
/// Represents one explicitly typed step in a sequential execution plan.
/// </summary>
public sealed class PlanStep
{
    /// <summary>
    /// Creates a plan step candidate. Semantic validation occurs before execution.
    /// </summary>
    /// <param name="id">A stable identifier unique within the plan.</param>
    /// <param name="stepType">The registered handler type that is permitted to execute this step.</param>
    /// <param name="objective">A concise, untrusted objective for the selected handler.</param>
    /// <param name="inputReferences">Identifiers of prior step outputs explicitly made available to this step.</param>
    /// <param name="expectedOutcome">An optional description of the expected outcome.</param>
    public PlanStep(
        string id,
        string stepType,
        string objective,
        IEnumerable<string>? inputReferences = null,
        string? expectedOutcome = null)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(stepType);
        ArgumentNullException.ThrowIfNull(objective);

        var references = inputReferences?.ToArray() ?? Array.Empty<string>();
        if (references.Any(static reference => reference is null))
        {
            throw new ArgumentException("Input references cannot contain null values.", nameof(inputReferences));
        }

        Id = id;
        StepType = stepType;
        Objective = objective;
        InputReferences = Array.AsReadOnly(references);
        ExpectedOutcome = expectedOutcome;
    }

    /// <summary>Gets the stable step identifier.</summary>
    public string Id { get; }

    /// <summary>Gets the registered step handler type.</summary>
    public string StepType { get; }

    /// <summary>Gets the step objective supplied to the handler.</summary>
    public string Objective { get; }

    /// <summary>Gets the identifiers of prior step outputs explicitly mapped to this step.</summary>
    public IReadOnlyList<string> InputReferences { get; }

    /// <summary>Gets the optional expected outcome description.</summary>
    public string? ExpectedOutcome { get; }
}
