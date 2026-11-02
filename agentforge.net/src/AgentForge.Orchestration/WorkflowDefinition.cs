namespace AgentForge.Orchestration;

internal sealed class WorkflowConditionDefinition<TWorkflowInput>
{
    public WorkflowConditionDefinition(
        string id,
        IReadOnlyList<string> inputReferences,
        int declaredAfterStepCount,
        Func<WorkflowStepExecutionContext<TWorkflowInput>, CancellationToken, Task<bool>> evaluator)
    {
        Id = id;
        InputReferences = inputReferences;
        DeclaredAfterStepCount = declaredAfterStepCount;
        Evaluator = evaluator;
    }

    public string Id { get; }

    public IReadOnlyList<string> InputReferences { get; }

    public int DeclaredAfterStepCount { get; }

    public Func<WorkflowStepExecutionContext<TWorkflowInput>, CancellationToken, Task<bool>> Evaluator { get; }
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
