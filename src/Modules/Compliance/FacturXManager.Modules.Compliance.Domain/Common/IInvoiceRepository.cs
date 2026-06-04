using FacturXManager.Modules.Compliance.Domain.Invoices;

namespace FacturXManager.Modules.Compliance.Domain.Common;

/// <summary>
/// Accès en persistance à l'agrégat <see cref="Invoice"/>.
/// </summary>
public interface IInvoiceRepository
{
    Task<Invoice?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Invoice?> GetByNumberAsync(string number, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Invoice>> ListAsync(CancellationToken cancellationToken = default);

    void Insert(Invoice invoice);
}
