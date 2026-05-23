using FacturXManager.Modules.Compliance.Domain.Invoicing;
using FacturXManager.Modules.Compliance.UnitTests.Abstractions;
using FluentAssertions;

namespace FacturXManager.Modules.Compliance.UnitTests.Invoicing;

public class InvoiceLineTests : BaseTest
{
    [Fact]
    public void NetAmount_ShouldEqualQuantityTimesUnitPrice_Rounded()
    {
        // Arrange / Act
        var line = InvoiceLine.Create("1", "X", quantity: 2.5m, unitNetPrice: 33.333m, VatCategory.Standard, vatRate: 20m);

        // Assert
        line.NetAmount.Should().Be(83.33m);
    }

    [Fact]
    public void Equality_ShouldBeStructural()
    {
        // Arrange
        var a = InvoiceLine.Create("1", "X", 1m, 10m, VatCategory.Standard, 20m);
        var b = InvoiceLine.Create("1", "X", 1m, 10m, VatCategory.Standard, 20m);
        var c = InvoiceLine.Create("1", "X", 2m, 10m, VatCategory.Standard, 20m);

        // Assert
        a.Should().Be(b);
        a.Should().NotBe(c);
    }

    [Fact]
    public void Create_ShouldDefaultUnitCode_WhenNotProvided()
    {
        // Act
        var line = InvoiceLine.Create("1", "X", 1m, 10m, VatCategory.Standard, 20m);

        // Assert
        line.UnitCode.Should().Be("C62");
    }

    [Theory]
    [InlineData(VatCategory.Standard, "S")]
    [InlineData(VatCategory.ZeroRated, "Z")]
    [InlineData(VatCategory.Exempt, "E")]
    [InlineData(VatCategory.ReverseCharge, "AE")]
    [InlineData(VatCategory.IntraCommunity, "K")]
    [InlineData(VatCategory.Export, "G")]
    [InlineData(VatCategory.OutOfScope, "O")]
    public void VatCategory_ShouldMapTo_ExpectedUnclCode(VatCategory category, string expected)
    {
        // Act
        string code = category.ToUnclCode();

        // Assert
        code.Should().Be(expected);
    }
}
