using FacturXManager.Modules.Transmission.Domain.Transmissions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacturXManager.Modules.Transmission.Infrastructure.Transmissions;

internal sealed class TransmissionAttemptConfiguration : IEntityTypeConfiguration<TransmissionAttempt>
{
    public void Configure(EntityTypeBuilder<TransmissionAttempt> builder)
    {
        builder.ToTable("transmission_attempts");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.ChannelDepositId).HasMaxLength(100);
        builder.Property(a => a.RawResponse).HasMaxLength(4000);
        builder.Property(a => a.Message).HasMaxLength(2000);

        builder.HasIndex(a => a.TransmissionId);
    }
}
