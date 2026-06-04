using FacturXManager.Modules.Compliance.Domain.Common;
using FacturXManager.Modules.Compliance.Domain.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacturXManager.Modules.Compliance.Infrastructure.Invoices;

internal sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Number).HasMaxLength(200);
        builder.Property(i => i.CurrencyCode).HasMaxLength(3);
        builder.Property(i => i.BuyerReference).HasMaxLength(200);
        builder.Property(i => i.PurchaseOrderReference).HasMaxLength(200);

        builder.Property(i => i.Profile).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.OperationNature).HasConversion<string>().HasMaxLength(30);
        builder.Property(i => i.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(i => i.ChannelDepositId).HasMaxLength(100);

        builder.HasIndex(i => i.Number).IsUnique();

        ConfigureParty(builder, i => i.Seller, "seller");
        ConfigureParty(builder, i => i.Buyer, "buyer");

        builder.OwnsOne(i => i.Payment, payment =>
        {
            payment.Property(p => p.MeansCode).HasColumnName("payment_means_code").HasMaxLength(10);
            payment.Property(p => p.Iban).HasColumnName("payment_iban").HasMaxLength(34);
            payment.Property(p => p.DueDate).HasColumnName("payment_due_date");
        });
        builder.Navigation(i => i.Payment).IsRequired();

        builder.OwnsMany(i => i.Lines, ConfigureInvoiceLines);
        builder.Metadata
            .FindNavigation(nameof(Invoice.Lines))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.OwnsMany(i => i.VatBreakdowns, ConfigureVatBreakdowns);
        builder.Metadata
            .FindNavigation(nameof(Invoice.VatBreakdowns))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }

    private static void ConfigureParty(
        EntityTypeBuilder<Invoice> builder,
        System.Linq.Expressions.Expression<System.Func<Invoice, Party?>> navigation,
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

    private static void ConfigureInvoiceLines(OwnedNavigationBuilder<Invoice, InvoiceLine> lines)
    {
        lines.ToTable("invoice_lines");
        lines.WithOwner().HasForeignKey("invoice_id");
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

    private static void ConfigureVatBreakdowns(OwnedNavigationBuilder<Invoice, VatBreakdown> vat)
    {
        vat.ToTable("invoice_vat_breakdowns");
        vat.WithOwner().HasForeignKey("invoice_id");
        vat.Property<int>("Id").ValueGeneratedOnAdd();
        vat.HasKey("Id");

        vat.Property(v => v.Category).HasColumnName("vat_category").HasConversion<string>().HasMaxLength(20);
        vat.Property(v => v.Rate).HasColumnName("rate").HasPrecision(5, 2);
        vat.Property(v => v.BasisAmount).HasColumnName("basis_amount").HasPrecision(18, 2);
        vat.Property(v => v.TaxAmount).HasColumnName("tax_amount").HasPrecision(18, 2);
    }
}
