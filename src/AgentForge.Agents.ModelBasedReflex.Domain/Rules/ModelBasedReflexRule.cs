namespace AgentForge.Agents.ModelBasedReflex.Domain.Rules;

/// <summary>
/// Defines an ordered condition-action rule evaluated against an updated internal state.
/// </summary>
/// <typeparam name="TState">The internal state type.</typeparam>
/// <typeparam name="TAction">The proposed action type.</typeparam>
public sealed class ModelBasedReflexRule<TState, TAction>
    where TState : notnull
    where TAction : notnull
{
    private readonly Func<TState, bool> _condition;
    private readonly Func<TState, TAction> _actionFactory;

    /// <summary>
    /// Creates a condition-action rule for an internal state.
    /// </summary>
    public ModelBasedReflexRule(
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
    /// Gets the stable rule name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Evaluates this rule against the updated internal state.
    /// </summary>
    public bool Matches(TState state) => _condition(state);

    /// <summary>
    /// Creates the proposed action for a matching state.
    /// </summary>
    public TAction CreateAction(TState state) => _actionFactory(state);
}
