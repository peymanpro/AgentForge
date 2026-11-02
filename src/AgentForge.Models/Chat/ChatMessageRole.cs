namespace AgentForge.Models.Chat;

/// <summary>
/// Identifies the semantic role of a message in a model conversation.
/// </summary>
public enum ChatMessageRole
{
    /// <summary>Application-level system instructions.</summary>
    System,

    /// <summary>Application or developer instructions with priority below system instructions.</summary>
    Developer,

    /// <summary>Input supplied by the user or calling application.</summary>
    User,

    /// <summary>Content or tool-call proposals produced by the assistant model.</summary>
    Assistant,

    /// <summary>Output returned by a tool after execution.</summary>
    Tool
}
