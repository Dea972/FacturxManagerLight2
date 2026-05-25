using FacturXManager.Common.Domain;
using FacturXManager.Modules.Directory.Domain.Companies;
using FacturXManager.Modules.Directory.UnitTests.Abstractions;
using FluentAssertions;

namespace FacturXManager.Modules.Directory.UnitTests.Companies;

public class SiretTests : BaseTest
{
    // SIRET valide (SIREN 542051180 - TotalEnergies + NIC 00017).
    private const string ValidSiret = "54205118000017";

    [Fact]
    public void Create_ShouldSucceed_WhenSiretIsValid()
    {
        // Act
        Result<Siret> result = Siret.Create(ValidSiret);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(ValidSiret);
        result.Value.SirenValue.Should().Be("542051180");
        result.Value.Nic.Should().Be("00017");
    }

    [Fact]
    public void Create_ShouldFail_WhenLengthIsWrong()
    {
        // Act
        Result<Siret> result = Siret.Create("123456789");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.InvalidSiretFormat);
    }

    [Fact]
    public void Create_ShouldFail_WhenChecksumIsInvalid_AndNotLaPoste()
    {
        // Arrange : 14 chiffres mais Luhn invalide et hors exception La Poste
        const string invalidLuhn = "12345678901234";

        // Act
        Result<Siret> result = Siret.Create(invalidLuhn);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.InvalidSiretChecksum);
    }

    [Fact]
    public void Create_ShouldSucceed_ForLaPosteSiret_EvenIfLuhnIsInvalid()
    {
        // Arrange : SIRET de La Poste (SIREN 356000000) — règle officielle INSEE :
        // somme des chiffres divisible par 5, Luhn non respecté.
        const string laPosteSiret = "35600000000100";

        // Act
        Result<Siret> result = Siret.Create(laPosteSiret);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.SirenValue.Should().Be("356000000");
    }

    [Fact]
    public void Create_ShouldFail_ForLaPosteFormatSiret_WhenSumNotDivisibleByFive()
    {
        // Arrange : préfixe La Poste mais somme des chiffres non divisible par 5
        const string invalidLaPosteSiret = "35600000000048";

        // Act
        Result<Siret> result = Siret.Create(invalidLaPosteSiret);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.InvalidSiretChecksum);
    }

    [Fact]
    public void ToSiren_ShouldExtractSiren()
    {
        // Arrange
        Siret siret = Siret.Create(ValidSiret).Value;

        // Act
        Siren siren = siret.ToSiren();

        // Assert
        siren.Value.Should().Be("542051180");
    }

    [Theory]
    [InlineData("542 051 180 00017")]
    [InlineData("542-051-180-00017")]
    public void Create_ShouldNormalize_WhenInputContainsSpacesOrDashes(string raw)
    {
        // Act
        Result<Siret> result = Siret.Create(raw);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(ValidSiret);
    }
}
