using FacturXManager.Common.Domain;
using ValueObjectGenerator.Attributes;

namespace FacturXManager.Modules.Directory.Domain.Companies;

/// <summary>
/// Résultat d'une consultation annuaire pour une entreprise française : read-model éphémère
/// retourné par <c>IDirectoryLookupProvider</c>. N'est pas persisté côté FacturXManager.
/// </summary>
public sealed class CompanyDirectoryEntry : ValueObject
{
    private CompanyDirectoryEntry(
        Siren siren,
        Siret? siret,
        string? legalName,
        CompanyLegalStatus status,
        bool isPubliclyDisclosed,
        DirectorySource source,
        string? attachedPlatformIdentifier)
    {
        Siren = siren;
        Siret = siret;
        LegalName = legalName;
        Status = status;
        IsPubliclyDisclosed = isPubliclyDisclosed;
        Source = source;
        AttachedPlatformIdentifier = attachedPlatformIdentifier;
    }

    public Siren Siren { get; }

    /// <summary>SIRET d'établissement quand la consultation porte sur un établissement, sinon null.</summary>
    public Siret? Siret { get; }

    /// <summary>Raison sociale (personne morale) ou nom composé (personne physique). Null si non-diffusable.</summary>
    public string? LegalName { get; }

    public CompanyLegalStatus Status { get; }

    /// <summary>
    /// Indique si l'unité légale est diffusable publiquement (RGPD : statut <c>O</c> côté INSEE).
    /// Si false, <see cref="LegalName"/> peut être masqué — ce n'est pas une erreur.
    /// </summary>
    public bool IsPubliclyDisclosed { get; }

    public DirectorySource Source { get; }

    /// <summary>
    /// Identifiant de la plateforme PPF rattachée (maille de réception), si connu.
    /// Null tant que l'annuaire PPF n'est pas branché.
    /// </summary>
    public string? AttachedPlatformIdentifier { get; }

    public bool IsActive => Status == CompanyLegalStatus.Active;

    public static CompanyDirectoryEntry Create(
        Siren siren,
        Siret? siret,
        string? legalName,
        CompanyLegalStatus status,
        bool isPubliclyDisclosed,
        DirectorySource source,
        string? attachedPlatformIdentifier = null) =>
        new(siren, siret, legalName, status, isPubliclyDisclosed, source, attachedPlatformIdentifier);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Siren;
        yield return Siret;
        yield return LegalName;
        yield return Status;
        yield return IsPubliclyDisclosed;
        yield return Source;
        yield return AttachedPlatformIdentifier;
    }
}
