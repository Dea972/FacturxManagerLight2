using FacturXManager.Common.Infrastructure.Inbox;
using FacturXManager.Modules.AiEngine.Application.Abstractions.Data;
using FacturXManager.Modules.AiEngine.Domain.ConversationStatistics;
using FacturXManager.Modules.AiEngine.Infrastructure.ConversationStatistics;
using Microsoft.EntityFrameworkCore;

namespace FacturXManager.Modules.AiEngine.Infrastructure.Database;

public sealed class AiEngineDbContext(DbContextOptions<AiEngineDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<ConversationStatistic> ConversationStatistics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.AiEngine);

        modelBuilder.ApplyConfiguration(new ConversationStatisticConfiguration());
        modelBuilder.ApplyConfiguration(new LlmCallConfiguration());

        modelBuilder.ApplyConfiguration(new InboxMessageConfiguration());
        modelBuilder.ApplyConfiguration(new InboxMessageConsumerConfiguration());
    }
}
