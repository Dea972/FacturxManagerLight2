using System;
using System.Collections.Generic;

namespace FacturXManager.Client.Compliance.Models;

/// <summary>
/// Représentation scalaire d'un avoir, mirroir du contrat backend CreditNoteModel.
/// </summary>
public sealed class CreditNoteRequest
{
    public string Number { get; set; } = string.Empty;
    public string IssueDate { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = string.Empty;
    public string Profile { get; set; } = string.Empty;
    public string OperationNature { get; set; } = string.Empty;
    public PartyRequest Seller { get; set; } = new();
    public PartyRequest Buyer { get; set; } = new();
    public string OriginalInvoiceNumber { get; set; } = string.Empty;
    public List<CreditNoteLineRequest> Lines { get; set; } = new();
    public string? OriginalInvoiceIssueDate { get; set; }
    public string? CreditReason { get; set; }
    public string? BuyerReference { get; set; }
    public string PaymentMeansCode { get; set; } = "BankTransfer";
}

public sealed class CreditNoteLineRequest
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitNetPrice { get; set; }
    public string VatCategory { get; set; } = string.Empty;
    public decimal VatRate { get; set; }
    public string UnitCode { get; set; } = "C62";
}

/// <summary>
/// Corps de requête pour la génération d'un avoir Factur-X.
/// </summary>
public sealed class GenerateCreditNoteRequest
{
    public CreditNoteRequest CreditNote { get; set; } = new();
}

/// <summary>
/// Corps de requête pour le téléchargement direct du PDF d'un avoir.
/// </summary>
public sealed class DownloadCreditNoteRequest
{
    public CreditNoteRequest CreditNote { get; set; } = new();
}

/// <summary>
/// Corps de requête pour le téléchargement direct du PDF d'une facture.
/// </summary>
public sealed class DownloadInvoiceRequest
{
    public InvoiceRequest Invoice { get; set; } = new();
}
