using System;

namespace FacturXManager.Client.Transmission.Models;

public sealed class SubmitTransmissionDto
{
    public Guid TransmissionId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ChannelDepositId { get; set; }
    public string? Message { get; set; }
}
