using FacturXManager.BackOffice.Web.Common;
using FacturXManager.BackOffice.Web.Features.Compliance;
using FacturXManager.BackOffice.Web.Features.CreditNotes.ViewModels;
using FacturXManager.Client.Compliance;
using FacturXManager.Client.Compliance.Models;
using FacturXManager.Client.Transmission;
using FacturXManager.Client.Transmission.Models;
using MediatR;

namespace FacturXManager.BackOffice.Web.Features.CreditNotes.GenerateAndSubmitCreditNote;

/// <summary>
/// Résultat affiché après génération + dépôt d'un avoir.
/// </summary>
public sealed record GenerateAndSubmitCreditNoteViewModel(
    Guid CreditNoteId,
    string CreditNoteNumber,
    string Profile,
    decimal TotalWithVat,
    string FacturXPdfBase64,
    string Status,
    string? ChannelDepositId,
    string? Message);

/// <summary>
/// Use case front : génère un avoir Factur-X et le dépose sur Chorus Pro en une étape.
/// </summary>
public sealed record GenerateAndSubmitCreditNoteRequest(
        CreditNoteFormViewModel Form,
        string RecipientSiret,
        string Channel,
        string? ExecutingService = null)
    : IRequest<Result<GenerateAndSubmitCreditNoteViewModel>>;

internal sealed class GenerateAndSubmitCreditNoteHandler(
    ICreditNoteApi creditNoteApi,
    ITransmissionApi transmissionApi,
    ILogger<GenerateAndSubmitCreditNoteHandler> logger)
    : IRequestHandler<GenerateAndSubmitCreditNoteRequest, Result<GenerateAndSubmitCreditNoteViewModel>>
{
    public async Task<Result<GenerateAndSubmitCreditNoteViewModel>> Handle(
        GenerateAndSubmitCreditNoteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var apiRequest = new Client.Compliance.Models.GenerateCreditNoteRequest
            {
                CreditNote = ComplianceRequestMapper.ToRequest(request.Form),
            };

            GenerateCreditNoteDto dto =
                await creditNoteApi.GenerateAsync(apiRequest, cancellationToken);

            var submitTransmissionRequest = new SubmitTransmissionRequest
            {
                DocumentReference = dto.EmbeddedFileName,
                FacturXPdfBase64 = dto.FacturXPdfBase64,
                RecipientSiret = request.RecipientSiret,
                Channel = request.Channel,
                ExecutingService = request.ExecutingService
            };

            SubmitTransmissionDto transmissionDto = await transmissionApi.SubmitAsync(submitTransmissionRequest, cancellationToken);

            return Result<GenerateAndSubmitCreditNoteViewModel>.Success(
                new GenerateAndSubmitCreditNoteViewModel(
                        Guid.CreateVersion7(),
                        dto.CreditNoteNumber,
                        dto.Profile,
                        dto.TotalWithVat,
                        dto.FacturXPdfBase64,
                        transmissionDto.Status,
                        transmissionDto.ChannelDepositId,
                        transmissionDto.Message));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Échec de la génération + dépôt de l'avoir {CreditNoteNumber}", request.Form.Number);
            return Result<GenerateAndSubmitCreditNoteViewModel>.Failure(ApiErrorTranslator.Describe(exception));
        }
    }
}
