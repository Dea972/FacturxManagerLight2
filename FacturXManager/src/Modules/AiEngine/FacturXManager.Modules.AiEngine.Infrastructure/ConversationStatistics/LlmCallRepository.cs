using FacturXManager.Modules.AiEngine.Domain.ConversationStatistics;
using FacturXManager.Modules.AiEngine.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace FacturXManager.Modules.AiEngine.Infrastructure.ConversationStatistics;

internal sealed class LlmCallRepository(AiEngineDbContext context) : ILlmCallRepository
{
    public void Insert(LlmCall llmCall)
    {
        context.Set<LlmCall>().Add(llmCall);
    }

    public async Task<List<LlmCall>> GetByConversationIdAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<LlmCall>()
            .Where(c => c.ConversationId == conversationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LlmCall>> GetByModelNameAsync(
        string modelName,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<LlmCall>()
            .Where(c => c.ModelName == modelName)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LlmCall>> GetByDateRangeAsync(
        DateTime fromUtc,
        DateTime toUtc,
        CancellationToken cancellationToken = default)
    {
        return await context.Set<LlmCall>()
            .Where(c => c.CalledAtUtc >= fromUtc && c.CalledAtUtc <= toUtc)
            .ToListAsync(cancellationToken);
    }
}
