namespace AgentForge.Orchestration;

internal delegate Task<bool> WorkflowConditionEvaluator<TWorkflowInput>(
    WorkflowStepExecutionContext<TWorkflowInput> context,
    CancellationToken cancellationToken);

internal sealed class WorkflowConditionDefinition<TWorkflowInput>
{
    public WorkflowConditionDefinition(
        string id,
        IReadOnlyList<string> inputReferences,
        WorkflowConditionEvaluator<TWorkflowInput> evaluator)
    {
        Id = id;
        InputReferences = inputReferences;
        Evaluator = evaluator;
    }

    public string Id { get; }

    public IReadOnlyList<string> InputReferences { get; }

    public WorkflowConditionEvaluator<TWorkflowInput> Evaluator { get; }
}

internal sealed class WorkflowDefinition<TWorkflowInput>
{
    public WorkflowDefinition(
        string workflowId,
        IReadOnlyList<IWorkflowStep<TWorkflowInput>> steps,
        IReadOnlyDictionary<string, WorkflowConditionDefinition<TWorkflowInput>> conditions)
    {
        WorkflowId = workflowId;
        Steps = steps;
        Conditions = conditions;
    }

    public string WorkflowId { get; }

    public IReadOnlyList<IWorkflowStep<TWorkflowInput>> Steps { get; }

    public IReadOnlyDictionary<string, WorkflowConditionDefinition<TWorkflowInput>> Conditions { get; }
}
