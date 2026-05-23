using System.Collections.Generic;

namespace FacturXManager.Client.Compliance.Models;

public sealed class ComplianceReportDto
{
    public bool IsCompliant { get; set; }
    public string Profile { get; set; } = string.Empty;
    public List<ComplianceViolationDto> Violations { get; set; } = new();
}

public sealed class ComplianceViolationDto
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
