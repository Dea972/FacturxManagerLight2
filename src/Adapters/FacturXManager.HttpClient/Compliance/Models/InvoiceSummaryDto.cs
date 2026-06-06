using System;

namespace FacturXManager.Client.Compliance.Models;

/// <summary>
/// Résumé d'une facture retourné par l'endpoint GET /compliance/invoices.
/// </summary>
public sealed class InvoiceSummaryDto
{
    public Guid Id { get; set; }
    public string Number { get; set; } = string.Empty;
    public string IssueDate { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public string Profile { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public decimal TotalWithVat { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ChannelDepositId { get; set; }
}
