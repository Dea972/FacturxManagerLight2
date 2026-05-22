using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Transmission.Domain.Transmissions;
using DomainTransmission = FacturXManager.Modules.Transmission.Domain.Transmissions.Transmission;

namespace FacturXManager.Modules.Transmission.Application.Transmissions.GetTransmission;

internal sealed class GetTransmissionQueryHandler(ITransmissionRepository repository)
    : IQueryHandler<GetTransmissionQuery, TransmissionResponse>
{
    public async Task<Result<TransmissionResponse>> Handle(
        GetTransmissionQuery request,
        CancellationToken cancellationToken)
    {
        DomainTransmission? transmission = await repository.GetAsync(request.TransmissionId, cancellationToken);

        if (transmission is null)
        {
            return Result.Failure<TransmissionResponse>(TransmissionErrors.NotFound(request.TransmissionId));
        }

        IReadOnlyList<TransmissionAttemptResponse> attempts = transmission.Attempts
            .OrderBy(a => a.OccurredOnUtc)
            .Select(a => new TransmissionAttemptResponse(
                a.Status.ToString(),
                a.OccurredOnUtc,
                a.ChannelDepositId,
                a.Message))
            .ToList();

        return new TransmissionResponse(
            transmission.Id,
            transmission.DocumentReference,
            transmission.RecipientSiret,
            transmission.ExecutingService,
            transmission.Channel.ToString(),
            transmission.Status.ToString(),
            transmission.ChannelDepositId,
            transmission.RejectionReason,
            transmission.CreatedOnUtc,
            transmission.LastUpdatedOnUtc,
            attempts);
    }
}
