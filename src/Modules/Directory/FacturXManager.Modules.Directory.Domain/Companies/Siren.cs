using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.Directory.Domain.Companies;

/// <summary>
/// Identifiant SIREN d'une unité légale française : 9 chiffres validés par l'algorithme de Luhn.
/// </summary>
public sealed class Siren : ValueObject
{
    public const int Length = 9;

    private Siren(string value) => Value = value;

    public string Value { get; }

    public static Result<Siren> Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return Result.Failure<Siren>(CompanyErrors.IdentifierRequired);
        }

        string normalized = Normalize(raw);

        if (normalized.Length != Length || !IsAllDigits(normalized))
        {
            return Result.Failure<Siren>(CompanyErrors.InvalidSirenFormat);
        }

        if (!LuhnValidator.IsValid(normalized))
        {
            return Result.Failure<Siren>(CompanyErrors.InvalidSirenChecksum);
        }

        return new Siren(normalized);
    }

    public override string ToString() => Value;

    internal static string Normalize(string raw) =>
        new(raw.Where(c => !char.IsWhiteSpace(c) && c != '-').ToArray());

    internal static bool IsAllDigits(string value)
    {
        foreach (char c in value)
        {
            if (!char.IsDigit(c))
            {
                return false;
            }
        }

        return true;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
