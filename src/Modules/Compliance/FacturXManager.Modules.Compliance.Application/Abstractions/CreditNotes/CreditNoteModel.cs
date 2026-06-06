using FacturXManager.Modules.Compliance.Application.Abstractions.Invoices;

namespace FacturXManager.Modules.Compliance.Application.Abstractions.CreditNotes;

/// <summary>
/// Représentation scalaire d'un avoir, partagée par les use cases du module.
/// Aucun type Domain n'apparaît ici : le handler instancie les types Domain à partir de ces scalaires.
/// </summary>
public sealed record CreditNoteModel(
    string Number,
    DateOnly IssueDate,
    string CurrencyCode,
    string Profile,
    string OperationNature,
    PartyModel Seller,
    PartyModel Buyer,
    string OriginalInvoiceNumber,
    IReadOnlyList<CreditNoteLineModel> Lines,
    DateOnly? OriginalInvoiceIssueDate = null,
    string? CreditReason = null,
    string? BuyerReference = null,
    PaymentMeansCode PaymentMeansCode = PaymentMeansCode.BankTransfer);

public sealed record CreditNoteLineModel(
    string Id,
    string Description,
    decimal Quantity,
    decimal UnitNetPrice,
    string VatCategory,
    decimal VatRate,
    string UnitCode = "C62");
