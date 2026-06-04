using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Domain.Invoices;

namespace FacturXManager.Modules.Compliance.Application.Abstractions;

/// <summary>
/// Valide une facture au regard de la norme EN16931 et des obligations réglementaires françaises.
/// Retourne un <see cref="Result"/> en échec agrégeant les violations détectées.
/// </summary>
public interface IEn16931Validator
{
    Result Validate(Invoice invoice);
}
