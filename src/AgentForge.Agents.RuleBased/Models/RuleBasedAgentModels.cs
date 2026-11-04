namespace AgentForge.Agents.RuleBased.Models;

/// <summary>Supplies the working memory for one production-system run.</summary>
public sealed record RuleBasedAgentInput<TMemory>(TMemory WorkingMemory)
    where TMemory : notnull;

/// <summary>Describes why the production-system cycle stopped.</summary>
public enum RuleBasedTerminationReason
{
    /// <summary>No rule remains applicable to the current working memory.</summary>
    RulesExhausted,

    /// <summary>The configured firing limit was reached while another rule remained applicable.</summary>
    MaxFiringsReached
}

/// <summary>Contains the resulting working memory and the rule-firing trace.</summary>
public sealed record RuleBasedAgentOutput<TMemory, TAction>(
    TMemory FinalWorkingMemory,
    IReadOnlyList<string> FiredRules,
    IReadOnlyList<TAction> Actions,
    RuleBasedTerminationReason TerminationReason)
    where TMemory : notnull
    where TAction : notnull;