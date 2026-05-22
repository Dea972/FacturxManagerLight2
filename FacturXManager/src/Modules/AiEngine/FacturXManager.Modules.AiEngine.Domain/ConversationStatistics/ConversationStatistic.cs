using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.AiEngine.Domain.ConversationStatistics;

public sealed class ConversationStatistic : AuditableEntity
{
    private readonly List<LlmCall> _llmCalls = [];

    private ConversationStatistic()
    { }

    public Guid Id { get; private set; }

    /// <summary>
    /// Identifiant externe de la conversation (ex: sessionId Azure Foundry).
    /// </summary>
    public string ConversationId { get; private set; }

    /// <summary>
    /// Identifiant de l'utilisateur ayant initié la conversation.
    /// </summary>
    public string? UserId { get; private set; }

    /// <summary>
    /// Module source ayant initié la conversation (ex: "ContentGenerator", "Catalog").
    /// </summary>
    public string? SourceModule { get; private set; }

    /// <summary>
    /// Nombre total d'appels LLM dans cette conversation.
    /// </summary>
    public int TotalCalls { get; private set; }

    /// <summary>
    /// Nombre total de tokens d'entrée consommés dans cette conversation.
    /// </summary>
    public int TotalInputTokens { get; private set; }

    /// <summary>
    /// Nombre total de tokens d'entrée provenant du cache.
    /// </summary>
    public int TotalInputCachedTokens { get; private set; }

    /// <summary>
    /// Nombre total de tokens de sortie.
    /// </summary>
    public int TotalOutputTokens { get; private set; }

    /// <summary>
    /// Nombre total de tokens de raisonnement.
    /// </summary>
    public int TotalOutputReasoningTokens { get; private set; }

    /// <summary>
    /// Nombre total de tokens consommés dans cette conversation.
    /// </summary>
    public int TotalTokens { get; private set; }

    /// <summary>
    /// Temps de réponse cumulé en millisecondes.
    /// </summary>
    public long TotalResponseTimeMs { get; private set; }

    /// <summary>
    /// Nombre d'appels ayant échoué.
    /// </summary>
    public int FailedCalls { get; private set; }

    public IReadOnlyCollection<LlmCall> LlmCalls => _llmCalls;

    public static ConversationStatistic Create(
        string conversationId,
        string? userId,
        string? sourceModule)
    {
        return new ConversationStatistic
        {
            Id = Guid.NewGuid(),
            CreationDate = DateTime.UtcNow,
            LastUpdateDate = DateTime.UtcNow,
            ConversationId = conversationId,
            UserId = userId,
            SourceModule = sourceModule,
            TotalCalls = 0,
            TotalInputTokens = 0,
            TotalInputCachedTokens = 0,
            TotalOutputTokens = 0,
            TotalOutputReasoningTokens = 0,
            TotalTokens = 0,
            TotalResponseTimeMs = 0,
            FailedCalls = 0
        };
    }

    /// <summary>
    /// Enregistre un appel LLM et met à jour les compteurs agrégés.
    /// </summary>
    public void AddLlmCall(LlmCall llmCall)
    {
        _llmCalls.Add(llmCall);

        TotalCalls++;
        TotalInputTokens += llmCall.InputTokens;
        TotalInputCachedTokens += llmCall.InputCachedTokens;
        TotalOutputTokens += llmCall.OutputTokens;
        TotalOutputReasoningTokens += llmCall.OutputReasoningTokens;
        TotalTokens += llmCall.TotalTokens;
        TotalResponseTimeMs += llmCall.ResponseTimeMs;

        if (!llmCall.IsSuccess)
        {
            FailedCalls++;
        }

        LastUpdateDate = DateTime.UtcNow;
    }
}
