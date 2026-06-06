namespace FacturXManager.Client.Compliance.Models;

/// <summary>
/// Résultat de la génération d'un avoir Factur-X : PDF/A-3b en Base64 + XML CII.
/// </summary>
public sealed class GenerateCreditNoteDto
{
    public string CreditNoteNumber { get; set; } = string.Empty;
    public string Profile { get; set; } = string.Empty;
    public string EmbeddedFileName { get; set; } = string.Empty;
    public string CrossIndustryInvoiceXml { get; set; } = string.Empty;
    public string FacturXPdfBase64 { get; set; } = string.Empty;
    public decimal TotalWithVat { get; set; }
}
