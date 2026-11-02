# AgentForge.Tools

The tool declaration, validation, registration, authorization, and execution library for AgentForge .NET.

## Execution order

1. Resolve the exact tool name in an immutable ToolRegistry.
2. Validate the untrusted JSON arguments against the tool's declared contract.
3. Ask the application-owned IToolAuthorizationPolicy for a decision.
4. Execute only after an explicit Allowed decision.
5. Return a normalized ToolExecutionResult.

Denied and ApprovalRequired decisions never invoke the tool implementation.

## Security boundary

A model-generated tool call is data. It cannot grant itself permission. The application chooses which tools enter a ToolRegistry and provides the authorization policy that applies to each invocation.

Validation, authorization, provider/runtime failure, and execution failure have distinct result codes. Sensitive argument and output values are not emitted in default execution events.

## Deliberate limits

The initial executor is asynchronous and cancellation-aware. It does not silently retry, parallelize side effects, or claim idempotency. Applications must design their own approval handling, idempotency, and side-effect policy.
