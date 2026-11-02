using AgentForge.Models.Chat;

namespace AgentForge.Models.Contracts;

/// <summary>
/// Provides the baseline provider-neutral chat generation operation.
/// </summary>
public interface IChatModel
{
    /// <summary>
    /// Generates one assistant response for the supplied conversation.
    /// </summary>
    /// <param name="request">The normalized model request.</param>
    /// <param name="cancellationToken">A token used to request cooperative cancellation.</param>
    /// <returns>A normalized response from the model provider.</returns>
    Task<ModelResponse> GenerateAsync(
        ModelRequest request,
        CancellationToken cancellationToken = default);
}
