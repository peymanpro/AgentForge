using System.Text.Json;
using AgentForge.Abstractions.Execution;
using AgentForge.Tools;
using Xunit;

namespace AgentForge.Tools.Tests;

public sealed class ToolRegistryTests
{
    [Fact]
    public void Constructor_RejectsDuplicateNames()
    {
        var first = new FakeTool("lookup");
        var second = new FakeTool("lookup");

        Assert.Throws<ArgumentException>(() => new ToolRegistry([first, second]));
    }

    [Fact]
    public void TryGet_UsesExactCaseSensitiveNames()
    {
        var tool = new FakeTool("lookup");
        var registry = new ToolRegistry([tool]);

        Assert.True(registry.TryGet("lookup", out var found));
        Assert.Same(tool, found);
        Assert.False(registry.TryGet("Lookup", out _));
    }

    [Fact]
    public void Definitions_ExposeOnlyRegisteredTools()
    {
        var registry = new ToolRegistry([new FakeTool("lookup"), new FakeTool("search")]);

        Assert.Collection(
            registry.Definitions,
            definition => Assert.Equal("lookup", definition.Name),
            definition => Assert.Equal("search", definition.Name));
    }

    private sealed class FakeTool(string name) : ITool
    {
        public ToolDefinition Definition { get; } = CreateDefinition(name);

        public ToolValidationResult Validate(JsonElement arguments) => ToolValidationResult.Valid();

        public Task<JsonElement> ExecuteAsync(
            JsonElement arguments,
            AgentRunContext runContext,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(CreateJson("""{"ok":true}"""));
        }
    }

    private static ToolDefinition CreateDefinition(string name)
    {
        using var document = JsonDocument.Parse("""{"type":"object"}""");
        return new ToolDefinition(name, "A test tool.", document.RootElement);
    }

    private static JsonElement CreateJson(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}
