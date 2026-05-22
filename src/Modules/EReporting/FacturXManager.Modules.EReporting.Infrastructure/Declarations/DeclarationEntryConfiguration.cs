using FacturXManager.Modules.EReporting.Domain.Declarations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacturXManager.Modules.EReporting.Infrastructure.Declarations;

internal sealed class DeclarationEntryConfiguration : IEntityTypeConfiguration<DeclarationEntry>
{
    public void Configure(EntityTypeBuilder<DeclarationEntry> builder)
    {
        builder.ToTable("declaration_entries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.TaxableAmount).HasPrecision(18, 2);
        builder.Property(e => e.VatAmount).HasPrecision(18, 2);
        builder.Property(e => e.CountryCode).HasMaxLength(2);
        builder.Property(e => e.OperationNature).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(e => e.DeclarationId);
    }
}
