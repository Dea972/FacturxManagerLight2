using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.Compliance.Domain.Invoicing;

/// <summary>
/// Adresse postale d'une partie (BG-5 / BG-8 EN16931).
/// </summary>
public sealed class PostalAddress : ValueObject
{
    private PostalAddress(string line1, string? line2, string postalCode, string city, string countryCode)
    {
        Line1 = line1;
        Line2 = line2;
        PostalCode = postalCode;
        City = city;
        CountryCode = countryCode;
    }

    public string Line1 { get; }

    public string? Line2 { get; }

    public string PostalCode { get; }

    public string City { get; }

    /// <summary>Code pays ISO 3166-1 alpha-2 (ex : FR).</summary>
    public string CountryCode { get; }

    public static PostalAddress Create(
        string line1,
        string postalCode,
        string city,
        string countryCode,
        string? line2 = null) =>
        new(
            line1?.Trim() ?? string.Empty,
            string.IsNullOrWhiteSpace(line2) ? null : line2.Trim(),
            postalCode?.Trim() ?? string.Empty,
            city?.Trim() ?? string.Empty,
            countryCode?.Trim().ToUpperInvariant() ?? string.Empty);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Line1;
        yield return Line2;
        yield return PostalCode;
        yield return City;
        yield return CountryCode;
    }
}
