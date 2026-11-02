using System.Collections.ObjectModel;

namespace AgentForge.Abstractions.Execution;

/// <summary>
/// Represents a structured, non-sensitive event emitted during an agent run.
/// </summary>
public sealed class AgentRunEvent
{
    /// <summary>
    /// Creates an execution event.
    /// </summary>
    /// <param name="runId">The run that emitted the event.</param>
    /// <param name="parentRunId">The parent run identifier, when nested.</param>
    /// <param name="name">A stable event name such as <c>run.started</c> or <c>tool.completed</c>.</param>
    /// <param name="occurredAt">The event timestamp.</param>
    /// <param name="attributes">Optional non-sensitive metadata.</param>
    public AgentRunEvent(
        Guid runId,
        Guid? parentRunId,
        string name,
        DateTimeOffset occurredAt,
        IReadOnlyDictionary<string, string>? attributes = null)
    {
        if (runId == Guid.Empty)
        {
            throw new ArgumentException("A run identifier cannot be empty.", nameof(runId));
        }

        if (parentRunId == Guid.Empty)
        {
            throw new ArgumentException("A parent run identifier cannot be empty.", nameof(parentRunId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (name.Length > 128 || !name.All(static character =>
                char.IsAsciiLetterOrDigit(character) || character is '.' or '_' or '-'))
        {
            throw new ArgumentException(
                "An event name must be at most 128 characters and contain only ASCII letters, digits, periods, underscores, or hyphens.",
                nameof(name));
        }

        var copiedAttributes = new Dictionary<string, string>(StringComparer.Ordinal);
        if (attributes is not null)
        {
            foreach (var attribute in attributes)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(attribute.Key);
                ArgumentNullException.ThrowIfNull(attribute.Value);

                if (!copiedAttributes.TryAdd(attribute.Key, attribute.Value))
                {
                    throw new ArgumentException("Event attribute keys must be unique.", nameof(attributes));
                }
            }
        }

        EventId = Guid.NewGuid();
        RunId = runId;
        ParentRunId = parentRunId;
        Name = name;
        OccurredAt = occurredAt;
        Attributes = new ReadOnlyDictionary<string, string>(copiedAttributes);
    }

    /// <summary>
    /// Gets the unique event identifier.
    /// </summary>
    public Guid EventId { get; }

    /// <summary>
    /// Gets the run identifier.
    /// </summary>
    public Guid RunId { get; }

    /// <summary>
    /// Gets the parent run identifier, when nested.
    /// </summary>
    public Guid? ParentRunId { get; }

    /// <summary>
    /// Gets the stable event name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the event timestamp.
    /// </summary>
    public DateTimeOffset OccurredAt { get; }

    /// <summary>
    /// Gets an immutable collection of non-sensitive event attributes.
    /// </summary>
    public IReadOnlyDictionary<string, string> Attributes { get; }
}
