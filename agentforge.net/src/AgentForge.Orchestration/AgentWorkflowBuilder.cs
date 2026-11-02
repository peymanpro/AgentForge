using AgentForge.Abstractions.Execution;

namespace AgentForge.Orchestration;

/// <summary>
/// Builds an ordered workflow from application operations and agents with explicit data dependencies.
/// </summary>
public sealed class AgentWorkflowBuilder<TWorkflowInput>
{
    private readonly string _workflowId;
    private readonly WorkflowOptions _options;
    private readonly List<IWorkflowStep<TWorkflowInput>> _steps = [];
    private readonly Dictionary<string, WorkflowConditionDefinition<TWorkflowInput>> _conditions =
        new(StringComparer.Ordinal);

    internal AgentWorkflowBuilder(string workflowId, WorkflowOptions? options)
    {
        ValidateIdentifier(workflowId, nameof(workflowId));
        _workflowId = workflowId;
        _options = options ?? new WorkflowOptions();
    }

    /// <summary>
    /// Adds an application-defined asynchronous operation to the workflow.
    /// </summary>
    /// <typeparam name="TOutput">The type produced by the step.</typeparam>
    /// <param name="stepId">A unique step identifier.</param>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="inputReferences">Identifiers of prior step outputs explicitly exposed to this step.</param>
    /// <param name="runWhen">An optional previously declared condition.</param>
    public AgentWorkflowBuilder<TWorkflowInput> AddStep<TOutput>(
        string stepId,
        Func<WorkflowStepExecutionContext<TWorkflowInput>, CancellationToken, Task<TOutput>> operation,
        IEnumerable<string>? inputReferences = null,
        WorkflowStepCondition? runWhen = null)
    {
        ArgumentNullException.ThrowIfNull(operation);
        var references = ValidateNewStep(stepId, inputReferences);

        _steps.Add(new DelegateWorkflowStep<TWorkflowInput, TOutput>(
            stepId,
            references,
            runWhen,
            operation));

        return this;
    }

    /// <summary>
    /// Adds an agent as a workflow step without exposing its internal implementation to the orchestrator.
    /// </summary>
    /// <typeparam name="TAgentInput">The input type expected by the agent.</typeparam>
    /// <typeparam name="TAgentOutput">The output type produced by the agent.</typeparam>
    /// <param name="stepId">A unique step identifier.</param>
    /// <param name="agent">The independently implemented agent.</param>
    /// <param name="inputFactory">Maps the workflow input and explicitly referenced outputs to the agent input.</param>
    /// <param name="inputReferences">Identifiers of prior step outputs explicitly exposed to the mapping function.</param>
    /// <param name="runWhen">An optional previously declared condition.</param>
    public AgentWorkflowBuilder<TWorkflowInput> AddAgentStep<TAgentInput, TAgentOutput>(
        string stepId,
        IAgent<TAgentInput, TAgentOutput> agent,
        Func<WorkflowStepExecutionContext<TWorkflowInput>, TAgentInput> inputFactory,
        IEnumerable<string>? inputReferences = null,
        WorkflowStepCondition? runWhen = null)
    {
        ArgumentNullException.ThrowIfNull(agent);
        ArgumentNullException.ThrowIfNull(inputFactory);
        var references = ValidateNewStep(stepId, inputReferences);

        _steps.Add(new AgentWorkflowStep<TWorkflowInput, TAgentInput, TAgentOutput>(
            stepId,
            references,
            runWhen,
            agent,
            inputFactory));

        return this;
    }

    /// <summary>
    /// Declares an application-defined condition using only outputs from earlier steps.
    /// </summary>
    /// <param name="conditionId">A unique condition identifier.</param>
    /// <param name="evaluator">The application-owned condition evaluator.</param>
    /// <param name="inputReferences">Identifiers of prior step outputs explicitly exposed to the evaluator.</param>
    public AgentWorkflowBuilder<TWorkflowInput> AddCondition(
        string conditionId,
        Func<WorkflowStepExecutionContext<TWorkflowInput>, CancellationToken, Task<bool>> evaluator,
        IEnumerable<string>? inputReferences = null)
    {
        ValidateIdentifier(conditionId, nameof(conditionId));
        ArgumentNullException.ThrowIfNull(evaluator);

        if (_conditions.ContainsKey(conditionId))
        {
            throw new ArgumentException($"Condition '{conditionId}' is already defined.", nameof(conditionId));
        }

        var references = ValidateExistingReferences(inputReferences, conditionId);
        _conditions.Add(
            conditionId,
            new WorkflowConditionDefinition<TWorkflowInput>(
                conditionId,
                references,
                _steps.Count,
                evaluator));

        return this;
    }

    /// <summary>
    /// Declares a synchronous application-owned condition.
    /// </summary>
    public AgentWorkflowBuilder<TWorkflowInput> AddCondition(
        string conditionId,
        Func<WorkflowStepExecutionContext<TWorkflowInput>, bool> evaluator,
        IEnumerable<string>? inputReferences = null)
    {
        ArgumentNullException.ThrowIfNull(evaluator);

        return AddCondition(
            conditionId,
            (context, cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(evaluator(context));
            },
            inputReferences);
    }

    /// <summary>
    /// Validates and creates the immutable workflow definition.
    /// </summary>
    public AgentWorkflow<TWorkflowInput> Build()
    {
        if (_steps.Count == 0)
        {
            throw new InvalidOperationException("A workflow must contain at least one step.");
        }

        for (var index = 0; index < _steps.Count; index++)
        {
            var step = _steps[index];
            if (step.RunWhen is not { } condition)
            {
                continue;
            }

            if (!_conditions.TryGetValue(condition.ConditionId, out var definition))
            {
                throw new InvalidOperationException(
                    $"Step '{step.Id}' references unknown condition '{condition.ConditionId}'.");
            }

            if (definition.DeclaredAfterStepCount > index)
            {
                throw new InvalidOperationException(
                    $"Condition '{condition.ConditionId}' must be declared before step '{step.Id}'.");
            }
        }

        var steps = Array.AsReadOnly(_steps.ToArray());
        var conditions = new System.Collections.ObjectModel.ReadOnlyDictionary<string, WorkflowConditionDefinition<TWorkflowInput>>(
            new Dictionary<string, WorkflowConditionDefinition<TWorkflowInput>>(_conditions, StringComparer.Ordinal));

        return new AgentWorkflow<TWorkflowInput>(
            new WorkflowDefinition<TWorkflowInput>(_workflowId, steps, conditions),
            _options);
    }

    private IReadOnlyList<string> ValidateReferences(IEnumerable<string>? inputReferences, string ownerId)
    {
        var references = inputReferences?.ToArray() ?? [];
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var reference in references)
        {
            ValidateIdentifier(reference, nameof(inputReferences));

            if (!seen.Add(reference))
            {
                throw new ArgumentException(
                    $"Output reference '{reference}' is repeated for '{ownerId}'.",
                    nameof(inputReferences));
            }

            if (!_steps.Any(step => string.Equals(step.Id, reference, StringComparison.Ordinal)))
            {
                throw new ArgumentException(
                    $"Output reference '{reference}' must refer to a step added before '{ownerId}'.",
                    nameof(inputReferences));
            }
        }

        return Array.AsReadOnly(references);
    }

    private IReadOnlyList<string> ValidateExistingReferences(IEnumerable<string>? inputReferences, string ownerId)
    {
        var references = inputReferences?.ToArray() ?? [];
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var reference in references)
        {
            ValidateIdentifier(reference, nameof(inputReferences));

            if (!seen.Add(reference))
            {
                throw new ArgumentException(
                    $"Output reference '{reference}' is repeated for condition '{ownerId}'.",
                    nameof(inputReferences));
            }

            if (!_steps.Any(step => string.Equals(step.Id, reference, StringComparison.Ordinal)))
            {
                throw new ArgumentException(
                    $"Condition '{ownerId}' can reference only outputs from earlier steps.",
                    nameof(inputReferences));
            }
        }

        return Array.AsReadOnly(references);
    }

    private IReadOnlyList<string> ValidateNewStep(string stepId, IEnumerable<string>? inputReferences)
    {
        ValidateIdentifier(stepId, nameof(stepId));

        if (_steps.Any(step => string.Equals(step.Id, stepId, StringComparison.Ordinal)))
        {
            throw new ArgumentException($"Step '{stepId}' is already defined.", nameof(stepId));
        }

        return ValidateReferences(inputReferences, stepId);
    }

    internal static void ValidateIdentifier(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value) ||
            value.Length > 64 ||
            !value.All(static character =>
                char.IsAsciiLetterOrDigit(character) || character is '_' or '-'))
        {
            throw new ArgumentException(
                "Identifiers must contain 1–64 ASCII letters, digits, underscores, or hyphens.",
                parameterName);
        }
    }
}
