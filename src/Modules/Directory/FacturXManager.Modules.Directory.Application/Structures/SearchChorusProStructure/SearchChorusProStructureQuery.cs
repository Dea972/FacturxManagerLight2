using FacturXManager.Common.Application.Messaging;
using FacturXManager.Modules.Directory.Application.Companies.LookupCompany;

namespace FacturXManager.Modules.Directory.Application.Structures.SearchChorusProStructure;

/// <summary>
/// Recherche une structure Chorus Pro par SIRET et retourne ses informations de routage PPF.
/// </summary>
public sealed record SearchChorusProStructureQuery(string Siret) : IQuery<CompanyLookupResponse>;
