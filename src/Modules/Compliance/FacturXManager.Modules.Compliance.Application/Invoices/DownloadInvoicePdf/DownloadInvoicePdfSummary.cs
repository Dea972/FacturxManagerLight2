namespace FacturXManager.Modules.Compliance.Application.Invoices.DownloadInvoicePdf;

/// <summary>
/// Résultat du téléchargement : octets du PDF, nom de fichier suggéré,
/// XMP embarqué pour diagnostic PDF/A et XML CII généré.
/// </summary>
public sealed record DownloadInvoicePdfSummary(
    byte[] PdfBytes,
    string FileName,
    string CrossIndustryInvoiceXml,
    string XmpMetadata);
