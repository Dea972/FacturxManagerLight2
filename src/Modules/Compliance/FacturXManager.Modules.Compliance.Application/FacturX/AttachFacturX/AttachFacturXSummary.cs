namespace FacturXManager.Modules.Compliance.Application.FacturX.AttachFacturX;

/// <summary>
/// Contient le résultat de l'opération d'assemblage / embedding PDF/A-3b Factur-X.
/// </summary>
/// <param name="InvoiceNumber">Le numéro de facture lié.</param>
/// <param name="Profile">Le profil Factur-X appliqué (ex : EN16931).</param>
/// <param name="EmbeddedFileName">Le nom réglementaire du fichier embarqué (factur-x.xml).</param>
/// <param name="CrossIndustryInvoiceXml">Le contenu XML CII généré.</param>
/// <param name="FacturXPdfBase64">Le PDF/A-3b finalisé encodé en Base64.</param>
/// <param name="TotalWithVat">Le total TTC calculé et validé.</param>
public sealed record AttachFacturXSummary(
    string InvoiceNumber,
    string Profile,
    string EmbeddedFileName,
    string CrossIndustryInvoiceXml,
    string FacturXPdfBase64,
    decimal TotalWithVat);
