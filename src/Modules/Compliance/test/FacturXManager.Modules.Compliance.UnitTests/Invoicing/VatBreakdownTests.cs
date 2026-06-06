using FacturXManager.Modules.Compliance.Domain.Common;
using FacturXManager.Modules.Compliance.UnitTests.Abstractions;
using FluentAssertions;

namespace FacturXManager.Modules.Compliance.UnitTests.Invoicing;

public class VatBreakdownTests : BaseTest
{
    [Fact]
    public void Create_ShouldComputeTaxAmount_AsBasisTimesRate()
    {
        // Act
        var breakdown = VatBreakdown.Create(VatCategory.Standard, rate: 20m, basisAmount: 1000m);

        // Assert
        breakdown.BasisAmount.Should().Be(1000m);
        breakdown.TaxAmount.Should().Be(200m);
    }

    [Fact]
    public void Create_ShouldRoundTaxAmount_AwayFromZero()
    {
        // Act
        var breakdown = VatBreakdown.Create(VatCategory.Standard, rate: 5.5m, basisAmount: 100m);

        // Assert
        breakdown.TaxAmount.Should().Be(5.5m);
    }

    [Fact]
    public void Equality_ShouldBeStructural()
    {
        // Arrange
        var a = VatBreakdown.Create(VatCategory.Standard, 20m, 100m);
        var b = VatBreakdown.Create(VatCategory.Standard, 20m, 100m);

        // Assert
        a.Should().Be(b);
    }
}
