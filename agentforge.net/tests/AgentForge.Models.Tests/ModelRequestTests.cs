using AgentForge.Models.Chat;
using Xunit;

namespace AgentForge.Models.Tests;

public sealed class ModelRequestTests
{
    [Fact]
    public void Constructor_RejectsAnEmptyMessageCollection()
    {
        Assert.Throws<ArgumentException>(() => new ModelRequest([]));
    }

    [Fact]
    public void Constructor_CopiesMessagesAndDefaultsOptions()
    {
        var messages = new List<ChatMessage> { ChatMessage.User("Hello.") };

        var request = new ModelRequest(messages);
        messages.Clear();

        Assert.Single(request.Messages);
        Assert.NotNull(request.Options);
    }

    [Fact]
    public void GenerationOptions_CopiesStopSequences()
    {
        var stopSequences = new List<string> { "END" };

        var options = new ModelGenerationOptions(stopSequences: stopSequences);
        stopSequences[0] = "CHANGED";

        Assert.Equal("END", Assert.Single(options.StopSequences));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GenerationOptions_RejectsNonPositiveOutputLimit(int value)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ModelGenerationOptions(maxOutputTokens: value));
    }

    [Fact]
    public void GenerationOptions_RejectsInvalidTopP()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ModelGenerationOptions(topP: 1.1));
    }

    [Fact]
    public void GenerationOptions_RejectsEmptyStopSequences()
    {
        Assert.Throws<ArgumentException>(() =>
            new ModelGenerationOptions(stopSequences: ["END", " "]));
    }
}
