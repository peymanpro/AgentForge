namespace AgentForge.Models.Chat;

/// <summary>
/// Provides a normalized reason why a model response ended.
/// </summary>
public enum ModelFinishReason
{
    /// <summary>The model completed a response normally.</summary>
    Stop,

    /// <summary>The model proposed one or more tool calls.</summary>
    ToolCalls,

    /// <summary>The configured output limit was reached.</summary>
    Length,

    /// <summary>The provider reported a refusal or safety-related stop.</summary>
    Refusal,

    /// <summary>The provider reported another finish condition not represented above.</summary>
    Other
}
