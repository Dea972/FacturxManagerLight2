namespace FacturXManager.Client.Compliance.Models;

public sealed class CertifyInvoiceDto
{
    public string InvoiceNumber { get; set; } = string.Empty;
    public string Profile { get; set; } = string.Empty;
    public string EmbeddedFileName { get; set; } = string.Empty;
    public string CrossIndustryInvoiceXml { get; set; } = string.Empty;
    public string FacturXPdfBase64 { get; set; } = string.Empty;
    public decimal TotalWithVat { get; set; }
}
