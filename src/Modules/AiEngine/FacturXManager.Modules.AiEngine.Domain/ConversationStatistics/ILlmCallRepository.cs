namespace FacturXManager.Modules.AiEngine.Domain.ConversationStatistics;

public interface ILlmCallRepository
{
    void Insert(LlmCall llmCall);

    Task<List<LlmCall>> GetByConversationIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);

    Task<List<LlmCall>> GetByModelNameAsync(
        string modelName,
        CancellationToken cancellationToken = default);

    Task<List<LlmCall>> GetByDateRangeAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default);
}
