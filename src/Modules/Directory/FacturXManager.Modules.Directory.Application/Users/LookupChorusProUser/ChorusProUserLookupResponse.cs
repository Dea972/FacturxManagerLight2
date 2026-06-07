namespace FacturXManager.Modules.Directory.Application.Users.LookupChorusProUser;

public sealed record ChorusProUserLookupResponse(
    string Login,
    string? Nom,
    string? Prenom,
    string? Email,
    bool Actif,
    long? IdUtilisateurCourant,
    IReadOnlyList<ChorusProUserStructureResponse> Structures);

public sealed record ChorusProUserStructureResponse(
    long IdStructureCpp,
    string? RaisonSociale,
    string? Siret);
