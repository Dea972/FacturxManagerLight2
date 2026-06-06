using FacturXManager.BackOffice.Web.Common;
using FacturXManager.BackOffice.Web.Features.Compliance;
using FacturXManager.BackOffice.Web.Features.Invoices.ViewModels;
using FacturXManager.Client.Compliance;
using FacturXManager.Client.Compliance.Models;
using MediatR;

namespace FacturXManager.BackOffice.Web.Features.Invoices.GenerateInvoice;

/// <summary>
/// Use case front : génère une facture Factur-X et retourne le document Factur-X généré.
/// </summary>
public sealed record GenerateInvoiceRequest(InvoiceFormViewModel Form)
    : IRequest<Result<CertifiedInvoiceViewModel>>;

/// <summary>
/// Résultat affichable d'une certification : métadonnées + PDF Base64 réutilisable pour le dépôt.
/// </summary>
public sealed record CertifiedInvoiceViewModel(
    string InvoiceNumber,
    string Profile,
    string EmbeddedFileName,
    decimal TotalWithVat,
    string FacturXPdfBase64,
    string CrossIndustryInvoiceXml);

internal sealed class GenerateInvoiceHandler(
    IInvoiceApi complianceApi,
    ILogger<GenerateInvoiceHandler> logger)
    : IRequestHandler<GenerateInvoiceRequest, Result<CertifiedInvoiceViewModel>>
{
    public async Task<Result<CertifiedInvoiceViewModel>> Handle(
        GenerateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var apiRequest = new Client.Compliance.Models. GenerateInvoiceRequest
            {
                Invoice = ComplianceRequestMapper.ToRequest(request.Form),
            };

            GenerateInvoiceDto dto = await complianceApi.GenerateAsync(apiRequest, cancellationToken);

            return Result<CertifiedInvoiceViewModel>.Success(new CertifiedInvoiceViewModel(
                dto.InvoiceNumber,
                dto.Profile,
                dto.EmbeddedFileName,
                dto.TotalWithVat,
                dto.FacturXPdfBase64,
                dto.CrossIndustryInvoiceXml));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Échec de la certification de la facture {InvoiceNumber}", request.Form.Number);
            return Result<CertifiedInvoiceViewModel>.Failure(ApiErrorTranslator.Describe(exception));
        }
    }
}
