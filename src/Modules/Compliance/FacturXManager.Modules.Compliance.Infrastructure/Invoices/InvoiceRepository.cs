using FacturXManager.Modules.Compliance.Domain.Common;
using FacturXManager.Modules.Compliance.Domain.Invoices;
using FacturXManager.Modules.Compliance.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace FacturXManager.Modules.Compliance.Infrastructure.Invoices;

internal sealed class InvoiceRepository(ComplianceDbContext context) : IInvoiceRepository
{
    public async Task<Invoice?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Invoices
            .Include(i => i.Lines)
            .Include(i => i.VatBreakdowns)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public async Task<Invoice?> GetByNumberAsync(string number, CancellationToken cancellationToken = default) =>
        await context.Invoices
            .Include(i => i.Lines)
            .Include(i => i.VatBreakdowns)
            .FirstOrDefaultAsync(i => i.Number == number, cancellationToken);

    public async Task<IReadOnlyList<Invoice>> ListAsync(CancellationToken cancellationToken = default) =>
        await context.Invoices
            .Include(i => i.Lines)
            .Include(i => i.VatBreakdowns)
            .OrderByDescending(i => i.IssueDate)
            .ToListAsync(cancellationToken);

    public void Insert(Invoice invoice) => context.Invoices.Add(invoice);
}
