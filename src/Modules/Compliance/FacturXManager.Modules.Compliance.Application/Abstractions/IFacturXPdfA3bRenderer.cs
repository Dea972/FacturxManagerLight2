using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.Compliance.Application.Abstractions;

/// <summary>
/// Point d'extension produisant les octets du PDF/A-3b avec le XML CII embarqué (factur-x.xml).
/// L'implémentation concrète repose sur une bibliothèque PDF/A-3b (à câbler en Infrastructure).
/// </summary>
public interface IFacturXPdfA3bRenderer
{
    Task<Result<byte[]>> RenderAsync(FacturXRenderRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Données nécessaires au rendu : le XML CII à embarquer + métadonnées d'affichage de la facture.
/// </summary>
public sealed record FacturXRenderRequest(
    string CrossIndustryInvoiceXml,
    string EmbeddedFileName,
    string InvoiceNumber,
    string SellerName,
    string BuyerName,
    DateOnly IssueDate,
    string CurrencyCode,
    decimal TotalWithoutVat,
    decimal TotalVat,
    decimal TotalWithVat);
