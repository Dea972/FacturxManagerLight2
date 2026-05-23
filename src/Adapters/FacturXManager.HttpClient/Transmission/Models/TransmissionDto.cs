using System;
using System.Collections.Generic;

namespace FacturXManager.Client.Transmission.Models;

public sealed class TransmissionDto
{
    public Guid Id { get; set; }
    public string DocumentReference { get; set; } = string.Empty;
    public string RecipientSiret { get; set; } = string.Empty;
    public string? ExecutingService { get; set; }
    public string Channel { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? ChannelDepositId { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedOnUtc { get; set; }
    public DateTime LastUpdatedOnUtc { get; set; }
    public List<TransmissionAttemptDto> Attempts { get; set; } = new();
}

public sealed class TransmissionAttemptDto
{
    public string Status { get; set; } = string.Empty;
    public DateTime OccurredOnUtc { get; set; }
    public string? ChannelDepositId { get; set; }
    public string? Message { get; set; }
}
