namespace FacturXManager.Modules.Transmission.Application.Transmissions.GetTransmission;

public sealed record TransmissionResponse(
    Guid Id,
    string DocumentReference,
    string RecipientSiret,
    string? ExecutingService,
    string Channel,
    string Status,
    string? ChannelDepositId,
    string? RejectionReason,
    DateTime CreatedOnUtc,
    DateTime LastUpdatedOnUtc,
    IReadOnlyList<TransmissionAttemptResponse> Attempts);

public sealed record TransmissionAttemptResponse(
    string Status,
    DateTime OccurredOnUtc,
    string? ChannelDepositId,
    string? Message);
