using FacturXManager.Modules.Compliance.Domain.Common;
using FacturXManager.Modules.Compliance.UnitTests.Abstractions;
using FluentAssertions;

namespace FacturXManager.Modules.Compliance.UnitTests.Invoicing;

public class PostalAddressTests : BaseTest
{
    [Fact]
    public void Create_ShouldUpperCaseCountryCode()
    {
        // Act
        var address = PostalAddress.Create("1 rue", "75001", "Paris", "fr");

        // Assert
        address.CountryCode.Should().Be("FR");
    }

    [Fact]
    public void Equality_ShouldBeStructural()
    {
        // Arrange
        var a = PostalAddress.Create("1 rue", "75001", "Paris", "FR");
        var b = PostalAddress.Create("1 rue", "75001", "Paris", "FR");
        var c = PostalAddress.Create("2 rue", "75001", "Paris", "FR");

        // Assert
        a.Should().Be(b);
        a.Should().NotBe(c);
    }
}
