namespace FacturXManager.Client.Transmission.Models;

public sealed class SubmitTransmissionRequest
{
    public string DocumentReference { get; set; } = string.Empty;
    public string RecipientSiret { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string FacturXPdfBase64 { get; set; } = string.Empty;
    public string? ExecutingService { get; set; }
}
