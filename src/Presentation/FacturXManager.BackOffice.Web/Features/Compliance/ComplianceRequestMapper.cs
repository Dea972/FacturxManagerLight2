using System.Globalization;
using FacturXManager.BackOffice.Web.Features.CreditNotes.ViewModels;
using FacturXManager.BackOffice.Web.Features.Invoices.ViewModels;
using FacturXManager.Client.Compliance.Models;

namespace FacturXManager.BackOffice.Web.Features.Compliance;

/// <summary>
/// Mappe les ViewModels de formulaire vers les DTOs de requête Refit du backend.
/// </summary>
internal static class ComplianceRequestMapper
{
    public static InvoiceRequest ToRequest(InvoiceFormViewModel form) => new()
    {
        Number = form.Number,
        IssueDate = ToIsoDate(form.IssueDate),
        CurrencyCode = form.CurrencyCode,
        Profile = form.Profile,
        OperationNature = form.OperationNature,
        BuyerReference = form.BuyerReference,
        PurchaseOrderReference = form.PurchaseOrderReference,
        Seller = ToRequest(form.Seller),
        Buyer = ToRequest(form.Buyer),
        Payment = new PaymentRequest
        {
            MeansCode = form.Payment.MeansCode,
            DueDate = form.Payment.DueDate is { } due ? ToIsoDate(due) : null,
            Iban = form.Payment.Iban,
        },
        Lines = form.Lines.ConvertAll(line => new InvoiceLineRequest
        {
            Id = line.Id,
            Description = line.Description,
            Quantity = line.Quantity,
            UnitNetPrice = line.UnitNetPrice,
            VatCategory = line.VatCategory,
            VatRate = line.VatRate,
            UnitCode = line.UnitCode,
        }),
    };

    public static CreditNoteRequest ToRequest(CreditNoteFormViewModel form) => new()
    {
        Number = form.Number,
        IssueDate = ToIsoDate(form.IssueDate),
        CurrencyCode = form.CurrencyCode,
        Profile = form.Profile,
        OperationNature = form.OperationNature,
        OriginalInvoiceNumber = form.OriginalInvoiceNumber,
        OriginalInvoiceIssueDate = form.OriginalInvoiceIssueDate is { } date ? ToIsoDate(date) : null,
        CreditReason = form.CreditReason,
        BuyerReference = form.BuyerReference,
        PaymentMeansCode = form.PaymentMeansCode,
        Seller = ToRequest(form.Seller),
        Buyer = ToRequest(form.Buyer),
        Lines = form.Lines.ConvertAll(line => new CreditNoteLineRequest
        {
            Id = line.Id,
            Description = line.Description,
            Quantity = line.Quantity,
            UnitNetPrice = line.UnitNetPrice,
            VatCategory = line.VatCategory,
            VatRate = line.VatRate,
            UnitCode = line.UnitCode,
        }),
    };

    private static PartyRequest ToRequest(PartyFormViewModel party) => new()
    {
        Name = party.Name,
        AddressLine1 = party.AddressLine1,
        AddressLine2 = party.AddressLine2,
        PostalCode = party.PostalCode,
        City = party.City,
        CountryCode = party.CountryCode,
        LegalRegistrationId = party.LegalRegistrationId,
        VatIdentifier = party.VatIdentifier,
    };

    private static string ToIsoDate(DateTime value) =>
        value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
}
