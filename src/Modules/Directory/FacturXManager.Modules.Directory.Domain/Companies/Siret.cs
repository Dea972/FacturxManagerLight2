using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.Directory.Domain.Companies;

/// <summary>
/// Identifiant SIRET d'un établissement français : 14 chiffres (SIREN + NIC) validés par Luhn,
/// avec l'exception officielle La Poste (SIREN 356 000 000) dont les SIRET ne respectent pas Luhn.
/// </summary>
public sealed class Siret : ValueObject
{
    public const int Length = 14;

    private const string LaPosteSirenPrefix = "356000000";

    private Siret(string value) => Value = value;

    public string Value { get; }

    public string SirenValue => Value[..Siren.Length];

    public string Nic => Value[Siren.Length..];

    public static Result<Siret> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Result.Failure<Siret>(CompanyErrors.IdentifierRequired);
        }

        string normalized = Siren.Normalize(raw);

        if (normalized.Length != Length || !Siren.IsAllDigits(normalized))
        {
            return Result.Failure<Siret>(CompanyErrors.InvalidSiretFormat);
        }

        if (!IsValidChecksum(normalized))
        {
            return Result.Failure<Siret>(CompanyErrors.InvalidSiretChecksum);
        }

        return new Siret(normalized);
    }

    /// <summary>Retourne le SIREN porté par ce SIRET (les 9 premiers chiffres).</summary>
    public Siren ToSiren() =>
        Siren.Create(SirenValue).Value;

    public override string ToString() => Value;

    private static bool IsValidChecksum(string value)
    {
        // Exception officielle INSEE : les SIRET de La Poste (SIREN 356000000)
        // ne respectent pas Luhn ; la règle est que la somme des chiffres doit être multiple de 5.
        if (value.StartsWith(LaPosteSirenPrefix, StringComparison.Ordinal))
        {
            int sum = 0;
            foreach (char c in value)
            {
                sum += c - '0';
            }

            return sum % 5 == 0;
        }

        return LuhnValidator.IsValid(value);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
