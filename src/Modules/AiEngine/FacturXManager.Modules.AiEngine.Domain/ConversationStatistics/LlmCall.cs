using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.AiEngine.Domain.ConversationStatistics;

public sealed class LlmCall : AuditableEntity
{
    private LlmCall()
    { }

    public Guid Id { get; private set; }

    /// <summary>
    /// Référence vers la conversation parente.
    /// </summary>
    public Guid ConversationId { get; private set; }

    /// <summary>
    /// Nom de l'agent IA utilisé (ex: "PageBuilder", "ProductEnricher").
    /// </summary>
    public string? AgentName { get; private set; }

    /// <summary>
    /// Nom du modèle LLM utilisé (ex: "gpt-4o", "gpt-4o-mini").
    /// </summary>
    public string ModelName { get; private set; }

    /// <summary>
    /// Fournisseur du modèle (ex: "Azure OpenAI", "OpenAI", "Anthropic").
    /// </summary>
    public string? ModelProvider { get; private set; }

    /// <summary>
    /// Type d'opération effectuée (ex: "PageGeneration", "ProductEnrichment").
    /// </summary>
    public string? OperationType { get; private set; }

    /// <summary>
    /// Nombre de tokens en entrée.
    /// </summary>
    public int InputTokens { get; private set; }

    /// <summary>
    /// Nombre de tokens en entrée provenant du cache.
    /// </summary>
    public int InputCachedTokens { get; private set; }

    /// <summary>
    /// Nombre de tokens en sortie.
    /// </summary>
    public int OutputTokens { get; private set; }

    /// <summary>
    /// Nombre de tokens de raisonnement en sortie.
    /// </summary>
    public int OutputReasoningTokens { get; private set; }

    /// <summary>
    /// Nombre total de tokens consommés.
    /// </summary>
    public int TotalTokens { get; private set; }

    /// <summary>
    /// Temps de réponse en millisecondes.
    /// </summary>
    public long ResponseTimeMs { get; private set; }

    /// <summary>
    /// Indique si l'appel au LLM a réussi.
    /// </summary>
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Message d'erreur en cas d'échec.
    /// </summary>
    public string? ErrorMessage { get; private set; }

    /// <summary>
    /// Date et heure UTC de l'appel.
    /// </summary>
    public DateTime CalledAtUtc { get; private set; }

    public static LlmCall Create(
        Guid conversationId,
        string? agentName,
        string modelName,
        string? modelProvider,
        string? operationType,
        int inputTokens,
        int inputCachedTokens,
        int outputTokens,
        int outputReasoningTokens,
        int totalTokens,
        long responseTimeMs,
        bool isSuccess,
        string? errorMessage,
        DateTime calledAtUtc)
    {
        return new LlmCall
        {
            Id = Guid.NewGuid(),
            CreationDate = DateTime.UtcNow,
            LastUpdateDate = DateTime.UtcNow,
            ConversationId = conversationId,
            AgentName = agentName,
            ModelName = modelName,
            ModelProvider = modelProvider,
            OperationType = operationType,
            InputTokens = inputTokens,
            InputCachedTokens = inputCachedTokens,
            OutputTokens = outputTokens,
            OutputReasoningTokens = outputReasoningTokens,
            TotalTokens = totalTokens,
            ResponseTimeMs = responseTimeMs,
            IsSuccess = isSuccess,
            ErrorMessage = errorMessage,
            CalledAtUtc = calledAtUtc
        };
    }
}
