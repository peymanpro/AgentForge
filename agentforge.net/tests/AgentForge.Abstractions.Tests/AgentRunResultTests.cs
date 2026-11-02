using AgentForge.Abstractions.Execution;

using Xunit;

namespace AgentForge.Abstractions.Tests;

public sealed class AgentRunResultTests
{
    private static readonly DateTimeOffset StartedAt =
        new(2026, 11, 2, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Completed_ContainsOutputWithoutFailure()
    {
        var completedAt = StartedAt.AddSeconds(2);

        var result = AgentRunResult<string>.Completed("done", StartedAt, completedAt);

        Assert.Equal(AgentRunStatus.Completed, result.Status);
        Assert.Equal("done", result.Output);
        Assert.Null(result.Failure);
        Assert.Equal(TimeSpan.FromSeconds(2), result.Duration);
    }

    [Fact]
    public void PartiallyCompleted_RequiresAndPreservesFailure()
    {
        var failure = new AgentFailure(
            AgentFailureCategory.ToolFailure,
            "tool.timeout",
            "The tool did not complete before its timeout.");

        var result = AgentRunResult<string>.PartiallyCompleted(
            "partial",
            failure,
            StartedAt,
            StartedAt.AddSeconds(3));

        Assert.Equal(AgentRunStatus.PartiallyCompleted, result.Status);
        Assert.Equal("partial", result.Output);
        Assert.Equal(failure, result.Failure);
    }

    [Fact]
    public void Stopped_RejectsCompletedStatus()
    {
        var failure = new AgentFailure(
            AgentFailureCategory.InternalFailure,
            "run.invalid",
            "The run stopped unexpectedly.");

        Assert.Throws<ArgumentException>(() =>
            AgentRunResult<string>.Stopped(
                AgentRunStatus.Completed,
                failure,
                StartedAt,
                StartedAt.AddSeconds(1)));
    }

    [Fact]
    public void Completed_RejectsAnEndTimeBeforeStart()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            AgentRunResult<string>.Completed("done", StartedAt, StartedAt.AddSeconds(-1)));
    }

    [Fact]
    public void Stopped_RequiresAnExplanation()
    {
        Assert.Throws<ArgumentNullException>(() =>
            AgentRunResult<string>.Stopped(
                AgentRunStatus.Cancelled,
                null!,
                StartedAt,
                StartedAt.AddSeconds(1)));
    }
}
