using FacturXManager.Common.Domain;
using FacturXManager.Modules.Directory.Domain.Companies;
using FacturXManager.Modules.Directory.Infrastructure.Sirene.Dto;

namespace FacturXManager.Modules.Directory.Infrastructure.Sirene;

/// <summary>
/// Projection des DTO SIRENE vers les value objects du Domain.
/// Encapsule les règles de composition (personne physique / morale), de diffusabilité
/// et la sélection de la période courante (<c>dateFin == null</c>).
/// </summary>
internal static class SireneCompanyMapper
{
    private const string DiffusionPublic = "O";
    private const string EtatActif = "A";

    internal static Result<CompanyDirectoryEntry> ToDirectoryEntry(SireneUniteLegale? uniteLegale, string fallbackSiren)
    {
        if (uniteLegale is null)
        {
            return Result.Failure<CompanyDirectoryEntry>(CompanyErrors.NotFound(fallbackSiren));
        }

        Result<Siren> sirenResult = Siren.Create(uniteLegale.Siren ?? fallbackSiren);
        if (sirenResult.IsFailure)
        {
            return Result.Failure<CompanyDirectoryEntry>(sirenResult.Error);
        }

        bool isPublic = string.Equals(uniteLegale.StatutDiffusion, DiffusionPublic, StringComparison.OrdinalIgnoreCase);

        return CompanyDirectoryEntry.Create(
            sirenResult.Value,
            siret: null,
            legalName: isPublic ? ComposeLegalName(uniteLegale) : null,
            status: ResolveStatus(uniteLegale),
            isPubliclyDisclosed: isPublic,
            source: DirectorySource.Sirene);
    }

    internal static Result<CompanyDirectoryEntry> ToDirectoryEntry(SireneEtablissement? etablissement, string fallbackSiret)
    {
        if (etablissement is null)
        {
            return Result.Failure<CompanyDirectoryEntry>(CompanyErrors.NotFound(fallbackSiret));
        }

        Result<Siret> siretResult = Siret.Create(etablissement.Siret ?? fallbackSiret);
        if (siretResult.IsFailure)
        {
            return Result.Failure<CompanyDirectoryEntry>(siretResult.Error);
        }

        SireneUniteLegale? uniteLegale = etablissement.UniteLegale;

        bool isPublic = uniteLegale is null ||
            string.Equals(uniteLegale.StatutDiffusion, DiffusionPublic, StringComparison.OrdinalIgnoreCase);

        return CompanyDirectoryEntry.Create(
            siretResult.Value.ToSiren(),
            siretResult.Value,
            legalName: isPublic ? ComposeLegalName(uniteLegale) : null,
            status: ResolveStatus(etablissement),
            isPubliclyDisclosed: isPublic,
            source: DirectorySource.Sirene);
    }

    private static string? ComposeLegalName(SireneUniteLegale? uniteLegale)
    {
        if (uniteLegale is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(uniteLegale.Denomination))
        {
            return uniteLegale.Denomination.Trim();
        }


        if (!string.IsNullOrWhiteSpace(uniteLegale.Periodes?[0]?.DenominationUniteLegale))
        {
            return uniteLegale.Periodes?[0]?.DenominationUniteLegale.Trim();
        }

        // Personne physique (entrepreneur individuel) : prénom + nom (d'usage le cas échéant).
        string? lastName = string.IsNullOrWhiteSpace(uniteLegale.NomUsage) ? uniteLegale.Nom : uniteLegale.NomUsage;

        if (string.IsNullOrWhiteSpace(uniteLegale.PrenomUsuel) && string.IsNullOrWhiteSpace(lastName) )
        {
            return null;
        }

        return $"{uniteLegale.PrenomUsuel} {lastName}".Trim();
    }

    private static CompanyLegalStatus ResolveStatus(SireneUniteLegale uniteLegale) =>
        ResolveStatus(uniteLegale.Periodes?.FirstOrDefault(p => p.DateFin is null)?.EtatAdministratif);

    private static CompanyLegalStatus ResolveStatus(SireneEtablissement etablissement) =>
        ResolveStatus(etablissement.Periodes?.FirstOrDefault(p => p.DateFin is null)?.EtatAdministratif);

    private static CompanyLegalStatus ResolveStatus(string? code) => code switch
    {
        EtatActif => CompanyLegalStatus.Active,
        null or "" => CompanyLegalStatus.Unknown,
        _ => CompanyLegalStatus.Ceased,
    };
}
