using AgentForge.Models.Chat;
using Xunit;

namespace AgentForge.Models.Tests;

public sealed class ModelResponseTests
{
    [Fact]
    public void Constructor_AcceptsCompletedAssistantMessage()
    {
        var response = new ModelResponse(
            ChatMessage.Assistant("Hello."),
            ModelFinishReason.Stop,
            new ModelUsage(inputTokens: 5, outputTokens: 3, totalTokens: 8));

        Assert.Equal(ModelFinishReason.Stop, response.FinishReason);
        Assert.Equal(8L, response.Usage?.TotalTokens);
    }

    [Fact]
    public void Constructor_RequiresToolCallsForToolCallFinishReason()
    {
        Assert.Throws<ArgumentException>(() =>
            new ModelResponse(ChatMessage.Assistant("No tool call."), ModelFinishReason.ToolCalls));
    }

    [Fact]
    public void Constructor_RequiresToolCallFinishReasonWhenCallsArePresent()
    {
        using var document = System.Text.Json.JsonDocument.Parse("""{"id":"item-1"}""");
        var message = ChatMessage.AssistantToolCalls([
            new ModelToolCall("call-1", "lookup", document.RootElement)
        ]);

        Assert.Throws<ArgumentException>(() =>
            new ModelResponse(message, ModelFinishReason.Stop));
    }

    [Fact]
    public void Usage_RejectsNegativeTokenCounts()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ModelUsage(inputTokens: -1));
    }
}
