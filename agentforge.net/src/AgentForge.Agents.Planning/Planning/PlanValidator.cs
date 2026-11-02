namespace AgentForge.Agents.Planning.Planning;

/// <summary>
/// Validates plan structure, registered step types, and backward-only output references.
/// </summary>
public sealed class PlanValidator
{
    private const int MaxObjectiveLength = 2_000;
    private const int MaxStepObjectiveLength = 2_000;
    private const int MaxExpectedOutcomeLength = 1_000;
    private const int MaxStepIdLength = 64;
    private const int MaxStepTypeLength = 64;

    /// <summary>
    /// Validates a plan candidate before any step handler is invoked.
    /// </summary>
    /// <param name="plan">The plan candidate.</param>
    /// <param name="allowedStepTypes">The exact step types registered for this execution.</param>
    /// <param name="maxPlanSteps">The maximum allowed number of steps.</param>
    public PlanValidationResult Validate(
        ExecutionPlan plan,
        IReadOnlyCollection<string> allowedStepTypes,
        int maxPlanSteps)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(allowedStepTypes);

        if (maxPlanSteps <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPlanSteps));
        }

        var issues = new List<PlanValidationIssue>();

        if (string.IsNullOrWhiteSpace(plan.Objective))
        {
            issues.Add(new("plan.objective_required", "$.objective", "A plan objective is required."));
        }
        else if (plan.Objective.Length > MaxObjectiveLength)
        {
            issues.Add(new("plan.objective_too_long", "$.objective", "The plan objective exceeds the supported length."));
        }

        if (plan.Steps.Count == 0)
        {
            issues.Add(new("plan.steps_required", "$.steps", "A plan must contain at least one step."));
        }

        if (plan.Steps.Count > maxPlanSteps)
        {
            issues.Add(new("plan.step_limit_exceeded", "$.steps", "The plan exceeds the configured maximum step count."));
        }

        var allowed = new HashSet<string>(allowedStepTypes, StringComparer.Ordinal);
        var seenIds = new HashSet<string>(StringComparer.Ordinal);

        for (var index = 0; index < plan.Steps.Count; index++)
        {
            var step = plan.Steps[index];
            var path = $"$.steps[{index}]";

            var validUniqueId = IsValidIdentifier(step.Id, MaxStepIdLength);
            if (!validUniqueId)
            {
                issues.Add(new("plan.invalid_step_id", $"{path}.id", "A step identifier must be 1–64 ASCII letters, digits, underscores, or hyphens."));
            }
            else if (seenIds.Contains(step.Id))
            {
                issues.Add(new("plan.duplicate_step_id", $"{path}.id", "Step identifiers must be unique within a plan."));
                validUniqueId = false;
            }

            if (!IsValidIdentifier(step.StepType, MaxStepTypeLength))
            {
                issues.Add(new("plan.invalid_step_type", $"{path}.stepType", "A step type must be a valid registered identifier."));
            }
            else if (!allowed.Contains(step.StepType))
            {
                issues.Add(new("plan.unknown_step_type", $"{path}.stepType", "The plan references a step type that is not available to this execution."));
            }

            if (string.IsNullOrWhiteSpace(step.Objective))
            {
                issues.Add(new("plan.step_objective_required", $"{path}.objective", "A step objective is required."));
            }
            else if (step.Objective.Length > MaxStepObjectiveLength)
            {
                issues.Add(new("plan.step_objective_too_long", $"{path}.objective", "The step objective exceeds the supported length."));
            }

            if (step.ExpectedOutcome is { Length: > MaxExpectedOutcomeLength })
            {
                issues.Add(new("plan.expected_outcome_too_long", $"{path}.expectedOutcome", "The expected outcome exceeds the supported length."));
            }

            var references = new HashSet<string>(StringComparer.Ordinal);
            foreach (var reference in step.InputReferences)
            {
                if (!IsValidIdentifier(reference, MaxStepIdLength))
                {
                    issues.Add(new("plan.invalid_input_reference", $"{path}.inputReferences", "An input reference must be a valid step identifier."));
                    continue;
                }

                if (!references.Add(reference))
                {
                    issues.Add(new("plan.duplicate_input_reference", $"{path}.inputReferences", "Input references cannot be repeated."));
                    continue;
                }

                if (!seenIds.Contains(reference))
                {
                    issues.Add(new("plan.forward_or_unknown_reference", $"{path}.inputReferences", "A step may reference only outputs from earlier steps."));
                }
            }

            if (validUniqueId)
            {
                seenIds.Add(step.Id);
            }
        }

        return issues.Count == 0
            ? PlanValidationResult.Valid()
            : PlanValidationResult.Invalid(issues);
    }

    private static bool IsValidIdentifier(string value, int maxLength)
    {
        return value.Length is > 0 &&
               value.Length <= maxLength &&
               value.All(static character =>
                   char.IsAsciiLetterOrDigit(character) || character is '_' or '-');
    }
}
