using FacturXManager.Modules.Compliance.Domain.Invoicing;
using FacturXManager.Modules.Compliance.UnitTests.Abstractions;
using FluentAssertions;

namespace FacturXManager.Modules.Compliance.UnitTests.Invoicing;

public class PaymentDetailsTests : BaseTest
{
    [Fact]
    public void Create_ShouldNormalizeIban_StripSpacesAndUpperCase()
    {
        // Act
        var payment = PaymentDetails.Create("58", new DateOnly(2026, 10, 15), "fr76 3000 6000 0112 3456 7890 189");

        // Assert
        payment.Iban.Should().Be("FR7630006000011234567890189");
    }

    [Fact]
    public void Create_ShouldDefaultToTransferCode_WhenMeansCodeEmpty()
    {
        // Act
        var payment = PaymentDetails.Create("", new DateOnly(2026, 10, 15));

        // Assert
        payment.MeansCode.Should().Be("30");
    }

    [Fact]
    public void Create_ShouldAcceptNullIban()
    {
        // Act
        var payment = PaymentDetails.Create("58");

        // Assert
        payment.Iban.Should().BeNull();
    }
}
