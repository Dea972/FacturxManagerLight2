using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.Compliance.Domain.Invoicing;

/// <summary>
/// Modalités de paiement (BG-16 EN16931).
/// </summary>
public sealed class PaymentDetails : ValueObject
{
    private PaymentDetails(string meansCode, string? iban, DateOnly? dueDate)
    {
        MeansCode = meansCode;
        Iban = iban;
        DueDate = dueDate;
    }

    /// <summary>Code moyen de paiement UNCL4461 (ex : 58 = virement SEPA, 30 = virement).</summary>
    public string MeansCode { get; }

    public string? Iban { get; }

    public DateOnly? DueDate { get; }

    public static PaymentDetails Create(string meansCode, DateOnly? dueDate = null, string? iban = null) =>
        new(
            string.IsNullOrWhiteSpace(meansCode) ? "30" : meansCode.Trim(),
            string.IsNullOrWhiteSpace(iban) ? null : iban.Trim().Replace(" ", string.Empty, StringComparison.Ordinal).ToUpperInvariant(),
            dueDate);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return MeansCode;
        yield return Iban;
        yield return DueDate;
    }
}
