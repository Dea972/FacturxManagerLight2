namespace FacturXManager.Modules.Compliance.Application.CreditNotes.GenerateCreditNote;

/// <summary>
/// Résultat de la génération d'un avoir Factur-X : PDF/A-3b en Base64 + XML CII.
/// </summary>
public sealed record GenerateCreditNoteSummary(
    string CreditNoteNumber,
    string Profile,
    string EmbeddedFileName,
    string CrossIndustryInvoiceXml,
    string FacturXPdfBase64,
    decimal TotalWithVat);
