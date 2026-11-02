namespace AgentForge.Agents.Classical;

/// <summary>
/// Defines one ordered condition-action rule used by a reflex agent.
/// </summary>
/// <typeparam name="TState">The percept or internal-state type inspected by the rule.</typeparam>
/// <typeparam name="TAction">The action type produced by the rule.</typeparam>
public sealed class ReflexRule<TState, TAction>
    where TState : notnull
    where TAction : notnull
{
    private readonly Func<TState, bool> _condition;
    private readonly Func<TState, TAction> _actionFactory;

    /// <summary>
    /// Creates a condition-action rule.
    /// </summary>
    /// <param name="name">A stable, non-empty rule name.</param>
    /// <param name="condition">Returns whether the rule applies to the supplied state.</param>
    /// <param name="actionFactory">Creates the action when the rule applies.</param>
    public ReflexRule(
        string name,
        Func<TState, bool> condition,
        Func<TState, TAction> actionFactory)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("A rule name is required.", nameof(name));
        }

        ArgumentNullException.ThrowIfNull(condition);
        ArgumentNullException.ThrowIfNull(actionFactory);

        Name = name;
        _condition = condition;
        _actionFactory = actionFactory;
    }

    /// <summary>
    /// Gets the stable name of this rule.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Evaluates the rule condition.
    /// </summary>
    /// <param name="state">The percept or state to inspect.</param>
    /// <returns><see langword="true"/> when the rule applies.</returns>
    public bool IsApplicable(TState state) => _condition(state);

    /// <summary>
    /// Creates the action associated with this rule.
    /// </summary>
    /// <param name="state">The percept or state used to create the action.</param>
    /// <returns>The proposed action.</returns>
    public TAction CreateAction(TState state) => _actionFactory(state);
}
