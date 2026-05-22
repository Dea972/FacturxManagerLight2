using FacturXManager.Modules.AiEngine.Domain.ConversationStatistics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacturXManager.Modules.AiEngine.Infrastructure.ConversationStatistics;

internal sealed class LlmCallConfiguration : IEntityTypeConfiguration<LlmCall>
{
    public void Configure(EntityTypeBuilder<LlmCall> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.AgentName)
            .HasMaxLength(100);

        builder.Property(c => c.ModelName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(c => c.ModelProvider)
             .HasMaxLength(100);

        builder.Property(c => c.OperationType)
            .HasMaxLength(100);
    }
}
