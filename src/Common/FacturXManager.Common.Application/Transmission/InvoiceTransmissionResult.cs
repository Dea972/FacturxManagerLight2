namespace FacturXManager.Common.Application.Transmission;

/// <summary>
/// Résultat brut d'une tentative d'acheminement, retourné par un canal.
/// </summary>
public sealed record InvoiceTransmissionResult(
    TransmissionOutcome Outcome,
    string? ChannelDepositId,
    string? RawResponse,
    string? Message)
{
    public static InvoiceTransmissionResult Submitted(string channelDepositId, string? rawResponse = null) =>
        new(TransmissionOutcome.Submitted, channelDepositId, rawResponse, null);

    public static InvoiceTransmissionResult Rejected(string message, string? rawResponse = null) =>
        new(TransmissionOutcome.Rejected, null, rawResponse, message);

    public static InvoiceTransmissionResult TechnicalFailure(string message, string? rawResponse = null) =>
        new(TransmissionOutcome.TechnicalFailure, null, rawResponse, message);
}
