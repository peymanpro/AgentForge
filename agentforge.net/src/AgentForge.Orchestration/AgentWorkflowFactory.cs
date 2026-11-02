namespace AgentForge.Orchestration;

/// <summary>
/// Creates typed workflow builders.
/// </summary>
public static class AgentWorkflow
{
    /// <summary>
    /// Creates a workflow builder for the specified input type.
    /// </summary>
    /// <typeparam name="TWorkflowInput">The input type accepted by the workflow.</typeparam>
    /// <param name="workflowId">A stable workflow identifier.</param>
    /// <param name="options">Optional finite workflow defaults.</param>
    public static AgentWorkflowBuilder<TWorkflowInput> Create<TWorkflowInput>(
        string workflowId,
        WorkflowOptions? options = null)
    {
        return new AgentWorkflowBuilder<TWorkflowInput>(workflowId, options);
    }
}
