using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.Compliance.Domain.Common;

/// <summary>
/// Partie d'une facture (vendeur BG-4 ou acheteur BG-7).
/// </summary>
public sealed class Party : ValueObject
{
    private Party() { }  // ← pour EF Core
    private Party(string name,   string legalOrganizationId, string? vatIdentifier, PostalAddress address, string? electronicAddress, string? electronicAddressScheme)
    {
        Name = name;
        LegalOrganizationId = legalOrganizationId;
        VatIdentifier = vatIdentifier;
        Address = address;
        ElectronicAddress = electronicAddress;
        ElectronicAddressScheme = electronicAddressScheme;
    }

    public string Name { get; }

    /// <summary>Identifiant SIREN (9 chiffres) — dérivé automatiquement du SIRET.</summary>
    public string? LegalRegistrationId => LegalOrganizationId is { Length: >= 9 } siret
        ? siret[..9]
        : null;

    /// <summary>Identifiant SIRET (14 chiffres) — identifiant de l'établissement (SpecifiedLegalOrganization).</summary>
    public string LegalOrganizationId { get; }

    /// <summary>Numéro de TVA intracommunautaire (ex : FR12345678901).</summary>
    public string? VatIdentifier { get; }

    public PostalAddress Address { get; }

    /// <summary>Adresse électronique (BT-34/BT-49) — endpoint pour le routage e-invoicing.</summary>
    public string? ElectronicAddress { get; }

    /// <summary>Scheme identifier de l'adresse électronique (ex : "EM" pour email, "0009" pour SIREN).</summary>
    public string? ElectronicAddressScheme { get; }

    public bool HasLegalRegistrationId => !string.IsNullOrWhiteSpace(LegalRegistrationId);

    public bool HasLegalOrganizationId => !string.IsNullOrWhiteSpace(LegalOrganizationId);

    public static Party Create(
        string name,
        PostalAddress address,
        string legalOrganizationId,
        string? vatIdentifier = null,
        string? electronicAddress = null,
        string? electronicAddressScheme = null) =>
        new(
            name?.Trim() ?? string.Empty,
            string.IsNullOrWhiteSpace(legalOrganizationId) ? string.Empty : legalOrganizationId.Trim(),
            string.IsNullOrWhiteSpace(vatIdentifier) ? null : vatIdentifier.Trim().ToUpperInvariant(),
            address,
            string.IsNullOrWhiteSpace(electronicAddress) ? null : electronicAddress.Trim(),
            string.IsNullOrWhiteSpace(electronicAddressScheme) ? null : electronicAddressScheme.Trim());

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Name;
        yield return LegalRegistrationId;
        yield return LegalOrganizationId;
        yield return VatIdentifier;
        yield return Address;
        yield return ElectronicAddress;
        yield return ElectronicAddressScheme;
    }
}
