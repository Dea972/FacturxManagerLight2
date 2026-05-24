namespace FacturXManager.Modules.Directory.Domain.Companies;

/// <summary>
/// Algorithme de Luhn (modulo 10) utilisé par l'INSEE pour la clé de contrôle des SIREN et SIRET.
/// </summary>
internal static class LuhnValidator
{
    internal static bool IsValid(string digits)
    {
        int sum = 0;
        bool doubleDigit = false;

        for (int i = digits.Length - 1; i >= 0; i--)
        {
            int d = digits[i] - '0';

            if (doubleDigit)
            {
                d *= 2;
                if (d > 9)
                {
                    d -= 9;
                }
            }

            sum += d;
            doubleDigit = !doubleDigit;
        }

        return sum % 10 == 0;
    }
}
