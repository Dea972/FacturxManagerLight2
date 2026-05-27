using System.Text.Json.Serialization;

namespace FacturXManager.Modules.Directory.Infrastructure.Sirene.Dto;

internal sealed class SireneLegalUnitDto
{
    [JsonPropertyName("uniteLegale")]
    public SireneLegalUnit? Company { get; init; }
}

internal sealed class SireneLegalUnit
{
    [JsonPropertyName("siren")]
    public string? Siren { get; init; }

    [JsonPropertyName("statutDiffusionUniteLegale")]
    public string? DisclosureStatus { get; init; }

    [JsonPropertyName("denominationUniteLegale")]
    public string? LegalName { get; init; }

    [JsonPropertyName("prenomUsuelUniteLegale")]
    public string? FirstName { get; init; }

    [JsonPropertyName("nomUniteLegale")]
    public string? LastName { get; init; }

    [JsonPropertyName("nomUsageUniteLegale")]
    public string? UsageLastName { get; init; }

    [JsonPropertyName("periodesUniteLegale")]
    public IReadOnlyList<SirenePeriodLegalUnit>? Periods { get; init; }
}

internal sealed class SirenePeriodLegalUnit
{
    [JsonPropertyName("dateFin")]
    public string? EndDate { get; init; }

    [JsonPropertyName("denominationUniteLegale")]
    public string? LegalName { get; init; }

    [JsonPropertyName("etatAdministratifUniteLegale")]
    public string? AdministrativeStatus { get; init; }
}
