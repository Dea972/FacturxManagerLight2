using System;

namespace FacturXManager.Client.EReporting.Models;

public sealed class SubmitDeclarationDto
{
    public Guid DeclarationId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string SubmissionReference { get; set; } = string.Empty;
}
