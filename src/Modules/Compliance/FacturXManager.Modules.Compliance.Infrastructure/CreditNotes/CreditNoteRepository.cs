using FacturXManager.Modules.Compliance.Domain.CreditNotes;
using FacturXManager.Modules.Compliance.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace FacturXManager.Modules.Compliance.Infrastructure.CreditNotes;

internal sealed class CreditNoteRepository(ComplianceDbContext context) : ICreditNoteRepository
{
    public async Task<CreditNote?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.CreditNotes
            .Include(c => c.Lines)
            .Include(c => c.VatBreakdowns)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<CreditNote>> ListAsync(CancellationToken cancellationToken = default) =>
        await context.CreditNotes
            .Include(c => c.Lines)
            .Include(c => c.VatBreakdowns)
            .OrderByDescending(c => c.IssueDate)
            .ToListAsync(cancellationToken);

    public void Insert(CreditNote creditNote) => context.CreditNotes.Add(creditNote);
}
