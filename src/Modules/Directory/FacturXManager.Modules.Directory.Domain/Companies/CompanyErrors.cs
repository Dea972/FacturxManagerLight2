using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.Directory.Domain.Companies;

public static class CompanyErrors
{
    public static readonly Error IdentifierRequired =
        Error.Problem("Directory.Company.IdentifierRequired", "Un identifiant SIREN ou SIRET est obligatoire.");

    public static readonly Error InvalidSirenFormat =
        Error.Problem(
            "Directory.Company.InvalidSirenFormat",
            "Le SIREN doit comporter exactement 9 chiffres.");

    public static readonly Error InvalidSiretFormat =
        Error.Problem(
            "Directory.Company.InvalidSiretFormat",
            "Le SIRET doit comporter exactement 14 chiffres.");

    public static readonly Error InvalidSirenChecksum =
        Error.Problem(
            "Directory.Company.InvalidSirenChecksum",
            "La clé de contrôle Luhn du SIREN est invalide.");

    public static readonly Error InvalidSiretChecksum =
        Error.Problem(
            "Directory.Company.InvalidSiretChecksum",
            "La clé de contrôle Luhn du SIRET est invalide.");

    public static readonly Error UnknownIdentifierFormat =
        Error.Problem(
            "Directory.Company.UnknownIdentifierFormat",
            "L'identifiant fourni n'est ni un SIREN (9 chiffres) ni un SIRET (14 chiffres).");

    public static Error NotFound(string identifier) =>
        Error.NotFound(
            "Directory.Company.NotFound",
            $"Aucune entreprise n'est référencée pour l'identifiant '{identifier}'.");

    public static readonly Error LookupUnavailable =
        Error.ExternalService(
            "Directory.Company.LookupUnavailable",
            "L'annuaire des entreprises est temporairement indisponible.");

    public static readonly Error LookupTimeout =
        Error.Timeout(
            "Directory.Company.LookupTimeout",
            "Le délai d'attente de l'annuaire des entreprises a été dépassé.");
}
