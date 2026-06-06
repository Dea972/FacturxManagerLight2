namespace FacturXManager.Modules.Compliance.Application.Invoices.GetInvoices;

/// <summary>
/// Résumé d'une facture pour l'affichage dans la liste — pas de données binaires.
/// </summary>
public sealed record InvoiceSummaryResponse(
    Guid Id,
    string Number,
    DateOnly IssueDate,
    string CurrencyCode,
    string Profile,
    string SellerName,
    string BuyerName,
    decimal TotalWithVat,
    string Status,
    string? ChannelDepositId);
