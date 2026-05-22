using FacturXManager.Modules.EReporting.Domain.Declarations;
using FacturXManager.Modules.EReporting.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace FacturXManager.Modules.EReporting.Infrastructure.Declarations;

internal sealed class DeclarationRepository(EReportingDbContext context) : IDeclarationRepository
{
    public async Task<Declaration?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Declarations
            .Include(d => d.Entries)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public void Insert(Declaration declaration) => context.Declarations.Add(declaration);
}
