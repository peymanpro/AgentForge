# AgentForge.Agents.RuleBased

A standalone rule-based production-system agent. It evaluates ordered condition/action rules against explicit working memory, applies the first applicable rule, updates memory, and repeats until no rule applies or a firing budget is reached.

## Installation

After publication to NuGet.org:

\`\`\`bash
dotnet add package AgentForge.Agents.RuleBased
\`\`\`

## Quick Start

\`\`\`csharp
using AgentForge.Agents.RuleBased.Builders;
using AgentForge.Agents.RuleBased.Models;

var agent = RuleBasedAgentBuilder.Create<Inventory, string>()
    .When(
        "reorder",
        inventory => inventory.Stock < inventory.ReorderPoint,
        inventory => $"Order {inventory.Sku}",
        inventory => inventory with { Stock = inventory.Stock + inventory.ReorderQuantity })
    .When(
        "mark-healthy",
        inventory => inventory.Stock >= inventory.ReorderPoint,
        _ => "Stock is healthy",
        inventory => inventory)
    .WithMaxFirings(8)
    .Build();

var result = await agent.RunAsync(new RuleBasedAgentInput<Inventory>(
    new Inventory("SKU-42", 3, 5, 10)), cancellationToken);

Console.WriteLine(result.Output?.FinalWorkingMemory);
Console.WriteLine(string.Join(", ", result.Output?.FiredRules ?? []));

public sealed record Inventory(string Sku, int Stock, int ReorderPoint, int ReorderQuantity);
\`\`\`

## Execution semantics

- Rules are evaluated in registration order; the first applicable rule wins.
- The selected rule creates an action and transforms working memory.
- Evaluation restarts from the first rule after every firing.
- The cycle ends successfully when no rule applies.
- \`WithMaxFirings\` and the shared \`AgentRunOptions.MaxSteps\` bound execution.
- Hitting a firing limit while another rule remains applicable returns \`BudgetExhausted\` and preserves the partial trace.
- The input memory is not mutated by the framework; use immutable state or copy-on-write transformations.

## Boundaries

This module implements a forward-chaining production-system control loop. It does not provide a general-purpose logic programming engine, backward chaining, probabilistic rules, external side-effect execution, or durable working-memory storage. Rule predicates and transformations are application code and should be deterministic and side-effect free.

## Fluent configuration

- \`When(name, condition, actionFactory, apply)\` registers one ordered production rule.
- \`WithMaxFirings(value)\` sets the per-run firing ceiling (default: 32).
- \`WithTimeProvider(provider)\` configures run timestamps.
- \`Build()\` validates that at least one rule exists and returns the common \`AgentHandle\`.

The host owns working-memory persistence, action authorization, and external effects.