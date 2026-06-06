namespace FacturXManager.Modules.Compliance.Application.CreditNotes.DownloadCreditNotePdf;

/// <summary>
/// Résultat du téléchargement d'un avoir : octets du PDF et nom de fichier suggéré.
/// </summary>
public sealed record DownloadCreditNotePdfSummary(
    byte[] PdfBytes,
    string FileName,
    string CrossIndustryInvoiceXml);
