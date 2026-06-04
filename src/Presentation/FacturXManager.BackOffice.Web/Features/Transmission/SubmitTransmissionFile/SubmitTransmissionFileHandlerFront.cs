using FacturXManager.BackOffice.Web.Common;
using FacturXManager.BackOffice.Web.Features.Transmission;
using FacturXManager.Client.Transmission;
using FacturXManager.Client.Transmission.Models;
using MediatR;
using Refit;

namespace FacturXManager.BackOffice.Web.Features.Transmission.SubmitTransmissionFile;

/// <summary>
/// Use case front : dépose un fichier PDF Factur-X (upload) sur Chorus Pro via le backend.
/// </summary>
public sealed record SubmitTransmissionFileRequestFront(
    string DocumentReference,
    string RecipientSiret,
    string Channel,
    string? ExecutingService,
    byte[] FileContent,
    string FileName)
    : IRequest<Result<TransmissionResultViewModel>>;

internal sealed class SubmitTransmissionFileHandlerFront(
    ITransmissionApi transmissionApi,
    ILogger<SubmitTransmissionFileHandlerFront> logger)
    : IRequestHandler<SubmitTransmissionFileRequestFront, Result<TransmissionResultViewModel>>
{
    public async Task<Result<TransmissionResultViewModel>> Handle(
        SubmitTransmissionFileRequestFront request,
        CancellationToken cancellationToken)
    {
        try
        {
            var stream = new MemoryStream(request.FileContent);
            var streamPart = new StreamPart(stream, request.FileName, "application/pdf");

            SubmitTransmissionDto dto = await transmissionApi.SubmitFileAsync(
                streamPart,
                request.DocumentReference,
                request.RecipientSiret,
                request.Channel,
                request.ExecutingService,
                cancellationToken);

            return Result<TransmissionResultViewModel>.Success(new TransmissionResultViewModel(
                dto.TransmissionId,
                dto.Status,
                dto.ChannelDepositId,
                dto.Message));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Échec du dépôt du fichier de transmission {DocumentReference}", request.DocumentReference);
            return Result<TransmissionResultViewModel>.Failure(ApiErrorTranslator.Describe(exception));
        }
    }
}
