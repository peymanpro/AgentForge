namespace AgentForge.Agents.SimpleReflex.Domain.Rules;

/// <summary>
/// Defines an ordered condition-action rule for a simple-reflex agent.
/// </summary>
/// <typeparam name="TPercept">The current percept type.</typeparam>
/// <typeparam name="TAction">The action proposed when the rule matches.</typeparam>
public sealed class SimpleReflexRule<TPercept, TAction>
    where TPercept : notnull
    where TAction : notnull
{
    private readonly Func<TPercept, bool> _condition;
    private readonly Func<TPercept, TAction> _actionFactory;

    /// <summary>
    /// Creates a condition-action rule.
    /// </summary>
    /// <param name="name">A unique, non-empty rule name within the agent.</param>
    /// <param name="condition">Determines whether the rule matches the current percept.</param>
    /// <param name="actionFactory">Creates the proposed action when the rule matches.</param>
    public SimpleReflexRule(
        string name,
        Func<TPercept, bool> condition,
        Func<TPercept, TAction> actionFactory)
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
    /// Gets the stable rule name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Evaluates this rule against a percept.
    /// </summary>
    public bool Matches(TPercept percept) => _condition(percept);

    /// <summary>
    /// Creates the proposed action for a matching percept.
    /// </summary>
    public TAction CreateAction(TPercept percept) => _actionFactory(percept);
}
