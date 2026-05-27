using System.Text.Json.Serialization;

namespace FacturXManager.Modules.Directory.Infrastructure.Sirene.Dto;

internal sealed class SireneCompanyDto
{
    [JsonPropertyName("etablissement")]
    public SireneCompany? LegalUnit { get; init; }
}

internal sealed class SireneCompany
{
    [JsonPropertyName("siret")]
    public string? Siret { get; init; }

    [JsonPropertyName("siren")]
    public string? Siren { get; init; }

    [JsonPropertyName("uniteLegale")]
    public SireneLegalUnit? Company { get; init; }

    [JsonPropertyName("periodesEtablissement")]
    public IReadOnlyList<SirenePeriodCompany>? Periods { get; init; }
}

internal sealed class SirenePeriodCompany
{
    [JsonPropertyName("dateFin")]
    public string? EndDate { get; init; }

    [JsonPropertyName("etatAdministratifEtablissement")]
    public string? AdministrativeStatus { get; init; }
}
