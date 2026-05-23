using FacturXManager.Common.Domain;
using FacturXManager.Modules.EReporting.Domain.Declarations;
using FacturXManager.Modules.EReporting.UnitTests.Abstractions;
using FluentAssertions;

namespace FacturXManager.Modules.EReporting.UnitTests.Declarations;

public class ReportingPeriodTests : BaseTest
{
    [Fact]
    public void Create_ShouldReturnPeriod_WhenYearAndMonthAreValid()
    {
        // Act
        Result<ReportingPeriod> result = ReportingPeriod.Create(2026, 9);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Label.Should().Be("2026-09");
    }

    [Theory]
    [InlineData(2023)]
    [InlineData(2101)]
    public void Create_ShouldFail_WhenYearIsOutOfRange(int year)
    {
        // Act
        Result<ReportingPeriod> result = ReportingPeriod.Create(year, 1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EReporting.InvalidPeriodYear");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    [InlineData(-1)]
    public void Create_ShouldFail_WhenMonthIsOutOfRange(int month)
    {
        // Act
        Result<ReportingPeriod> result = ReportingPeriod.Create(2026, month);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EReporting.InvalidPeriodMonth");
    }

    [Fact]
    public void Label_ShouldZeroPad_MonthAndYear()
    {
        // Act
        ReportingPeriod period = ReportingPeriod.Create(2026, 3).Value;

        // Assert
        period.Label.Should().Be("2026-03");
    }

    [Fact]
    public void Equality_ShouldBeStructural()
    {
        // Arrange
        ReportingPeriod a = ReportingPeriod.Create(2026, 9).Value;
        ReportingPeriod b = ReportingPeriod.Create(2026, 9).Value;
        ReportingPeriod c = ReportingPeriod.Create(2026, 10).Value;

        // Assert
        a.Should().Be(b);
        a.Should().NotBe(c);
    }
}
