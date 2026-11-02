namespace AgentForge.Tools;

/// <summary>
/// Defines an application-controlled authorization boundary for tool invocations.
/// </summary>
public interface IToolAuthorizationPolicy
{
    /// <summary>
    /// Determines whether a proposed tool invocation is authorized.
    /// </summary>
    /// <param name="request">The tool and validated arguments being requested.</param>
    /// <param name="cancellationToken">A token used to request cooperative cancellation.</param>
    /// <returns>An explicit authorization decision.</returns>
    Task<ToolAuthorizationDecision> AuthorizeAsync(
        ToolAuthorizationRequest request,
        CancellationToken cancellationToken = default);
}
