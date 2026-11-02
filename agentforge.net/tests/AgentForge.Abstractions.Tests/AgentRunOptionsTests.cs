using AgentForge.Abstractions.Execution;

using Xunit;

namespace AgentForge.Abstractions.Tests;

public sealed class AgentRunOptionsTests
{
    [Fact]
    public void Validate_AllowsUnspecifiedLimits()
    {
        var options = new AgentRunOptions();

        options.Validate();
    }

    [Fact]
    public void Validate_AllowsPositiveLimits()
    {
        var options = new AgentRunOptions
        {
            MaxSteps = 10,
            MaxModelRequests = 8,
            MaxToolInvocations = 12,
            Timeout = TimeSpan.FromSeconds(30)
        };

        options.Validate();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_RejectsNonPositiveStepLimits(int value)
    {
        var options = new AgentRunOptions { MaxSteps = value };

        Assert.Throws<ArgumentOutOfRangeException>(options.Validate);
    }

    [Fact]
    public void Validate_RejectsNonPositiveTimeout()
    {
        var options = new AgentRunOptions { Timeout = TimeSpan.Zero };

        Assert.Throws<ArgumentOutOfRangeException>(options.Validate);
    }
}
