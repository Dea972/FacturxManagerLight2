using FacturXManager.Common.Domain;
using FacturXManager.Modules.Directory.Application.Companies.LookupCompany;
using FacturXManager.Modules.Directory.IntegrationTests.Abstractions;
using FluentAssertions;

namespace FacturXManager.Modules.Directory.IntegrationTests.Companies;

/// <summary>
/// Tests d'intégration contre la vraie API SIRENE (INSEE).
/// Requiert la variable d'environnement <c>SIRENE_API_KEY</c>.
/// SIREN de référence : TotalEnergies SE (542051180) — entreprise cotée, données stables.
/// </summary>
public class LookupCompanyTests : BaseIntegrationTest
{
    /// <summary>TotalEnergies SE — SIREN connu, actif, diffusé.</summary>
    private const string ValidSiren = "442395448";

    /// <summary>Siège social TotalEnergies — SIRET connu, actif, diffusé.</summary>
    private const string ValidSiret = "44239544800057";

    public LookupCompanyTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenIdentifierIsEmpty()
    {
        // Act
        Result<CompanyLookupResponse> result = await Sender.Send(new LookupCompanyQuery(string.Empty));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Directory.Company.IdentifierRequired");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenIdentifierLengthIsNeitherSirenNorSiret()
    {
        // Act
        Result<CompanyLookupResponse> result = await Sender.Send(new LookupCompanyQuery("1234567"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Directory.Company.UnknownIdentifierFormat");
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalidChecksum_WhenSirenLuhnIsInvalid()
    {
        // Arrange : 9 chiffres avec Luhn invalide — aucun appel HTTP ne doit partir
        const string invalidLuhn = "123456789";

        // Act
        Result<CompanyLookupResponse> result = await Sender.Send(new LookupCompanyQuery(invalidLuhn));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Directory.Company.InvalidSirenChecksum");
    }

    [Fact]
    public async Task Handle_ShouldRouteToSirenLookup_WhenIdentifierHas9Digits()
    {
        // Act
        Result<CompanyLookupResponse> result = await Sender.Send(new LookupCompanyQuery(ValidSiren));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Siren.Should().Be(ValidSiren);
        result.Value.Siret.Should().BeNull();
        result.Value.LegalName.Should().NotBeNullOrWhiteSpace();
        result.Value.IsActive.Should().BeTrue();
        result.Value.Source.Should().Be("Sirene");
    }

    [Fact]
    public async Task Handle_ShouldRouteToSiretLookup_WhenIdentifierHas14Digits()
    {
        // Act
        Result<CompanyLookupResponse> result = await Sender.Send(new LookupCompanyQuery(ValidSiret));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Siret.Should().Be(ValidSiret);
        result.Value.Siren.Should().Be(ValidSiren);
        result.Value.LegalName.Should().NotBeNullOrWhiteSpace();
        result.Value.Source.Should().Be("Sirene");
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenSirenDoesNotExist()
    {
        // Arrange : SIREN Luhn-valide mais non attribué (préfixe 000 non alloué par l'INSEE)
        const string unassignedSiren = "000000000";

        // Act
        Result<CompanyLookupResponse> result = await Sender.Send(new LookupCompanyQuery(unassignedSiren));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("Directory.Company.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldNormalizeSpacesAndDashes_BeforeLookup()
    {
        // Act — saisie avec espaces, normalisée avant l'appel HTTP
        Result<CompanyLookupResponse> result = await Sender.Send(new LookupCompanyQuery("442 395 448"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Siren.Should().Be(ValidSiren);
    }
}
