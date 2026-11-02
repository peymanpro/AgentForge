using AgentForge.Abstractions.Execution;

namespace AgentForge.Orchestration;

internal sealed record WorkflowStepExecutionOutcome(
    WorkflowStepStatus Status,
    object? Output,
    AgentFailure? Failure,
    Guid? ChildRunId);

internal interface IWorkflowStep<TWorkflowInput>
{
    string Id { get; }

    IReadOnlyList<string> InputReferences { get; }

    WorkflowStepCondition? RunWhen { get; }

    Task<WorkflowStepExecutionOutcome> ExecuteAsync(
        WorkflowStepExecutionContext<TWorkflowInput> context,
        AgentRunOptions runOptions,
        TimeSpan remainingTimeout,
        TimeProvider timeProvider,
        CancellationToken cancellationToken);
}

internal sealed class DelegateWorkflowStep<TWorkflowInput, TOutput>(
    string id,
    IReadOnlyList<string> inputReferences,
    WorkflowStepCondition? runWhen,
    Func<WorkflowStepExecutionContext<TWorkflowInput>, CancellationToken, Task<TOutput>> operation)
    : IWorkflowStep<TWorkflowInput>
{
    public string Id => id;

    public IReadOnlyList<string> InputReferences => inputReferences;

    public WorkflowStepCondition? RunWhen => runWhen;

    public async Task<WorkflowStepExecutionOutcome> ExecuteAsync(
        WorkflowStepExecutionContext<TWorkflowInput> context,
        AgentRunOptions runOptions,
        TimeSpan remainingTimeout,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var output = await operation(context, cancellationToken).ConfigureAwait(false);
        return new WorkflowStepExecutionOutcome(
            WorkflowStepStatus.Completed,
            output,
            null,
            null);
    }
}

internal sealed class AgentWorkflowStep<TWorkflowInput, TAgentInput, TAgentOutput>(
    string id,
    IReadOnlyList<string> inputReferences,
    WorkflowStepCondition? runWhen,
    IAgent<TAgentInput, TAgentOutput> agent,
    Func<WorkflowStepExecutionContext<TWorkflowInput>, TAgentInput> inputFactory)
    : IWorkflowStep<TWorkflowInput>
{
    public string Id => id;

    public IReadOnlyList<string> InputReferences => inputReferences;

    public WorkflowStepCondition? RunWhen => runWhen;

    public async Task<WorkflowStepExecutionOutcome> ExecuteAsync(
        WorkflowStepExecutionContext<TWorkflowInput> context,
        AgentRunOptions runOptions,
        TimeSpan remainingTimeout,
        TimeProvider timeProvider,
        CancellationToken cancellationToken)
    {
        var input = inputFactory(context);
        var childContext = context.RunContext.CreateChild(timeProvider.GetUtcNow());
        var childOptions = new AgentRunOptions
        {
            MaxSteps = runOptions.MaxSteps,
            MaxModelRequests = runOptions.MaxModelRequests,
            MaxToolInvocations = runOptions.MaxToolInvocations,
            Timeout = remainingTimeout,
            EventSink = runOptions.EventSink
        };

        var result = await agent.RunAsync(
            input,
            childContext,
            childOptions,
            cancellationToken).ConfigureAwait(false);

        var status = result.Status switch
        {
            AgentRunStatus.Completed => WorkflowStepStatus.Completed,
            AgentRunStatus.PartiallyCompleted => WorkflowStepStatus.PartiallyCompleted,
            AgentRunStatus.Failed => WorkflowStepStatus.Failed,
            AgentRunStatus.Cancelled => WorkflowStepStatus.Cancelled,
            AgentRunStatus.TimedOut => WorkflowStepStatus.TimedOut,
            AgentRunStatus.BudgetExhausted => WorkflowStepStatus.BudgetExhausted,
            AgentRunStatus.Rejected => WorkflowStepStatus.Rejected,
            _ => WorkflowStepStatus.Failed
        };

        return new WorkflowStepExecutionOutcome(
            status,
            result.Output,
            result.Failure,
            childContext.RunId);
    }
}
