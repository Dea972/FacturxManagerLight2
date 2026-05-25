using System.Text.Json.Serialization;

namespace FacturXManager.Modules.Directory.Infrastructure.Sirene.Dto;

internal sealed class SireneUniteLegaleResponse
{
    [JsonPropertyName("uniteLegale")]
    public SireneUniteLegale? UniteLegale { get; init; }
}

internal sealed class SireneUniteLegale
{
    [JsonPropertyName("siren")]
    public string? Siren { get; init; }

    [JsonPropertyName("statutDiffusionUniteLegale")]
    public string? StatutDiffusion { get; init; }

    [JsonPropertyName("denominationUniteLegale")]
    public string? Denomination { get; init; }

    [JsonPropertyName("prenomUsuelUniteLegale")]
    public string? PrenomUsuel { get; init; }

    [JsonPropertyName("nomUniteLegale")]
    public string? Nom { get; init; }

    [JsonPropertyName("nomUsageUniteLegale")]
    public string? NomUsage { get; init; }

    [JsonPropertyName("periodesUniteLegale")]
    public IReadOnlyList<SirenePeriodeUniteLegale>? Periodes { get; init; }
}

internal sealed class SirenePeriodeUniteLegale
{
    [JsonPropertyName("dateFin")]
    public string? DateFin { get; init; }

    [JsonPropertyName("denominationUniteLegale")]
    public string DenominationUniteLegale { get; init; }

    [JsonPropertyName("etatAdministratifUniteLegale")]
    public string? EtatAdministratif { get; init; }
}
