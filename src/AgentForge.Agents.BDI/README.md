# AgentForge.Agents.BDI

A standalone Belief-Desire-Intention (BDI) agent. Each bounded run revises an explicit belief set from a new observation, filters feasible desires, keeps or reconsiders existing intentions through a commitment policy, adds intentions for eligible desires, and proposes an action.

## Installation

After publication to NuGet.org:

\`\`\`bash
dotnet add package AgentForge.Agents.BDI
\`\`\`

## Quick Start

\`\`\`csharp
using AgentForge.Agents.BDI.Builders;
using AgentForge.Agents.BDI.Models;

var agent = BdiAgentBuilder.Create<string, string, string, string, string>()
    .WithBeliefRevision((beliefs, observation) => beliefs
        .Where(belief => belief != "door-closed")
        .Append(observation)
        .Distinct()
        .ToArray())
    .WithDesireSelection((beliefs, desires) => beliefs.Contains("door-open")
        ? desires.Where(desire => desire == "close-door").ToArray()
        : Array.Empty<string>())
    .WithIntentionFactory(desire => desire)
    .WithCommitmentPolicy((beliefs, intention) =>
        intention == "stay-safe" || beliefs.Contains("door-open"))
    .WithActionSelection((beliefs, intentions) =>
        intentions.Contains("close-door")
            ? BdiAction<string>.From("close-door")
            : BdiAction<string>.None)
    .WithMaxIntentions(8)
    .Build();

var result = await agent.RunAsync(new BdiAgentInput<string, string, string, string>(
    Beliefs: ["door-closed"],
    AvailableDesires: ["close-door", "open-window"],
    CurrentIntentions: ["stay-safe"],
    Observation: "door-open"), cancellationToken);

Console.WriteLine(string.Join(", ", result.Output?.Beliefs ?? []));
Console.WriteLine(result.Output?.ProposedAction.Action);
\`\`\`

## BDI cycle

1. **Belief revision:** merge or revise the prior belief set using the new observation.
2. **Desire selection:** evaluate the available desires against the revised beliefs.
3. **Intention reconsideration:** retain existing intentions only when the commitment policy still approves them.
4. **Intention formation:** map eligible desires to intentions, avoiding duplicates and respecting the intention limit.
5. **Action selection:** propose an action from the revised beliefs and resulting intentions.

## State and safety boundaries

- Beliefs, desires, and current intentions are explicit input; the resulting values are explicit output.
- The module does not retain per-user mutable state. The host owns persistence and session lifecycle.
- Strategy delegates are application-supplied and should be deterministic, bounded, and side-effect free.
- The proposed action is not executed by the agent. The host remains responsible for authorization and external effects.
- The intention count is bounded by \`WithMaxIntentions\` (default: 16).
- Cancellation and strategy failures use the shared normalized run result.

## Fluent configuration

- \`WithBeliefRevision\` updates beliefs from observations.
- \`WithDesireSelection\` filters feasible desires.
- \`WithIntentionFactory\` maps a desire to an intention.
- \`WithCommitmentPolicy\` defines whether an existing intention remains committed.
- \`WithActionSelection\` proposes an action.
- \`WithMaxIntentions\` bounds the resulting intention set.
- \`Build()\` validates that all five strategies are supplied.

This module provides the BDI control cycle, not a built-in planner, ontology, logic engine, or persistent intention store.