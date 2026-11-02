using AgentForge.Abstractions.Execution;

using Xunit;

namespace AgentForge.Abstractions.Tests;

public sealed class AgentRunContextTests
{
    [Fact]
    public void Create_GeneratesDistinctRunIdentifiers()
    {
        var first = AgentRunContext.Create();
        var second = AgentRunContext.Create();

        Assert.NotEqual(first.RunId, second.RunId);
        Assert.Null(first.ParentRunId);
    }

    [Fact]
    public void CreateChild_PreservesParentCorrelation()
    {
        var parent = AgentRunContext.Create();
        var child = parent.CreateChild();

        Assert.NotEqual(parent.RunId, child.RunId);
        Assert.Equal(parent.RunId, child.ParentRunId);
    }

    [Fact]
    public void Create_RejectsAnEmptyParentIdentifier()
    {
        Assert.Throws<ArgumentException>(() => AgentRunContext.Create(Guid.Empty));
    }
}
