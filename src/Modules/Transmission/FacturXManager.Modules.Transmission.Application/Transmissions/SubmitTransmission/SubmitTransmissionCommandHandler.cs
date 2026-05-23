using FacturXManager.Common.Application.Clock;
using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Application.Transmission;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Transmission.Application.Abstractions.Data;
using DomainTransmission = FacturXManager.Modules.Transmission.Domain.Transmissions.Transmission;
using FacturXManager.Modules.Transmission.Domain.Transmissions;
using SharedChannel = FacturXManager.Common.Application.Transmission.TransmissionChannel;
using DomainChannel = FacturXManager.Modules.Transmission.Domain.Transmissions.TransmissionChannel;

namespace FacturXManager.Modules.Transmission.Application.Transmissions.SubmitTransmission;

internal sealed class SubmitTransmissionCommandHandler(
    ITransmissionRepository repository,
    IUnitOfWork unitOfWork,
    IInvoiceTransmitter transmitter,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<SubmitTransmissionCommand, SubmitTransmissionSummary>
{
    public async Task<Result<SubmitTransmissionSummary>> Handle(
        SubmitTransmissionCommand request,
        CancellationToken cancellationToken)
    {
        if (!Enum.TryParse(request.Channel?.Trim(), ignoreCase: true, out DomainChannel channel) ||
            !Enum.IsDefined(channel))
        {
            return Result.Failure<SubmitTransmissionSummary>(
                TransmissionErrors.InvalidChannel(request.Channel ?? string.Empty));
        }

        Result<DomainTransmission> createResult = DomainTransmission.Create(
            request.DocumentReference,
            request.RecipientSiret,
            channel,
            dateTimeProvider.UtcNow,
            request.ExecutingService);

        if (createResult.IsFailure)
        {
            return Result.Failure<SubmitTransmissionSummary>(createResult.Error);
        }

        DomainTransmission transmission = createResult.Value;

        var transmissionRequest = new InvoiceTransmissionRequest(
            (SharedChannel)channel,
            transmission.DocumentReference,
            transmission.RecipientSiret,
            request.FacturXPdfBase64,
            transmission.ExecutingService);

        InvoiceTransmissionResult result = await transmitter.TransmitAsync(transmissionRequest, cancellationToken);

        DateTime now = dateTimeProvider.UtcNow;
        switch (result.Outcome)
        {
            case TransmissionOutcome.Submitted:
                transmission.RecordSubmission(result.ChannelDepositId ?? string.Empty, result.RawResponse, now);
                break;
            case TransmissionOutcome.Rejected:
                transmission.RecordRejection(result.Message ?? "Rejet plateforme", result.RawResponse, now);
                break;
            default:
                transmission.RecordTechnicalFailure(
                    result.Message ?? "Échec technique",
                    result.RawResponse,
                    now);
                break;
        }

        // Trace de la tentative (sera persistée dès que la couche de persistance sera activée).
        repository.Insert(transmission);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Outcome switch
        {
            TransmissionOutcome.Submitted => new SubmitTransmissionSummary(
                transmission.Id,
                transmission.Status.ToString(),
                transmission.ChannelDepositId,
                result.Message),

            TransmissionOutcome.Rejected => Result.Failure<SubmitTransmissionSummary>(
                TransmissionErrors.Rejected(result.Message ?? "Rejet plateforme")),

            TransmissionOutcome.Timeout => Result.Failure<SubmitTransmissionSummary>(
                TransmissionErrors.Timeout(result.Message ?? "Timeout plateforme")),

            _ => Result.Failure<SubmitTransmissionSummary>(
                TransmissionErrors.TechnicalFailure(result.Message ?? "Erreur technique")),
        };
    }
}
