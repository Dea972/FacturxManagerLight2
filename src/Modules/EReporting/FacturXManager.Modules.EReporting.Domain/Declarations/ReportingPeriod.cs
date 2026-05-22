using FacturXManager.Common.Domain;

namespace FacturXManager.Modules.EReporting.Domain.Declarations;

/// <summary>
/// Période couverte par une déclaration (mensuelle).
/// </summary>
public sealed class ReportingPeriod : ValueObject
{
    private ReportingPeriod(int year, int month)
    {
        Year = year;
        Month = month;
    }

    public int Year { get; }

    public int Month { get; }

    public string Label => $"{Year:D4}-{Month:D2}";

    public static Result<ReportingPeriod> Create(int year, int month)
    {
        if (year is < 2024 or > 2100)
        {
            return Result.Failure<ReportingPeriod>(DeclarationErrors.InvalidPeriodYear(year));
        }

        if (month is < 1 or > 12)
        {
            return Result.Failure<ReportingPeriod>(DeclarationErrors.InvalidPeriodMonth(month));
        }

        return new ReportingPeriod(year, month);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Year;
        yield return Month;
    }
}
