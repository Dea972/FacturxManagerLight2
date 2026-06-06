using FacturXManager.BackOffice.Web.Common;
using FacturXManager.BackOffice.Web.Features.Transmission;
using FacturXManager.Client.Transmission;
using FacturXManager.Client.Transmission.Models;
using MediatR;
using Refit;

namespace FacturXManager.BackOffice.Web.Features.Transmission.SubmitTransmission;

/// <summary>
/// Use case front : dépose un PDF Factur-X (Base64) sur Chorus Pro via le backend.
/// </summary>
public sealed record SubmitTransmissionRequestFront(
    string DocumentReference,
    string RecipientSiret,
    string Channel,
    string FacturXPdfBase64,
    string? ExecutingService)
    : IRequest<Result<TransmissionResultViewModel>>;

internal sealed class SubmitTransmissionHandlerFront(
    ITransmissionApi transmissionApi,
    ILogger<SubmitTransmissionHandlerFront> logger)
    : IRequestHandler<SubmitTransmissionRequestFront, Result<TransmissionResultViewModel>>
{
    public async Task<Result<TransmissionResultViewModel>> Handle(
        SubmitTransmissionRequestFront request,
        CancellationToken cancellationToken)
    {
        try
        {
            var apiRequest = new SubmitTransmissionRequest
            {
                DocumentReference = request.DocumentReference,
                RecipientSiret = request.RecipientSiret,
                Channel = request.Channel,
                FacturXPdfBase64 = request.FacturXPdfBase64,
                ExecutingService = request.ExecutingService,
            };

            SubmitTransmissionDto dto =
                await transmissionApi.SubmitAsync(apiRequest, cancellationToken);

            return Result<TransmissionResultViewModel>.Success(new TransmissionResultViewModel(
                dto.TransmissionId,
                dto.Status,
                dto.ChannelDepositId,
                dto.Message));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Échec du dépôt de la transmission {DocumentReference}", request.DocumentReference);
            return Result<TransmissionResultViewModel>.Failure(ApiErrorTranslator.Describe(exception));
        }
    }
}
