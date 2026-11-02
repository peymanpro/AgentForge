namespace AgentForge.Agents.Planning.Execution;

/// <summary>
/// Provides an immutable registry of explicitly permitted plan step handlers.
/// </summary>
public sealed class PlanStepHandlerRegistry
{
    private readonly IReadOnlyDictionary<string, IPlanStepHandler> _handlers;
    private readonly IReadOnlyList<string> _stepTypes;

    /// <summary>
    /// Creates a handler registry. Duplicate handler types are rejected.
    /// </summary>
    public PlanStepHandlerRegistry(IEnumerable<IPlanStepHandler> handlers)
    {
        ArgumentNullException.ThrowIfNull(handlers);

        var map = new Dictionary<string, IPlanStepHandler>(StringComparer.Ordinal);
        foreach (var handler in handlers)
        {
            ArgumentNullException.ThrowIfNull(handler);
            var stepType = handler.StepType;

            if (string.IsNullOrWhiteSpace(stepType))
            {
                throw new ArgumentException("A handler must expose a non-empty step type.", nameof(handlers));
            }

            if (!map.TryAdd(stepType, handler))
            {
                throw new ArgumentException($"A handler for step type '{stepType}' is already registered.", nameof(handlers));
            }
        }

        _handlers = new System.Collections.ObjectModel.ReadOnlyDictionary<string, IPlanStepHandler>(map);
        _stepTypes = Array.AsReadOnly(map.Keys.Order(StringComparer.Ordinal).ToArray());
    }

    /// <summary>Gets the registered step types in deterministic ordinal order.</summary>
    public IReadOnlyList<string> StepTypes => _stepTypes;

    /// <summary>Attempts to resolve a handler by its exact, case-sensitive step type.</summary>
    public bool TryGet(string stepType, out IPlanStepHandler? handler)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stepType);
        return _handlers.TryGetValue(stepType, out handler);
    }
}
