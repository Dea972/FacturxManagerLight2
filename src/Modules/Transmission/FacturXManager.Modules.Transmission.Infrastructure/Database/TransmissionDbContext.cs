using FacturXManager.Modules.Transmission.Application.Abstractions.Data;
using FacturXManager.Modules.Transmission.Infrastructure.Transmissions;
using Microsoft.EntityFrameworkCore;
using DomainTransmission = FacturXManager.Modules.Transmission.Domain.Transmissions.Transmission;

namespace FacturXManager.Modules.Transmission.Infrastructure.Database;

public sealed class TransmissionDbContext(DbContextOptions<TransmissionDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<DomainTransmission> Transmissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Transmission);

        modelBuilder.ApplyConfiguration(new TransmissionConfiguration());
        modelBuilder.ApplyConfiguration(new TransmissionAttemptConfiguration());
    }
}
