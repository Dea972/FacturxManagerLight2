using FacturXManager.Common.Application.Messaging;

namespace FacturXManager.Modules.Compliance.Application.Invoices.GetInvoices;

/// <summary>
/// Retourne la liste paginée des factures persistées, ordonnées par date d'émission décroissante.
/// </summary>
public sealed record GetInvoicesQuery : IQuery<List<InvoiceSummaryResponse>>;
