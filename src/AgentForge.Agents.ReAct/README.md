# AgentForge.Agents.ReAct

An independently implemented ReAct loop for AgentForge .NET.

## Algorithm

The agent alternates between a model decision and an observation:

1. Build a normalized request from the current conversation messages.
2. Ask the injected IReactDecisionStrategy for a ModelResponse.
3. If the response is final, return the final text and execution history.
4. If the response contains tool-call proposals, resolve each proposal through ToolExecutor.
5. Append the normalized tool observation to the conversation.
6. Continue until the model returns a final answer or a configured limit is reached.

## Safety and limits

Every proposed tool call passes through ToolRegistry validation and the application authorization policy. The agent never treats a model proposal as permission.

The run is bounded by model-turn, tool-invocation, step, tool-observation-size, and timeout limits. Cancellation is propagated. Model and tool exceptions are converted into safe normalized failures.

## Extension point

IReactDecisionStrategy is injected. An application can implement it using its provider of choice. AgentForge does not bundle a vendor-specific client.

## Result

ReActAgentOutput records the final response, finish reason, model turns, tool invocation attempts, tool execution outcomes, and any pending proposals when execution stops.
