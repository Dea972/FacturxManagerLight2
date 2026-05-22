namespace FacturXManager.Modules.AiEngine.Domain.ConversationStatistics;

public interface IConversationStatisticRepository
{
    void Insert(ConversationStatistic conversation);

    Task<ConversationStatistic?> GetByConversationIdAsync(
        string conversationId,
        CancellationToken cancellationToken = default);

    Task<ConversationStatistic?> GetByConversationIdWithCallsAsync(
        string conversationId,
        CancellationToken cancellationToken = default);

    Task<List<ConversationStatistic>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default);
}
