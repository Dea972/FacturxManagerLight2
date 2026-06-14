using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Domain.Common;
using FacturXManager.Modules.Compliance.Domain.Invoices;

namespace FacturXManager.Modules.Compliance.UnitTests.Abstractions;

/// <summary>
/// Fabriques de tests pour construire des factures valides ou ciblées.
/// </summary>
internal static class InvoiceTestData
{
    public static PostalAddress FrenchAddress(string city = "Paris", string postalCode = "75001") =>
        PostalAddress.Create("1 rue de Rivoli", postalCode, city, "FR");

    public static Party SellerWithSiren(string siren = "552120222",string siret= "55212022212345", string? vat = "FR00552120222") =>
        Party.Create("Vendeur SA", FrenchAddress(), siren,siret, vat);

    public static Party BuyerWithoutSiren() =>
        Party.Create("Acheteur SAS", FrenchAddress("Lyon", "69001"), "55212022212345");

    public static Party BuyerWithSiren(string siren = "732829320") =>
        Party.Create("Acheteur SAS", FrenchAddress("Lyon", "69001"), siren);

    public static InvoiceLine StandardLine(decimal quantity, decimal unitNetPrice, decimal vatRate = 20m) =>
        InvoiceLine.Create( "Prestation de conseil", quantity, unitNetPrice, VatCategory.Standard, vatRate);

    public static Result<Invoice> BuildInvoice(
        DateOnly? issueDate = null,
        OperationNature operationNature = OperationNature.Services,
        Party? buyer = null,
        IReadOnlyCollection<InvoiceLine>? lines = null,
        FacturXProfile profile = FacturXProfile.En16931) =>
        Invoice.Create(
            number: "F-2026-0001",
            issueDate: issueDate ?? new DateOnly(2026, 9, 15),
            currencyCode: "EUR",
            profile: profile,
            operationNature: operationNature,
            seller: SellerWithSiren(),
            buyer: buyer ?? BuyerWithSiren(),
            payment: PaymentDetails.Create("58", new DateOnly(2026, 10, 15), "FR7630006000011234567890189"),
            lines: lines ?? [StandardLine(quantity: 2m, unitNetPrice: 100m)]);
}
