using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Domain.Common;

namespace FacturXManager.Modules.Compliance.Application.Abstractions;

/// <summary>
/// Point d'extension produisant les octets du PDF/A-3b avec le XML CII embarqué (factur-x.xml).
/// L'implémentation concrète repose sur une bibliothèque PDF/A-3b (à câbler en Infrastructure).
/// </summary>
public interface IFacturXPdfA3BRenderer
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
    decimal TotalWithVat,
    FacturXProfile Profile = FacturXProfile.En16931,
    string? SellerAddress = null,
    string? SellerVatIdentifier = null,
    string? SellerLegalRegistrationId = null,
    string? BuyerAddress = null,
    string? BuyerVatIdentifier = null,
    string? BuyerLegalRegistrationId = null,
    decimal? AmountDue = null,
    string? PaymentMeansCode = null,
    string? PaymentIban = null,
    DateOnly? PaymentDueDate = null,
    IReadOnlyList<RenderLineItem>? Lines = null,
    IReadOnlyList<RenderVatBreakdown>? VatBreakdowns = null);

/// <summary>
/// Ligne de facture pour le rendu PDF.
/// </summary>
public sealed record RenderLineItem(
    string Id,
    string Description,
    decimal Quantity,
    string UnitCode,
    decimal UnitNetPrice,
    decimal VatRate,
    decimal NetAmount);

/// <summary>
/// Ventilation TVA pour le rendu PDF.
/// </summary>
public sealed record RenderVatBreakdown(
    string Category,
    decimal Rate,
    decimal BasisAmount,
    decimal TaxAmount);
