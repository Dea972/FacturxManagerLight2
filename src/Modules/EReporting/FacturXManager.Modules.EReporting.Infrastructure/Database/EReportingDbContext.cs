using FacturXManager.Modules.EReporting.Application.Abstractions.Data;
using FacturXManager.Modules.EReporting.Domain.Declarations;
using FacturXManager.Modules.EReporting.Infrastructure.Declarations;
using Microsoft.EntityFrameworkCore;

namespace FacturXManager.Modules.EReporting.Infrastructure.Database;

public sealed class EReportingDbContext(DbContextOptions<EReportingDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<Declaration> Declarations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.EReporting);

        modelBuilder.ApplyConfiguration(new DeclarationConfiguration());
        modelBuilder.ApplyConfiguration(new DeclarationEntryConfiguration());
    }
}
