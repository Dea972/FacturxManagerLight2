using System.Text.Json.Serialization;

namespace FacturXManager.Modules.Directory.Infrastructure.Sirene.Dto;

internal sealed class SireneEtablissementDto
{
    [JsonPropertyName("etablissement")]
    public SireneEtablissement? Etablissement { get; init; }
}

internal sealed class SireneEtablissement
{
    [JsonPropertyName("siret")]
    public string? Siret { get; init; }

    [JsonPropertyName("siren")]
    public string? Siren { get; init; }

    [JsonPropertyName("uniteLegale")]
    public SireneUniteLegale? UniteLegale { get; init; }

    [JsonPropertyName("periodesEtablissement")]
    public IReadOnlyList<SirenePeriodeEtablissement>? Periodes { get; init; }
}

internal sealed class SirenePeriodeEtablissement
{
    [JsonPropertyName("dateFin")]
    public string? DateFin { get; init; }

    [JsonPropertyName("etatAdministratifEtablissement")]
    public string? EtatAdministratif { get; init; }
}
