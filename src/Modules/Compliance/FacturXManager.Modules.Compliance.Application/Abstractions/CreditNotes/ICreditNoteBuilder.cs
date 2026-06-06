using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Domain.CreditNotes;

namespace FacturXManager.Modules.Compliance.Application.Abstractions.CreditNotes;

/// <summary>
/// Construit un agrégat <see cref="CreditNote"/> Domain à partir de sa représentation scalaire.
/// </summary>
public interface ICreditNoteBuilder
{
    Result<CreditNote> Build(CreditNoteModel model);
}
