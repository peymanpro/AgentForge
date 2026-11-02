using System.Collections.ObjectModel;
using AgentForge.Abstractions.Execution;

namespace AgentForge.Orchestration;

/// <summary>
/// Builds a bounded sequential workflow with explicit typed input mappings and conditional step selection.
/// </summary>
public sealed class AgentWorkflowBuilder<TWorkflowInput>
{
    private readonly string _workflowId;
    private readonly List<IWorkflowStep<TWorkflowInput>> _steps = [];
    private readonly Dictionary<string, WorkflowConditionDefinition<TWorkflowInput>> _conditions =
        new(StringComparer.Ordinal);

    internal AgentWorkflowBuilder(string workflowId)
    {
        ValidateIdentifier(workflowId, nameof(workflowId));
        _workflowId = workflowId;
    }

    /// <summary>
    /// Adds a named condition. It is evaluated at most once per workflow run when first referenced by a step.
    /// </summary>
    /// <param name="conditionId">A stable condition identifier.</param>
    /// <param name="inputReferences">Earlier step outputs explicitly exposed to the condition.</param>
    /// <param name="evaluator">An application-defined condition evaluator.</param>
    public AgentWorkflowBuilder<TWorkflowInput> AddCondition(
        string conditionId,
        IEnumerable<string> inputReferences,
        Func<WorkflowStepExecutionContext<TWorkflowInput>, CancellationToken, Task<bool>> evaluator)
    {
        ValidateIdentifier(conditionId, nameof(conditionId));
        ArgumentNullException.ThrowIfNull(inputReferences);
        ArgumentNullException.ThrowIfNull(evaluator);

        if (_conditions.ContainsKey(conditionId))
        {
            throw new ArgumentException($"A workflow condition named '{conditionId}' is already registered.", nameof(conditionId));
        }

        _conditions.Add(
            conditionId,
            new WorkflowConditionDefinition<TWorkflowInput>(
                conditionId,
                NormalizeReferences(inputReferences, nameof(inputReferences)),
                (context, cancellationToken) => evaluator(context, cancellationToken)));

        return this;
    }

    /// <summary>
    /// Adds a synchronous application-defined condition.
    /// </summary>
    public AgentWorkflowBuilder<TWorkflowInput> AddCondition(
        string conditionId,
        IEnumerable<string> inputReferences,
        Func<WorkflowStepExecutionContext<TWorkflowInput>, bool> evaluator)
    {
        ArgumentNullException.ThrowIfNull(evaluator);

        return AddCondition(
            conditionId,
            inputReferences,
            (context, _) => Task.FromResult(evaluator(context)));
    }

    /// <summary>
    /// Adds an application-defined asynchronous workflow operation.
    /// </summary>
    /// <typeparam name="TOutput">The output type produced by the operation.</typeparam>
    /// <param name="stepId">A stable step identifier.</param>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="inputReferences">Earlier step outputs explicitly exposed to the operation.</param>
    /// <param name="runWhen">An optional condition and expected value required for execution.</param>
    public AgentWorkflowBuilder<TWorkflowInput> AddStep<TOutput>(
        string stepId,
        Func<WorkflowStepExecutionContext<TWorkflowInput>, CancellationToken, Task<TOutput>> operation,
        IEnumerable<string>? inputReferences = null,
        WorkflowStepCondition? runWhen = null)
    {
        ValidateIdentifier(stepId, nameof(stepId));
        ArgumentNullException.ThrowIfNull(operation);

        _steps.Add(new DelegateWorkflowStep<TWorkflowInput, TOutput>(
            stepId,
            NormalizeReferences(inputReferences ?? Array.Empty<string>(), nameof(inputReferences)),
            runWhen,
            operation));

        return this;
    }

    /// <summary>
    /// Adds an existing AgentForge agent through an explicit typed input mapping.
    /// </summary>
    /// <typeparam name="TAgentInput">The input type expected by the agent.</typeparam>
    /// <typeparam name="TAgentOutput">The output type produced by the agent.</typeparam>
    /// <param name="stepId">A stable step identifier.</param>
    /// <param name="agent">The agent to invoke as a nested run.</param>
    /// <param name="inputFactory">Maps workflow input and declared prior outputs into the agent input.</param>
    /// <param name="inputReferences">Earlier step outputs explicitly exposed to the input factory.</param>
    /// <param name="runWhen">An optional condition and expected value required for execution.</param>
    public AgentWorkflowBuilder<TWorkflowInput> AddAgentStep<TAgentInput, TAgentOutput>(
        string stepId,
        IAgent<TAgentInput, TAgentOutput> agent,
        Func<WorkflowStepExecutionContext<TWorkflowInput>, TAgentInput> inputFactory,
        IEnumerable<string>? inputReferences = null,
        WorkflowStepCondition? runWhen = null)
    {
        ValidateIdentifier(stepId, nameof(stepId));
        ArgumentNullException.ThrowIfNull(agent);
        ArgumentNullException.ThrowIfNull(inputFactory);

        _steps.Add(new AgentWorkflowStep<TWorkflowInput, TAgentInput, TAgentOutput>(
            stepId,
            NormalizeReferences(inputReferences ?? Array.Empty<string>(), nameof(inputReferences)),
            runWhen,
            agent,
            inputFactory));

        return this;
    }

    /// <summary>
    /// Validates step ordering and references, then builds an immutable workflow.
    /// </summary>
    public AgentWorkflow<TWorkflowInput> Build(
        AgentWorkflowOptions? options = null,
        TimeProvider? timeProvider = null)
    {
        if (_steps.Count == 0)
        {
            throw new InvalidOperationException("A workflow must contain at least one step.");
        }

        var seenSteps = new HashSet<string>(StringComparer.Ordinal);
        var firstConditionUse = new HashSet<string>(StringComparer.Ordinal);

        foreach (var step in _steps)
        {
            if (!seenSteps.Add(step.Id))
            {
                throw new InvalidOperationException($"Workflow step identifier '{step.Id}' is duplicated.");
            }

            ValidateEarlierReferences(step.Id, step.InputReferences, seenSteps);

            if (step.RunWhen is not null)
            {
                if (!_conditions.TryGetValue(step.RunWhen.ConditionId, out var condition))
                {
                    throw new InvalidOperationException(
                        $"Workflow step '{step.Id}' references unknown condition '{step.RunWhen.ConditionId}'.");
                }

                if (firstConditionUse.Add(condition.Id))
                {
                    ValidateEarlierReferences(step.Id, condition.InputReferences, seenSteps);
                }
            }
        }

        var conditionCopy = new ReadOnlyDictionary<string, WorkflowConditionDefinition<TWorkflowInput>>(
            new Dictionary<string, WorkflowConditionDefinition<TWorkflowInput>>(_conditions, StringComparer.Ordinal));

        return new AgentWorkflow<TWorkflowInput>(
            new WorkflowDefinition<TWorkflowInput>(
                _workflowId,
                Array.AsReadOnly(_steps.ToArray()),
                conditionCopy),
            options ?? new AgentWorkflowOptions(),
            timeProvider ?? TimeProvider.System);
    }

    private static IReadOnlyList<string> NormalizeReferences(
        IEnumerable<string> references,
        string parameterName)
    {
        var materialized = references.ToArray();

        foreach (var reference in materialized)
        {
            ValidateIdentifier(reference, parameterName);
        }

        if (materialized.Distinct(StringComparer.Ordinal).Count() != materialized.Length)
        {
            throw new ArgumentException("Input references cannot contain duplicates.", parameterName);
        }

        return Array.AsReadOnly(materialized);
    }

    private static void ValidateEarlierReferences(
        string currentStepId,
        IReadOnlyList<string> references,
        IReadOnlySet<string> earlierStepIds)
    {
        foreach (var reference in references)
        {
            if (!earlierStepIds.Contains(reference) ||
                string.Equals(reference, currentStepId, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Workflow step '{currentStepId}' can reference only outputs from earlier steps; '{reference}' is not an earlier step.");
            }
        }
    }

    private static void ValidateIdentifier(string value, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);

        if (value.Length > 64 ||
            !value.All(static character =>
                char.IsAsciiLetterOrDigit(character) || character is '_' or '-'))
        {
            throw new ArgumentException(
                "Workflow identifiers must be 1–64 ASCII letters, digits, underscores, or hyphens.",
                parameterName);
        }
    }
}

/// <summary>
/// Provides the entry point for building typed AgentForge workflows.
/// </summary>
public static class AgentWorkflow
{
    /// <summary>Creates a workflow builder for the supplied application input type.</summary>
    public static AgentWorkflowBuilder<TWorkflowInput> Create<TWorkflowInput>(string workflowId = "workflow")
    {
        return new AgentWorkflowBuilder<TWorkflowInput>(workflowId);
    }
}
