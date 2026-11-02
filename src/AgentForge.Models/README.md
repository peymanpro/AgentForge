# AgentForge.Models

Provider-neutral chat request and response contracts for AgentForge .NET.

## Responsibilities

- ChatMessage represents normalized system, user, assistant, and tool-result messages.
- ModelRequest preserves ordered conversation messages and generation options.
- ModelResponse represents assistant output, finish reason, optional tool-call proposals, and usage metadata.
- IChatModel defines the baseline generation operation.
- IToolCallingChatModel is an optional capability for providers that support native tool calling.

## Bring your own model

Applications implement IChatModel using their chosen provider SDK or HTTP API. An agent consumes the normalized contract and does not depend on a vendor SDK.

A provider that supports native tool calling may implement IToolCallingChatModel. The optional capability is not required by every model.

## Trust boundary

A normalized tool call is a proposal, not permission to execute. The model contract does not execute tools. AgentForge.Tools and the application authorization policy control validation and execution.

No provider adapter is bundled in this experimental foundation.
