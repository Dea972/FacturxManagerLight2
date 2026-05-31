using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Domain.Invoicing;

namespace FacturXManager.Modules.Compliance.Application.Abstractions;

/// <summary>
/// Construit un agrégat <see cref="CreditNote"/> Domain à partir de sa représentation scalaire.
/// </summary>
public interface ICreditNoteBuilder
{
    Result<CreditNote> Build(CreditNoteModel model);
}
