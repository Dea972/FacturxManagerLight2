namespace FacturXManager.Common.Application.AI;

/// <summary>
/// Configuration options for AI services.
/// Bind this to a configuration section (e.g., "AI") in appsettings.json.
/// </summary>
public sealed class AIServiceOptions
{
    /// <summary>
    /// The AI provider to use (e.g., "AzureOpenAI", "OpenAI", "Anthropic").
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// The API key for authentication with the AI provider.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// The endpoint URL for the AI provider (required for Azure OpenAI).
    /// </summary>
    public string? Endpoint { get; set; }

    /// <summary>
    /// The model identifier to use for chat completions.
    /// </summary>
    public string ChatModel { get; set; } = string.Empty;

    /// <summary>
    /// The model identifier to use for embeddings.
    /// </summary>
    public string? EmbeddingModel { get; set; }

    /// <summary>
    /// Maximum number of tokens for the AI response.
    /// </summary>
    public int MaxTokens { get; set; } = 4096;
}
