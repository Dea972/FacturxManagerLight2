using FacturXManager.Modules.Compliance.Application.Abstractions.Data;
using FacturXManager.Modules.Compliance.Domain.CreditNotes;
using FacturXManager.Modules.Compliance.Domain.Invoices;
using FacturXManager.Modules.Compliance.Infrastructure.CreditNotes;
using FacturXManager.Modules.Compliance.Infrastructure.Invoices;
using Microsoft.EntityFrameworkCore;

namespace FacturXManager.Modules.Compliance.Infrastructure.Database;

public sealed class ComplianceDbContext(DbContextOptions<ComplianceDbContext> options)
    : DbContext(options), IUnitOfWork
{
    internal DbSet<Invoice> Invoices { get; set; }

    internal DbSet<CreditNote> CreditNotes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Compliance);

        modelBuilder.ApplyConfiguration(new InvoiceConfiguration());
        modelBuilder.ApplyConfiguration(new CreditNoteConfiguration());
    }
}
