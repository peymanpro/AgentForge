using System.Text.Json;
using AgentForge.Models.Chat;
using Xunit;

namespace AgentForge.Models.Tests;

public sealed class ChatMessageTests
{
    [Fact]
    public void AssistantToolCalls_PreservesToolCallProposals()
    {
        var message = ChatMessage.AssistantToolCalls([CreateToolCall("call-1", "lookup")], "Searching.");

        Assert.Equal(ChatMessageRole.Assistant, message.Role);
        Assert.Equal("Searching.", message.Content);
        Assert.Single(message.ToolCalls);
        Assert.Equal("lookup", message.ToolCalls[0].Name);
    }

    [Fact]
    public void AssistantToolCalls_RejectsDuplicateCallIdentifiers()
    {
        ModelToolCall[] calls =
        [
            CreateToolCall("same-id", "lookup"),
            CreateToolCall("same-id", "search")
        ];

        Assert.Throws<ArgumentException>(() => ChatMessage.AssistantToolCalls(calls));
    }

    [Fact]
    public void ToolResult_RequiresCallIdentifier()
    {
        Assert.Throws<ArgumentException>(() => ChatMessage.ToolResult(" ", "result"));
    }

    [Fact]
    public void ModelToolCall_ClonesArguments()
    {
        ModelToolCall call;
        using (var document = JsonDocument.Parse("""{"id":"original"}"""))
        {
            call = new ModelToolCall("call-1", "lookup", document.RootElement);
        }

        Assert.Equal("original", call.Arguments.GetProperty("id").GetString());
    }

    private static ModelToolCall CreateToolCall(string id, string name)
    {
        using var document = JsonDocument.Parse("""{"id":"item-1"}""");
        return new ModelToolCall(id, name, document.RootElement);
    }
}
