using FacturXManager.BackOffice.Web.Common;
using FacturXManager.BackOffice.Web.Features.Compliance;
using FacturXManager.BackOffice.Web.Features.CreditNotes.ViewModels;
using FacturXManager.Client.Compliance;
using FacturXManager.Client.Compliance.Models;
using MediatR;

namespace FacturXManager.BackOffice.Web.Features.CreditNotes.GenerateCreditNote;

/// <summary>
/// Use case front : génère un avoir Factur-X et retourne le document en Base64.
/// </summary>
public sealed record GenerateCreditNoteRequestFront(CreditNoteFormViewModel Form)
    : IRequest<Result<GeneratedCreditNoteViewModel>>;

/// <summary>
/// Résultat affichable d'une génération d'avoir.
/// </summary>
public sealed record GeneratedCreditNoteViewModel(
    string CreditNoteNumber,
    string Profile,
    string EmbeddedFileName,
    decimal TotalWithVat,
    string FacturXPdfBase64,
    string CrossIndustryInvoiceXml);

internal sealed class GenerateCreditNoteHandler(
    ICreditNoteApi creditNoteApi,
    ILogger<GenerateCreditNoteHandler> logger)
    : IRequestHandler<GenerateCreditNoteRequestFront, Result<GeneratedCreditNoteViewModel>>
{
    public async Task<Result<GeneratedCreditNoteViewModel>> Handle(
        GenerateCreditNoteRequestFront request,
        CancellationToken cancellationToken)
    {
        try
        {
            var apiRequest = new GenerateCreditNoteRequest
            {
                CreditNote = ComplianceRequestMapper.ToRequest(request.Form),
            };

            GenerateCreditNoteDto dto =
                await creditNoteApi.GenerateAsync(apiRequest, cancellationToken);

            return Result<GeneratedCreditNoteViewModel>.Success(new GeneratedCreditNoteViewModel(
                dto.CreditNoteNumber,
                dto.Profile,
                dto.EmbeddedFileName,
                dto.TotalWithVat,
                dto.FacturXPdfBase64,
                dto.CrossIndustryInvoiceXml));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Échec de la génération de l'avoir {CreditNoteNumber}", request.Form.Number);
            return Result<GeneratedCreditNoteViewModel>.Failure(ApiErrorTranslator.Describe(exception));
        }
    }
}
