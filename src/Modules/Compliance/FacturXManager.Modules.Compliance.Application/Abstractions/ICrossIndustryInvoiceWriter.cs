using FacturXManager.Modules.Compliance.Domain.Invoicing;

namespace FacturXManager.Modules.Compliance.Application.Abstractions;

/// <summary>
/// Sérialise une facture ou un avoir au format XML Cross Industry Invoice (CII, UN/CEFACT) conforme EN16931.
/// </summary>
public interface ICrossIndustryInvoiceWriter
{
    string Write(Invoice invoice);

    string Write(CreditNote creditNote);
}
