using FacturXManager.Common.Application.EventBus;

namespace FacturXManager.Common.Application.AI;

/// <summary>
/// Événement d'intégration publié automatiquement à chaque appel LLM via <see cref="LlmServiceBase"/>.
/// Consommé par le module AiEngine pour centraliser les statistiques d'usage.
/// </summary>
public sealed class LlmCalledIntegrationEvent : IntegrationEvent
{
    public LlmCalledIntegrationEvent(
        Guid id,
        string sessionId,
        string? userId,
        string? agentName,
        string modelName,
        string? modelProvider,
        int inputTokens,
        int inputCachedTokens,
        int outputTokens,
        int outputReasoningTokens,
        int totalTokens,
        long responseTimeMs,
        string? sourceModule,
        string? operationType,
        bool isSuccess,
        string? errorMessage,
        DateTime occurredOnUtc) : base(id, occurredOnUtc)
    {
        SessionId = sessionId;
        UserId = userId;
        AgentName = agentName;
        ModelName = modelName;
        ModelProvider = modelProvider;
        InputTokens = inputTokens;
        InputCachedTokens = inputCachedTokens;
        OutputTokens = outputTokens;
        OutputReasoningTokens = outputReasoningTokens;
        TotalTokens = totalTokens;
        ResponseTimeMs = responseTimeMs;
        SourceModule = sourceModule;
        OperationType = operationType;
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Identifiant de la session/conversation.
    /// </summary>
    public string SessionId { get; }

    /// <summary>
    /// Identifiant de l'utilisateur ayant initié l'appel.
    /// </summary>
    public string? UserId { get; }

    /// <summary>
    /// Nom de l'agent IA utilisé (ex: "PageBuilder", "ProductEnricher").
    /// </summary>
    public string? AgentName { get; }

    /// <summary>
    /// Nom du modèle LLM utilisé (ex: "gpt-4o", "gpt-4o-mini").
    /// </summary>
    public string ModelName { get; }

    /// <summary>
    /// Fournisseur du modèle (ex: "Azure OpenAI", "OpenAI", "Anthropic").
    /// </summary>
    public string? ModelProvider { get; }

    /// <summary>
    /// Nombre de tokens en entrée.
    /// </summary>
    public int InputTokens { get; }

    /// <summary>
    /// Nombre de tokens en entrée provenant du cache.
    /// </summary>
    public int InputCachedTokens { get; }

    /// <summary>
    /// Nombre de tokens en sortie.
    /// </summary>
    public int OutputTokens { get; }

    /// <summary>
    /// Nombre de tokens de raisonnement en sortie.
    /// </summary>
    public int OutputReasoningTokens { get; }

    /// <summary>
    /// Nombre total de tokens consommés.
    /// </summary>
    public int TotalTokens { get; }

    /// <summary>
    /// Temps de réponse en millisecondes.
    /// </summary>
    public long ResponseTimeMs { get; }

    /// <summary>
    /// Module source ayant initié l'appel (ex: "ContentGenerator", "Catalog").
    /// </summary>
    public string? SourceModule { get; }

    /// <summary>
    /// Type d'opération effectuée (ex: "PageGeneration", "ProductEnrichment").
    /// </summary>
    public string? OperationType { get; }

    /// <summary>
    /// Indique si l'appel au LLM a réussi.
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Message d'erreur en cas d'échec.
    /// </summary>
    public string? ErrorMessage { get; }
}
