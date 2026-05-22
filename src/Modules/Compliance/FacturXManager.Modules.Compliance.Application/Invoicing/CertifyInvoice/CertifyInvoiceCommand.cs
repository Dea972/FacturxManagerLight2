using FacturXManager.Common.Application.Messaging;
using FacturXManager.Modules.Compliance.Application.Abstractions;

namespace FacturXManager.Modules.Compliance.Application.Invoicing.CertifyInvoice;

/// <summary>
/// Certifie une facture : valide EN16931, génère le XML CII et le PDF/A-3b embarquant factur-x.xml.
/// </summary>
public sealed record CertifyInvoiceCommand(InvoiceModel Invoice) : ICommand<CertifyInvoiceSummary>;
