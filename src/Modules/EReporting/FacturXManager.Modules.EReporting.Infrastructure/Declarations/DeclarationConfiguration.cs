using FacturXManager.Modules.EReporting.Domain.Declarations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacturXManager.Modules.EReporting.Infrastructure.Declarations;

internal sealed class DeclarationConfiguration : IEntityTypeConfiguration<Declaration>
{
    public void Configure(EntityTypeBuilder<Declaration> builder)
    {
        builder.ToTable("declarations");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.TransactionType).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(d => d.DgfipSubmissionReference).HasMaxLength(100);

        builder.OwnsOne(d => d.Period, period =>
        {
            period.Property(p => p.Year).HasColumnName("period_year");
            period.Property(p => p.Month).HasColumnName("period_month");
            period.Ignore(p => p.Label);
        });

        builder.HasMany(d => d.Entries)
            .WithOne()
            .HasForeignKey(e => e.DeclarationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata
            .FindNavigation(nameof(Declaration.Entries))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(d => d.TotalTaxableAmount);
        builder.Ignore(d => d.TotalVatAmount);
    }
}
