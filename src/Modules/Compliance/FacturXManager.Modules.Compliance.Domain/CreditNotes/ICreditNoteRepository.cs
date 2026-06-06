namespace FacturXManager.Modules.Compliance.Domain.CreditNotes;

/// <summary>
/// Accès en persistance à l'agrégat <see cref="CreditNote"/>.
/// </summary>
public interface ICreditNoteRepository
{
    Task<CreditNote?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CreditNote>> ListAsync(CancellationToken cancellationToken = default);

    void Insert(CreditNote creditNote);
}
