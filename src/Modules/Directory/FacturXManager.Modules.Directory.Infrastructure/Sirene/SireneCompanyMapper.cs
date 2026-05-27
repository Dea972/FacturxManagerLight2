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

    internal static Result<CompanyDirectoryEntry> ToDirectoryEntry(SireneLegalUnit? uniteLegale, string fallbackSiren)
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

        bool isPublic = string.Equals(uniteLegale.DisclosureStatus, DiffusionPublic, StringComparison.OrdinalIgnoreCase);

        return CompanyDirectoryEntry.Create(
            sirenResult.Value,
            siret: null,
            legalName: isPublic ? ComposeLegalName(uniteLegale) : null,
            status: ResolveStatus(uniteLegale),
            isPubliclyDisclosed: isPublic,
            source: DirectorySource.Sirene);
    }

    internal static Result<CompanyDirectoryEntry> ToDirectoryEntry(SireneCompany? etablissement, string fallbackSiret)
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

        SireneLegalUnit? uniteLegale = etablissement.Company;

        bool isPublic = uniteLegale is null ||
            string.Equals(uniteLegale.DisclosureStatus, DiffusionPublic, StringComparison.OrdinalIgnoreCase);

        return CompanyDirectoryEntry.Create(
            siretResult.Value.ToSiren(),
            siretResult.Value,
            legalName: isPublic ? ComposeLegalName(uniteLegale) : null,
            status: ResolveStatus(etablissement),
            isPubliclyDisclosed: isPublic,
            source: DirectorySource.Sirene);
    }

    private static string? ComposeLegalName(SireneLegalUnit? uniteLegale)
    {
        if (uniteLegale is null)
        {
            return null;
        }

        if (!string.IsNullOrWhiteSpace(uniteLegale.LegalName))
        {
            return uniteLegale.LegalName.Trim();
        }


        if (uniteLegale.Periods?[0]?.LegalName is string name && !string.IsNullOrWhiteSpace(name))
        {
            return name.Trim();
        }

        // Personne physique (entrepreneur individuel) : prénom + nom (d'usage le cas échéant).
        string? lastName = string.IsNullOrWhiteSpace(uniteLegale.UsageLastName) ? uniteLegale.LastName : uniteLegale.UsageLastName;

        if (string.IsNullOrWhiteSpace(uniteLegale.FirstName) && string.IsNullOrWhiteSpace(lastName))
        {
            return null;
        }

        return $"{uniteLegale.FirstName} {lastName}".Trim();
    }

    private static CompanyLegalStatus ResolveStatus(SireneLegalUnit uniteLegale) =>
        ResolveStatus(uniteLegale.Periods?.FirstOrDefault(p => p.EndDate is null)?.AdministrativeStatus);

    private static CompanyLegalStatus ResolveStatus(SireneCompany etablissement) =>
        ResolveStatus(etablissement.Periods?.FirstOrDefault(p => p.EndDate is null)?.AdministrativeStatus);

    private static CompanyLegalStatus ResolveStatus(string? code) => code switch
    {
        EtatActif => CompanyLegalStatus.Active,
        null or "" => CompanyLegalStatus.Unknown,
        _ => CompanyLegalStatus.Ceased,
    };
}
