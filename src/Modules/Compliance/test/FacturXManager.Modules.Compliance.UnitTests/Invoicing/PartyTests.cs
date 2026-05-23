using FacturXManager.Modules.Compliance.Domain.Invoicing;
using FacturXManager.Modules.Compliance.UnitTests.Abstractions;
using FluentAssertions;

namespace FacturXManager.Modules.Compliance.UnitTests.Invoicing;

public class PartyTests : BaseTest
{
    [Fact]
    public void HasLegalRegistrationId_ShouldReturnTrue_WhenSirenProvided()
    {
        // Arrange
        var party = Party.Create("X", InvoiceTestData.FrenchAddress(), legalRegistrationId: "552120222");

        // Assert
        party.HasLegalRegistrationId.Should().BeTrue();
    }

    [Fact]
    public void HasLegalRegistrationId_ShouldReturnFalse_WhenSirenMissing()
    {
        // Arrange
        var party = Party.Create("Y", InvoiceTestData.FrenchAddress());

        // Assert
        party.HasLegalRegistrationId.Should().BeFalse();
    }
}
