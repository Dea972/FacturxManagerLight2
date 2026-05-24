using FacturXManager.Common.Domain;
using FacturXManager.Modules.Directory.Domain.Companies;
using FacturXManager.Modules.Directory.UnitTests.Abstractions;
using FluentAssertions;

namespace FacturXManager.Modules.Directory.UnitTests.Companies;

public class SirenTests : BaseTest
{
    // Réel SIREN d'État (TotalEnergies, identifiant public) avec clé Luhn valide.
    private const string ValidSiren = "542051180";

    [Fact]
    public void Create_ShouldSucceed_WhenSirenIsValid()
    {
        // Act
        Result<Siren> result = Siren.Create(ValidSiren);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(ValidSiren);
    }

    [Fact]
    public void Create_ShouldFail_WhenInputIsEmpty()
    {
        // Act
        Result<Siren> result = Siren.Create("");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.IdentifierRequired);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("1234567890")]
    [InlineData("ABCDEFGHI")]
    public void Create_ShouldFail_WhenFormatIsInvalid(string raw)
    {
        // Act
        Result<Siren> result = Siren.Create(raw);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.InvalidSirenFormat);
    }

    [Fact]
    public void Create_ShouldFail_WhenLuhnChecksumIsInvalid()
    {
        // Arrange : 9 chiffres mais Luhn non valide
        const string invalidLuhn = "123456789";

        // Act
        Result<Siren> result = Siren.Create(invalidLuhn);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.InvalidSirenChecksum);
    }

    [Theory]
    [InlineData("542 051 180")]
    [InlineData("542-051-180")]
    [InlineData("  542051180  ")]
    public void Create_ShouldNormalize_WhenInputContainsSpacesOrDashes(string raw)
    {
        // Act
        Result<Siren> result = Siren.Create(raw);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(ValidSiren);
    }

    [Fact]
    public void Equality_ShouldBeStructural()
    {
        // Arrange
        Siren a = Siren.Create(ValidSiren).Value;
        Siren b = Siren.Create(ValidSiren).Value;

        // Assert
        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }
}
