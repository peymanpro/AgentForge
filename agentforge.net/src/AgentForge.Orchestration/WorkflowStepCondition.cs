namespace AgentForge.Orchestration;

/// <summary>
/// Specifies which outcome of a named workflow condition permits a step to run.
/// </summary>
public sealed class WorkflowStepCondition
{
    private WorkflowStepCondition(string conditionId, bool expectedValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conditionId);
        ConditionId = conditionId;
        ExpectedValue = expectedValue;
    }

    /// <summary>Gets the identifier of the condition that controls the step.</summary>
    public string ConditionId { get; }

    /// <summary>Gets the condition value required for the step to execute.</summary>
    public bool ExpectedValue { get; }

    /// <summary>Creates a condition that permits a step when the condition evaluates to true.</summary>
    public static WorkflowStepCondition WhenTrue(string conditionId) =>
        new(conditionId, true);

    /// <summary>Creates a condition that permits a step when the condition evaluates to false.</summary>
    public static WorkflowStepCondition WhenFalse(string conditionId) =>
        new(conditionId, false);
}
