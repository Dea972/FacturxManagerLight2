using FacturXManager.Modules.Transmission.Domain.Transmissions;
using FacturXManager.Modules.Transmission.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using DomainTransmission = FacturXManager.Modules.Transmission.Domain.Transmissions.Transmission;

namespace FacturXManager.Modules.Transmission.Infrastructure.Transmissions;

internal sealed class TransmissionRepository(TransmissionDbContext context) : ITransmissionRepository
{
    public async Task<DomainTransmission?> GetAsync(Guid id, CancellationToken cancellationToken = default) =>
        await context.Transmissions
            .Include(t => t.Attempts)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public void Insert(DomainTransmission transmission) => context.Transmissions.Add(transmission);
}
