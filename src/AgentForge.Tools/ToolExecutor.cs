using AgentForge.Abstractions.Execution;

namespace AgentForge.Tools;

/// <summary>
/// Validates, authorizes, and executes tool requests in a defined order.
/// </summary>
public sealed class ToolExecutor
{
    private readonly ToolRegistry _registry;
    private readonly IToolAuthorizationPolicy _authorizationPolicy;

    /// <summary>
    /// Creates a tool executor for an explicitly scoped registry and authorization policy.
    /// </summary>
    /// <param name="registry">The immutable registry of tools available to this executor.</param>
    /// <param name="authorizationPolicy">The application-controlled authorization policy.</param>
    public ToolExecutor(ToolRegistry registry, IToolAuthorizationPolicy authorizationPolicy)
    {
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(authorizationPolicy);

        _registry = registry;
        _authorizationPolicy = authorizationPolicy;
    }

    /// <summary>
    /// Executes a model-proposed tool request after validation and authorization.
    /// </summary>
    /// <param name="request">The untrusted tool request.</param>
    /// <param name="cancellationToken">A token used to request cooperative cancellation.</param>
    /// <returns>A normalized tool execution result.</returns>
    public async Task<ToolExecutionResult> ExecuteAsync(
        ToolExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (!_registry.TryGet(request.ToolName, out var tool) || tool is null)
        {
            return ToolExecutionResult.Unsuccessful(
                request.ToolCallId,
                ToolExecutionStatus.NotFound,
                new AgentFailure(
                    AgentFailureCategory.UnsupportedCapability,
                    "tool.not_found",
                    "The requested tool is not available in this execution scope."));
        }

        ToolValidationResult validation;
        try
        {
            validation = tool.Validate(request.Arguments);
        }
        catch (Exception)
        {
            return ToolExecutionResult.Unsuccessful(
                request.ToolCallId,
                ToolExecutionStatus.Failed,
                new AgentFailure(
                    AgentFailureCategory.ValidationFailure,
                    "tool.validation_failed",
                    "The tool argument validator failed."));
        }

        if (validation is null)
        {
            return ToolExecutionResult.Unsuccessful(
                request.ToolCallId,
                ToolExecutionStatus.Failed,
                new AgentFailure(
                    AgentFailureCategory.ValidationFailure,
                    "tool.validation_failed",
                    "The tool argument validator returned no result."));
        }

        if (!validation.IsValid)
        {
            return ToolExecutionResult.Unsuccessful(
                request.ToolCallId,
                ToolExecutionStatus.InvalidArguments,
                new AgentFailure(
                    AgentFailureCategory.ValidationFailure,
                    "tool.invalid_arguments",
                    "The tool arguments did not pass validation."));
        }

        ToolAuthorizationDecision authorization;
        try
        {
            authorization = await _authorizationPolicy.AuthorizeAsync(
                new ToolAuthorizationRequest(tool.Definition, request.Arguments, request.RunContext),
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return ToolExecutionResult.Unsuccessful(
                request.ToolCallId,
                ToolExecutionStatus.Failed,
                new AgentFailure(
                    AgentFailureCategory.AuthorizationFailed,
                    "tool.authorization_failed",
                    "The tool authorization policy failed."));
        }

        switch (authorization)
        {
            case ToolAuthorizationDecision.Denied:
                return ToolExecutionResult.Unsuccessful(
                    request.ToolCallId,
                    ToolExecutionStatus.Denied,
                    new AgentFailure(
                        AgentFailureCategory.PolicyDenied,
                        "tool.denied",
                        "The application policy denied this tool invocation."));

            case ToolAuthorizationDecision.ApprovalRequired:
                return ToolExecutionResult.Unsuccessful(
                    request.ToolCallId,
                    ToolExecutionStatus.ApprovalRequired,
                    new AgentFailure(
                        AgentFailureCategory.PolicyDenied,
                        "tool.approval_required",
                        "This tool invocation requires an approval handler before it can execute."));

            case ToolAuthorizationDecision.Allowed:
                break;

            default:
                return ToolExecutionResult.Unsuccessful(
                    request.ToolCallId,
                    ToolExecutionStatus.Failed,
                    new AgentFailure(
                        AgentFailureCategory.AuthorizationFailed,
                        "tool.invalid_authorization_decision",
                        "The authorization policy returned an unsupported decision."));
        }

        try
        {
            var output = await tool.ExecuteAsync(
                request.Arguments,
                request.RunContext,
                cancellationToken).ConfigureAwait(false);

            if (output.ValueKind == System.Text.Json.JsonValueKind.Undefined)
            {
                return ToolExecutionResult.Unsuccessful(
                    request.ToolCallId,
                    ToolExecutionStatus.Failed,
                    new AgentFailure(
                        AgentFailureCategory.ToolFailure,
                        "tool.invalid_output",
                        "The tool returned an undefined JSON value."));
            }

            return ToolExecutionResult.Succeeded(request.ToolCallId, output);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            return ToolExecutionResult.Unsuccessful(
                request.ToolCallId,
                ToolExecutionStatus.Failed,
                new AgentFailure(
                    AgentFailureCategory.ToolFailure,
                    "tool.execution_failed",
                    "The tool implementation failed."));
        }
    }
}
