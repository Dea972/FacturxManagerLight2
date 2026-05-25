using FacturXManager.Modules.Directory.Domain.Companies;
using FacturXManager.Modules.Directory.UnitTests.Abstractions;
using FluentAssertions;

namespace FacturXManager.Modules.Directory.UnitTests.Companies;

public class CompanyDirectoryEntryTests : BaseTest
{
    private const string ValidSiren = "542051180";
    private const string ValidSiret = "54205118000017";

    [Fact]
    public void Create_ShouldExposeActiveFlag_WhenStatusIsActive()
    {
        // Arrange
        Siren siren = Siren.Create(ValidSiren).Value;

        // Act
        var entry = CompanyDirectoryEntry.Create(
            siren,
            siret: null,
            legalName: "TOTALENERGIES SE",
            status: CompanyLegalStatus.Active,
            isPubliclyDisclosed: true,
            source: DirectorySource.Sirene);

        // Assert
        entry.IsActive.Should().BeTrue();
        entry.Source.Should().Be(DirectorySource.Sirene);
        entry.AttachedPlatformIdentifier.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldExposeInactive_WhenStatusIsCeased()
    {
        // Arrange
        Siren siren = Siren.Create(ValidSiren).Value;

        // Act
        var entry = CompanyDirectoryEntry.Create(
            siren,
            siret: null,
            legalName: "ENTREPRISE CESSEE",
            status: CompanyLegalStatus.Ceased,
            isPubliclyDisclosed: true,
            source: DirectorySource.Sirene);

        // Assert
        entry.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldAcceptNullLegalName_WhenNotPubliclyDisclosed()
    {
        // Arrange
        Siret siret = Siret.Create(ValidSiret).Value;

        // Act : RGPD — l'unité légale n'est pas diffusable, dénomination masquée
        var entry = CompanyDirectoryEntry.Create(
            siret.ToSiren(),
            siret,
            legalName: null,
            status: CompanyLegalStatus.Active,
            isPubliclyDisclosed: false,
            source: DirectorySource.Sirene);

        // Assert
        entry.LegalName.Should().BeNull();
        entry.IsPubliclyDisclosed.Should().BeFalse();
        entry.IsActive.Should().BeTrue();
        entry.Siret.Should().Be(siret);
    }
}
