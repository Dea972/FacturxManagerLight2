using FacturXManager.Common.Application.Messaging;
using FacturXManager.Modules.Compliance.Application.Abstractions.Invoices;

namespace FacturXManager.Modules.Compliance.Application.Invoices.GenerateInvoice;

/// <summary>
/// Génère une facture Factur-X : valide EN16931, génère le XML CII et le PDF/A-3b embarquant factur-x.xml.
/// </summary>
public sealed record GenerateInvoiceCommand(InvoiceModel Invoice) : ICommand<GenerateInvoiceSummary>;
