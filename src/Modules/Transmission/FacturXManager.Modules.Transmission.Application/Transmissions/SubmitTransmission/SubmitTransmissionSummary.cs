namespace FacturXManager.Modules.Transmission.Application.Transmissions.SubmitTransmission;

public sealed record SubmitTransmissionSummary(
    Guid TransmissionId,
    string Status,
    string? ChannelDepositId,
    string? Message);
