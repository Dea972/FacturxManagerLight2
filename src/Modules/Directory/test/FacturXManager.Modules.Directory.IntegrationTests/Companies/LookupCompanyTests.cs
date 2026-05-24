using FacturXManager.Common.Domain;
using FacturXManager.Modules.Directory.Application.Companies.LookupCompany;
using FacturXManager.Modules.Directory.Domain.Companies;
using FacturXManager.Modules.Directory.IntegrationTests.Abstractions;
using FluentAssertions;
using NSubstitute;

namespace FacturXManager.Modules.Directory.IntegrationTests.Companies;

public class LookupCompanyTests : BaseIntegrationTest
{
    private const string ValidSiren = "542051180";
    private const string ValidSiret = "54205118000017";

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
    public async Task Handle_ShouldRouteToSirenLookup_WhenIdentifierHas9Digits()
    {
        // Arrange
        Siren siren = Siren.Create(ValidSiren).Value;
        CompanyDirectoryEntry entry = CompanyDirectoryEntry.Create(
            siren,
            siret: null,
            legalName: "TOTALENERGIES SE",
            status: CompanyLegalStatus.Active,
            isPubliclyDisclosed: true,
            source: DirectorySource.Sirene);

        LookupProvider.LookupBySirenAsync(Arg.Any<Siren>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(entry));

        // Act
        Result<CompanyLookupResponse> result = await Sender.Send(new LookupCompanyQuery(ValidSiren));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Siren.Should().Be(ValidSiren);
        result.Value.Siret.Should().BeNull();
        result.Value.LegalName.Should().Be("TOTALENERGIES SE");
        result.Value.IsActive.Should().BeTrue();
        result.Value.Source.Should().Be("Sirene");

        await LookupProvider.Received(1).LookupBySirenAsync(Arg.Any<Siren>(), Arg.Any<CancellationToken>());
        await LookupProvider.DidNotReceive().LookupBySiretAsync(Arg.Any<Siret>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldRouteToSiretLookup_WhenIdentifierHas14Digits()
    {
        // Arrange
        Siret siret = Siret.Create(ValidSiret).Value;
        CompanyDirectoryEntry entry = CompanyDirectoryEntry.Create(
            siret.ToSiren(),
            siret,
            legalName: "TOTALENERGIES SE - SIEGE",
            status: CompanyLegalStatus.Active,
            isPubliclyDisclosed: true,
            source: DirectorySource.Sirene);

        LookupProvider.LookupBySiretAsync(Arg.Any<Siret>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(entry));

        // Act
        Result<CompanyLookupResponse> result = await Sender.Send(new LookupCompanyQuery(ValidSiret));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Siret.Should().Be(ValidSiret);
        result.Value.Siren.Should().Be("542051180");

        await LookupProvider.Received(1).LookupBySiretAsync(Arg.Any<Siret>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenProviderReturnsNotFound()
    {
        // Arrange
        LookupProvider.LookupBySirenAsync(Arg.Any<Siren>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<CompanyDirectoryEntry>(CompanyErrors.NotFound(ValidSiren)));

        // Act
        Result<CompanyLookupResponse> result = await Sender.Send(new LookupCompanyQuery(ValidSiren));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("Directory.Company.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldReturnInvalidChecksum_WhenSirenLuhnIsInvalid()
    {
        // Arrange : 9 chiffres mais Luhn invalide → la query échoue avant tout appel provider
        const string invalidLuhn = "123456789";

        // Act
        Result<CompanyLookupResponse> result = await Sender.Send(new LookupCompanyQuery(invalidLuhn));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Directory.Company.InvalidSirenChecksum");

        await LookupProvider.DidNotReceive().LookupBySirenAsync(Arg.Any<Siren>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldNormalizeSpacesAndDashes_BeforeLookup()
    {
        // Arrange
        Siren siren = Siren.Create(ValidSiren).Value;
        CompanyDirectoryEntry entry = CompanyDirectoryEntry.Create(
            siren,
            siret: null,
            legalName: "TOTALENERGIES SE",
            status: CompanyLegalStatus.Active,
            isPubliclyDisclosed: true,
            source: DirectorySource.Sirene);

        LookupProvider.LookupBySirenAsync(Arg.Any<Siren>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(entry));

        // Act
        Result<CompanyLookupResponse> result = await Sender.Send(new LookupCompanyQuery("542 051 180"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Siren.Should().Be(ValidSiren);
    }

    [Fact]
    public async Task Handle_ShouldExposeProtectedFlag_WhenCompanyIsNotPubliclyDisclosed()
    {
        // Arrange : RGPD — dénomination non diffusable côté SIRENE
        Siren siren = Siren.Create(ValidSiren).Value;
        CompanyDirectoryEntry entry = CompanyDirectoryEntry.Create(
            siren,
            siret: null,
            legalName: null,
            status: CompanyLegalStatus.Active,
            isPubliclyDisclosed: false,
            source: DirectorySource.Sirene);

        LookupProvider.LookupBySirenAsync(Arg.Any<Siren>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(entry));

        // Act
        Result<CompanyLookupResponse> result = await Sender.Send(new LookupCompanyQuery(ValidSiren));

        // Assert : pas une erreur, juste un flag à exposer
        result.IsSuccess.Should().BeTrue();
        result.Value.LegalName.Should().BeNull();
        result.Value.IsPubliclyDisclosed.Should().BeFalse();
    }
}
