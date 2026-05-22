using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.EReporting.Domain.Declarations;

public static class DeclarationErrors
{
    public static Error InvalidPeriodYear(int year) =>
        Error.Problem("EReporting.InvalidPeriodYear", $"L'année de période '{year}' est invalide.");

    public static Error InvalidPeriodMonth(int month) =>
        Error.Problem("EReporting.InvalidPeriodMonth", $"Le mois de période '{month}' est invalide.");

    public static Error InvalidTransactionType(string value) =>
        Error.Problem("EReporting.InvalidTransactionType", $"Le type de transaction '{value}' est invalide.");

    public static Error InvalidOperationNature(string value) =>
        Error.Problem("EReporting.InvalidOperationNature", $"La nature d'opération '{value}' est invalide.");

    public static Error NotFound(Guid declarationId) =>
        Error.NotFound("EReporting.NotFound", $"La déclaration {declarationId} est introuvable.");

    public static readonly Error EntryAmountInvalid =
        Error.Problem("EReporting.EntryAmountInvalid", "Le montant HT d'une transaction doit être positif.");

    public static readonly Error CountryRequired =
        Error.Problem("EReporting.CountryRequired", "Le pays de la transaction est obligatoire.");

    public static readonly Error NotEditable =
        Error.Conflict(
            "EReporting.NotEditable",
            "La déclaration n'est plus modifiable : elle doit être au statut Brouillon.");

    public static readonly Error CannotFinalizeEmpty =
        Error.Problem(
            "EReporting.CannotFinalizeEmpty",
            "Une déclaration sans transaction ne peut pas être finalisée.");

    public static readonly Error NotFinalized =
        Error.Conflict(
            "EReporting.NotFinalized",
            "La déclaration doit être finalisée avant d'être soumise à la DGFiP.");

    public static readonly Error NotSubmitted =
        Error.Conflict("EReporting.NotSubmitted", "La déclaration n'est pas au statut Soumise.");

    public static readonly Error DgfipClientNotConfigured =
        Error.Problem(
            "EReporting.DgfipClientNotConfigured",
            "Aucun client de soumission DGFiP n'est configuré. Enregistrez une implémentation de IDgfipReportingClient.");
}
