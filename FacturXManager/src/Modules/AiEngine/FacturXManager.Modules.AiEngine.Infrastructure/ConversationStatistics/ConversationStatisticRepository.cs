using FacturXManager.Modules.AiEngine.Domain.ConversationStatistics;
using FacturXManager.Modules.AiEngine.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace FacturXManager.Modules.AiEngine.Infrastructure.ConversationStatistics;

internal sealed class ConversationStatisticRepository(AiEngineDbContext context) : IConversationStatisticRepository
{
    public void Insert(ConversationStatistic conversation)
    {
        context.ConversationStatistics.Add(conversation);
    }

    public async Task<ConversationStatistic?> GetByConversationIdAsync(
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        return await context.ConversationStatistics
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId, cancellationToken);
    }

    public async Task<ConversationStatistic?> GetByConversationIdWithCallsAsync(
        string conversationId,
        CancellationToken cancellationToken = default)
    {
        return await context.ConversationStatistics
            .Include(c => c.LlmCalls)
            .FirstOrDefaultAsync(c => c.ConversationId == conversationId, cancellationToken);
    }

    public async Task<List<ConversationStatistic>> GetByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        return await context.ConversationStatistics
            .Where(c => c.UserId == userId)
            .ToListAsync(cancellationToken);
    }
}
