using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.Compliance.Domain.Invoicing;

/// <summary>
/// Partie d'une facture (vendeur BG-4 ou acheteur BG-7).
/// </summary>
public sealed class Party : ValueObject
{
    private Party(string name, string? legalRegistrationId, string? vatIdentifier, PostalAddress address)
    {
        Name = name;
        LegalRegistrationId = legalRegistrationId;
        VatIdentifier = vatIdentifier;
        Address = address;
    }

    public string Name { get; }

    /// <summary>Identifiant légal SIREN (9 chiffres) ou SIRET (14 chiffres).</summary>
    public string? LegalRegistrationId { get; }

    /// <summary>Numéro de TVA intracommunautaire (ex : FR12345678901).</summary>
    public string? VatIdentifier { get; }

    public PostalAddress Address { get; }

    public bool HasLegalRegistrationId => !string.IsNullOrWhiteSpace(LegalRegistrationId);

    public static Party Create(
        string name,
        PostalAddress address,
        string? legalRegistrationId = null,
        string? vatIdentifier = null) =>
        new(
            name?.Trim() ?? string.Empty,
            string.IsNullOrWhiteSpace(legalRegistrationId) ? null : legalRegistrationId.Trim(),
            string.IsNullOrWhiteSpace(vatIdentifier) ? null : vatIdentifier.Trim().ToUpperInvariant(),
            address);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
        yield return LegalRegistrationId;
        yield return VatIdentifier;
        yield return Address;
    }
}
