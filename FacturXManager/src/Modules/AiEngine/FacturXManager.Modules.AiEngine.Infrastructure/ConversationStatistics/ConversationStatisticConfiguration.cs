using FacturXManager.Modules.AiEngine.Domain.ConversationStatistics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacturXManager.Modules.AiEngine.Infrastructure.ConversationStatistics;

internal sealed class ConversationStatisticConfiguration : IEntityTypeConfiguration<ConversationStatistic>
{
    public void Configure(EntityTypeBuilder<ConversationStatistic> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ConversationId)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(c => c.ConversationId)
            .IsUnique();

        builder.Property(c => c.UserId)
            .HasMaxLength(100);

        builder.Property(c => c.SourceModule)
            .HasMaxLength(100);

        builder.HasMany(c => c.LlmCalls)
            .WithOne()
            .HasForeignKey(c => c.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
