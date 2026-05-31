using FacturXManager.Common.Application.Messaging;
using FacturXManager.Modules.Compliance.Application.Abstractions;

namespace FacturXManager.Modules.Compliance.Application.Invoicing.GenerateCreditNote;

/// <summary>
/// Génère un avoir Factur-X (type 381) : valide les invariants, génère le XML CII et le PDF/A-3b.
/// </summary>
public sealed record GenerateCreditNoteCommand(CreditNoteModel CreditNote) : ICommand<GenerateCreditNoteSummary>;
