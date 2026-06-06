using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Domain.Invoices;

namespace FacturXManager.Modules.Compliance.Application.Abstractions.Invoices;

/// <summary>
/// Construit un agrégat <see cref="Invoice"/> Domain à partir de sa représentation scalaire.
/// Logique de mapping partagée entre les use cases (jamais d'appel inter-commands).
/// </summary>
public interface IInvoiceBuilder
{
    Result<Invoice> Build(InvoiceModel model);
}
