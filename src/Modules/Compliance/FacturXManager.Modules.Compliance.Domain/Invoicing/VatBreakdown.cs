using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.Compliance.Domain.Invoicing;

/// <summary>
/// Ventilation de TVA par catégorie et par taux (BG-23 EN16931).
/// </summary>
public sealed class VatBreakdown : ValueObject
{
    private VatBreakdown(VatCategory category, decimal rate, decimal basisAmount, decimal taxAmount)
    {
        Category = category;
        Rate = rate;
        BasisAmount = basisAmount;
        TaxAmount = taxAmount;
    }

    public VatCategory Category { get; }

    public decimal Rate { get; }

    /// <summary>Base imposable HT (BT-116).</summary>
    public decimal BasisAmount { get; }

    /// <summary>Montant de TVA (BT-117).</summary>
    public decimal TaxAmount { get; }

    public static VatBreakdown Create(VatCategory category, decimal rate, decimal basisAmount) =>
        new(
            category,
            rate,
            decimal.Round(basisAmount, 2, MidpointRounding.AwayFromZero),
            decimal.Round(basisAmount * rate / 100m, 2, MidpointRounding.AwayFromZero));

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Category;
        yield return Rate;
        yield return BasisAmount;
        yield return TaxAmount;
    }
}
