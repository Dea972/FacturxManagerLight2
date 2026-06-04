namespace FacturXManager.Modules.Compliance.Application.Invoices.ValidateInvoiceCompliance;

/// <summary>
/// Rapport de conformité EN16931 d'une facture.
/// </summary>
public sealed record ComplianceReportResponse(
    bool IsCompliant,
    string Profile,
    IReadOnlyList<ComplianceViolation> Violations);

public sealed record ComplianceViolation(string Code, string Description);
