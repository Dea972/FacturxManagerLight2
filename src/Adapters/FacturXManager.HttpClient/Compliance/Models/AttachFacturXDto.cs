namespace FacturXManager.Client.Compliance.Models;

/// <summary>
/// DTO représentant le résultat de l'attachement d'une Factur-X à un PDF.
/// </summary>
public sealed class AttachFacturXDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public string Profile { get; set; } = string.Empty;
    public string EmbeddedFileName { get; set; } = string.Empty;
    public string CrossIndustryInvoiceXml { get; set; } = string.Empty;
    public string FacturXPdfBase64 { get; set; } = string.Empty;
    public decimal TotalWithVat { get; set; }
}
