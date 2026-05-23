namespace FacturXManager.Modules.Compliance.Application.Invoicing.CertifyInvoice;

/// <summary>
/// Résultat d'une certification : document Factur-X produit (PDF/A-3b en Base64 + XML CII).
/// </summary>
public sealed record CertifyInvoiceSummary(
    string InvoiceNumber,
    string Profile,
    string EmbeddedFileName,
    string CrossIndustryInvoiceXml,
    string FacturXPdfBase64,
    decimal TotalWithVat);
