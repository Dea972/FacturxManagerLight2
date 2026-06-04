using System.ComponentModel.DataAnnotations;
using FacturXManager.BackOffice.Web.Features.Invoices.ViewModels;

namespace FacturXManager.BackOffice.Web.Features.CreditNotes.ViewModels;

/// <summary>
/// ViewModel éditable d'un avoir, lié aux formulaires Radzen.
/// </summary>
public sealed class CreditNoteFormViewModel
{
    [Required(ErrorMessage = "Le numéro d'avoir est obligatoire.")]
    public string Number { get; set; } = string.Empty;

    [Required]
    public DateTime IssueDate { get; set; } = DateTime.Today;

    [Required]
    public string CurrencyCode { get; set; } = "EUR";

    [Required]
    public string Profile { get; set; } = "EN16931";

    [Required]
    public string OperationNature { get; set; } = "Goods";

    [Required(ErrorMessage = "Le numéro de la facture d'origine est obligatoire.")]
    public string OriginalInvoiceNumber { get; set; } = string.Empty;

    public DateTime? OriginalInvoiceIssueDate { get; set; }

    public string? CreditReason { get; set; }

    public string? BuyerReference { get; set; }

    public string PaymentMeansCode { get; set; } = "BankTransfer";

    public PartyFormViewModel Seller { get; set; } = new();

    public PartyFormViewModel Buyer { get; set; } = new();

    public List<CreditNoteLineFormViewModel> Lines { get; set; } = [new()];
}

public sealed class CreditNoteLineFormViewModel
{
    [Required]
    public string Id { get; set; } = "1";

    [Required(ErrorMessage = "La désignation est obligatoire.")]
    public string Description { get; set; } = string.Empty;

    public decimal Quantity { get; set; } = 1m;

    public decimal UnitNetPrice { get; set; }

    public string VatCategory { get; set; } = "Standard";

    public decimal VatRate { get; set; } = 20m;

    public string UnitCode { get; set; } = "C62";
}
