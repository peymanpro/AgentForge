using AgentForge.Abstractions.Execution;
using AgentForge.Orchestration;

var workflow = AgentWorkflow.Create<ResearchTask>("research-and-review")
    .AddAgentStep(
        "research",
        new ResearchAgent(),
        context => context.WorkflowInput)
    .AddAgentStep(
        "review",
        new ReviewAgent(),
        context => context.GetOutput<ResearchDraft>("research"),
        ["research"])
    .AddCondition(
        "needs_revision",
        context => context.GetOutput<ReviewResult>("review").NeedsRevision,
        ["review"])
    .AddAgentStep(
        "revise",
        new RevisionAgent(),
        context => new RevisionRequest(
            context.GetOutput<ResearchDraft>("research"),
            context.GetOutput<ReviewResult>("review")),
        ["research", "review"],
        WorkflowStepCondition.WhenTrue("needs_revision"))
    .AddStep(
        "publish_revised",
        (context, _) => Task.FromResult(context.GetOutput<PublishedAnswer>("revise")),
        ["revise"],
        WorkflowStepCondition.WhenTrue("needs_revision"))
    .AddStep(
        "publish_original",
        (context, _) => Task.FromResult(new PublishedAnswer(
            context.GetOutput<ResearchDraft>("research").Summary)),
        ["research"],
        WorkflowStepCondition.WhenFalse("needs_revision"))
    .Build();

var result = await workflow.RunAsync(
    new ResearchTask("How should AI agent workflows be tested?"),
    AgentRunContext.Create(),
    new AgentRunOptions());

Console.WriteLine($"Workflow: {workflow.WorkflowId}");
Console.WriteLine($"Status: {result.Status}");

if (result.Output is not null)
{
    foreach (var step in result.Output.Steps)
    {
        Console.WriteLine($"{step.StepId}: {step.Status}");
    }

    var published = result.Output.Steps
        .FirstOrDefault(step => step.StepId is "publish_revised" or "publish_original");

    if (published?.Output is PublishedAnswer answer)
    {
        Console.WriteLine();
        Console.WriteLine(answer.Text);
    }
}
else if (result.Failure is not null)
{
    Console.Error.WriteLine($"{result.Failure.Code}: {result.Failure.Message}");
}

internal sealed record ResearchTask(string Topic);

internal sealed record ResearchDraft(string Topic, string Summary);

internal sealed record ReviewResult(bool NeedsRevision, string Feedback);

internal sealed record RevisionRequest(ResearchDraft Draft, ReviewResult Review);

internal sealed record PublishedAnswer(string Text);

internal sealed class ResearchAgent : IAgent<ResearchTask, ResearchDraft>
{
    public Task<AgentRunResult<ResearchDraft>> RunAsync(
        ResearchTask input,
        AgentRunContext context,
        AgentRunOptions options,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var now = DateTimeOffset.UtcNow;
        var draft = new ResearchDraft(
            input.Topic,
            $"A useful evaluation plan for '{input.Topic}' should combine deterministic test cases, explicit success criteria, and failure-path coverage.");

        return Task.FromResult(AgentRunResult<ResearchDraft>.Completed(draft, now, now));
    }
}

internal sealed class ReviewAgent : IAgent<ResearchDraft, ReviewResult>
{
    public Task<AgentRunResult<ReviewResult>> RunAsync(
        ResearchDraft input,
        AgentRunContext context,
        AgentRunOptions options,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var review = new ReviewResult(
            NeedsRevision: !input.Summary.Contains("security", StringComparison.OrdinalIgnoreCase),
            Feedback: "Add an explicit security and isolation dimension.");

        var now = DateTimeOffset.UtcNow;
        return Task.FromResult(AgentRunResult<ReviewResult>.Completed(review, now, now));
    }
}

internal sealed class RevisionAgent : IAgent<RevisionRequest, PublishedAnswer>
{
    public Task<AgentRunResult<PublishedAnswer>> RunAsync(
        RevisionRequest input,
        AgentRunContext context,
        AgentRunOptions options,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var revised = new PublishedAnswer(
            $"{input.Draft.Summary} {input.Review.Feedback}");

        var now = DateTimeOffset.UtcNow;
        return Task.FromResult(AgentRunResult<PublishedAnswer>.Completed(revised, now, now));
    }
}
