namespace FacturXManager.Modules.Directory.Application.Structures.ConsultChorusProStructure;

public sealed record ChorusProStructureDetailResponse(
    long IdStructureCpp,
    string? Siret,
    string? RaisonSociale,
    string? CodeStatut,
    string? LibStatut,
    string? IdentifiantTechnique,
    string? TypeIdentifiantStructure,
    string? CodeServiceDlin,
    string? LibServiceDlin,
    int NombreServices);
