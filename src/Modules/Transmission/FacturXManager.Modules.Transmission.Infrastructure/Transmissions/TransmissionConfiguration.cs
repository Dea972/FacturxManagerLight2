using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using DomainTransmission = FacturXManager.Modules.Transmission.Domain.Transmissions.Transmission;

namespace FacturXManager.Modules.Transmission.Infrastructure.Transmissions;

internal sealed class TransmissionConfiguration : IEntityTypeConfiguration<DomainTransmission>
{
    public void Configure(EntityTypeBuilder<DomainTransmission> builder)
    {
        builder.ToTable("transmissions");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.DocumentReference).HasMaxLength(200);
        builder.Property(t => t.RecipientSiret).HasMaxLength(14);
        builder.Property(t => t.ExecutingService).HasMaxLength(100);
        builder.Property(t => t.ChannelDepositId).HasMaxLength(100);
        builder.Property(t => t.RejectionReason).HasMaxLength(2000);

        builder.Property(t => t.Channel).HasConversion<string>().HasMaxLength(20);
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(t => t.DocumentReference);

        builder.HasMany(t => t.Attempts)
            .WithOne()
            .HasForeignKey(a => a.TransmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(DomainTransmission.Attempts))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
