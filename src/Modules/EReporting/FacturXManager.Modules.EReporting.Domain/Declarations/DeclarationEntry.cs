using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.EReporting.Domain.Declarations;

/// <summary>
/// Transaction agrégée dans une déclaration : montant HT, TVA, pays et nature d'opération.
/// </summary>
public sealed class DeclarationEntry : Entity
{
    private DeclarationEntry()
    {
    }

    public Guid Id { get; private set; }

    public Guid DeclarationId { get; private set; }

    public decimal TaxableAmount { get; private set; }

    public decimal VatAmount { get; private set; }

    /// <summary>Code pays ISO 3166-1 alpha-2.</summary>
    public string CountryCode { get; private set; } = string.Empty;

    public OperationNature OperationNature { get; private set; }

    internal static DeclarationEntry Create(
        Guid declarationId,
        decimal taxableAmount,
        decimal vatAmount,
        string countryCode,
        OperationNature operationNature) =>
        new()
        {
            Id = Guid.NewGuid(),
            DeclarationId = declarationId,
            TaxableAmount = decimal.Round(taxableAmount, 2, MidpointRounding.AwayFromZero),
            VatAmount = decimal.Round(vatAmount, 2, MidpointRounding.AwayFromZero),
            CountryCode = countryCode.Trim().ToUpperInvariant(),
            OperationNature = operationNature,
        };
}
