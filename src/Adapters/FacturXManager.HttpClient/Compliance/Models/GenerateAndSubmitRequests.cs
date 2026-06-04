namespace FacturXManager.Client.Compliance.Models;

/// <summary>
/// Corps de requête pour la génération + dépôt d'une facture en une étape.
/// </summary>
public sealed class GenerateAndSubmitInvoiceRequest
{
    public InvoiceRequest Invoice { get; set; } = new();
    public string RecipientSiret { get; set; } = string.Empty;
    public string Channel { get; set; } = "ChorusPro";
    public string? ExecutingService { get; set; }
}

/// <summary>
/// Corps de requête pour la génération + dépôt d'un avoir en une étape.
/// </summary>
public sealed class GenerateAndSubmitCreditNoteRequest
{
    public CreditNoteRequest CreditNote { get; set; } = new();
    public string RecipientSiret { get; set; } = string.Empty;
    public string Channel { get; set; } = "ChorusPro";
    public string? ExecutingService { get; set; }
}

/// <summary>
/// Résultat du dépôt (partagé par facture et avoir).
/// </summary>
public sealed class GenerateAndSubmitDto
{
    public Guid DocumentId { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Profile { get; set; } = string.Empty;
    public decimal TotalWithVat { get; set; }
    public string FacturXPdfBase64 { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ChannelDepositId { get; set; }
    public string? Message { get; set; }
}
