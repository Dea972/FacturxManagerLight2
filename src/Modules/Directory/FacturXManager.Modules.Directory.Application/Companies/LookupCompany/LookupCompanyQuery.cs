using FacturXManager.Common.Application.Messaging;

namespace FacturXManager.Modules.Directory.Application.Companies.LookupCompany;

/// <summary>
/// Recherche l'identité d'une entreprise française à partir d'un SIREN (9 chiffres) ou d'un SIRET (14 chiffres).
/// </summary>
public sealed record LookupCompanyQuery(string Identifier) : IQuery<CompanyLookupResponse>;
