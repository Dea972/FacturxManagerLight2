using FacturXManager.Common.Application.Messaging;
using FacturXManager.Modules.Compliance.Application.Abstractions.Invoices;

namespace FacturXManager.Modules.Compliance.Application.Invoices.ValidateInvoiceCompliance;

/// <summary>
/// Vérifie la conformité EN16931 d'une facture sans produire de PDF.
/// </summary>
public sealed record ValidateInvoiceComplianceQuery(InvoiceModel Invoice) : IQuery<ComplianceReportResponse>;
