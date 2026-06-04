using FacturXManager.Modules.Compliance.Domain.Common;
using FacturXManager.Modules.Compliance.Domain.CreditNotes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacturXManager.Modules.Compliance.Infrastructure.CreditNotes;

internal sealed class CreditNoteConfiguration : IEntityTypeConfiguration<CreditNote>
{
    public void Configure(EntityTypeBuilder<CreditNote> builder)
    {
        builder.ToTable("credit_notes");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Number).HasMaxLength(200);
        builder.Property(c => c.CurrencyCode).HasMaxLength(3);
        builder.Property(c => c.CreditReason).HasMaxLength(500);
        builder.Property(c => c.BuyerReference).HasMaxLength(200);
        builder.Property(c => c.PaymentMeansCode).HasMaxLength(10);

        builder.Property(c => c.Profile).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.OperationNature).HasConversion<string>().HasMaxLength(30);

        builder.HasIndex(c => c.Number).IsUnique();

        ConfigureParty(builder, c => c.Seller, "seller");
        ConfigureParty(builder, c => c.Buyer, "buyer");

        builder.OwnsOne(c => c.OriginalInvoice, original =>
        {
            original.Property(o => o.Number).HasColumnName("original_invoice_number").HasMaxLength(200);
            original.Property(o => o.IssueDate).HasColumnName("original_invoice_issue_date");
        });
        builder.Navigation(c => c.OriginalInvoice).IsRequired();

        builder.OwnsMany(c => c.Lines, ConfigureCreditNoteLines);
        builder.Metadata
            .FindNavigation(nameof(CreditNote.Lines))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(c => c.VatBreakdowns, ConfigureVatBreakdowns);
        builder.Metadata
            .FindNavigation(nameof(CreditNote.VatBreakdowns))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureParty(
        EntityTypeBuilder<CreditNote> builder,
        System.Linq.Expressions.Expression<System.Func<CreditNote, Party?>> navigation,
        string prefix)
    {
        builder.OwnsOne(navigation, party =>
        {
            party.Property(p => p.Name).HasColumnName($"{prefix}_name").HasMaxLength(200);
            party.Property(p => p.LegalRegistrationId).HasColumnName($"{prefix}_legal_registration_id").HasMaxLength(20);
            party.Property(p => p.LegalOrganizationId).HasColumnName($"{prefix}_legal_organization_id").HasMaxLength(20);
            party.Property(p => p.VatIdentifier).HasColumnName($"{prefix}_vat_identifier").HasMaxLength(20);
            party.Property(p => p.ElectronicAddress).HasColumnName($"{prefix}_electronic_address").HasMaxLength(200);
            party.Property(p => p.ElectronicAddressScheme).HasColumnName($"{prefix}_electronic_address_scheme").HasMaxLength(10);

            party.OwnsOne(p => p.Address, address =>
            {
                address.Property(a => a.Line1).HasColumnName($"{prefix}_address_line1").HasMaxLength(200);
                address.Property(a => a.Line2).HasColumnName($"{prefix}_address_line2").HasMaxLength(200);
                address.Property(a => a.PostalCode).HasColumnName($"{prefix}_postal_code").HasMaxLength(20);
                address.Property(a => a.City).HasColumnName($"{prefix}_city").HasMaxLength(100);
                address.Property(a => a.CountryCode).HasColumnName($"{prefix}_country_code").HasMaxLength(2);
            });
            party.Navigation(p => p.Address).IsRequired();
        });
        builder.Navigation(navigation).IsRequired();
    }

    private static void ConfigureCreditNoteLines(OwnedNavigationBuilder<CreditNote, CreditNoteLine> lines)
    {
        lines.ToTable("credit_note_lines");
        lines.WithOwner().HasForeignKey("credit_note_id");
        lines.Property<int>("Id").ValueGeneratedOnAdd();
        lines.HasKey("Id");

        lines.Property(l => l.Id).HasColumnName("line_id").HasMaxLength(50);
        lines.Property(l => l.Description).HasColumnName("description").HasMaxLength(500);
        lines.Property(l => l.Quantity).HasColumnName("quantity").HasPrecision(18, 4);
        lines.Property(l => l.UnitCode).HasColumnName("unit_code").HasMaxLength(10);
        lines.Property(l => l.UnitNetPrice).HasColumnName("unit_net_price").HasPrecision(18, 4);
        lines.Property(l => l.VatCategory).HasColumnName("vat_category").HasConversion<string>().HasMaxLength(20);
        lines.Property(l => l.VatRate).HasColumnName("vat_rate").HasPrecision(5, 2);
    }

    private static void ConfigureVatBreakdowns(OwnedNavigationBuilder<CreditNote, VatBreakdown> vat)
    {
        vat.ToTable("credit_note_vat_breakdowns");
        vat.WithOwner().HasForeignKey("credit_note_id");
        vat.Property<int>("Id").ValueGeneratedOnAdd();
        vat.HasKey("Id");

        vat.Property(v => v.Category).HasColumnName("vat_category").HasConversion<string>().HasMaxLength(20);
        vat.Property(v => v.Rate).HasColumnName("rate").HasPrecision(5, 2);
        vat.Property(v => v.BasisAmount).HasColumnName("basis_amount").HasPrecision(18, 2);
        vat.Property(v => v.TaxAmount).HasColumnName("tax_amount").HasPrecision(18, 2);
    }
}
