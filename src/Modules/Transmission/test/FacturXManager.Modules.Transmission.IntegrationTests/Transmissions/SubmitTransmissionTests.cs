using FacturXManager.Common.Domain;
using FacturXManager.Modules.Transmission.Application.Transmissions.SubmitTransmission;
using FacturXManager.Modules.Transmission.IntegrationTests.Abstractions;
using FacturXManager.Modules.Transmission.IntegrationTests.Abstractions.Builders;
using FluentAssertions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using DomainTransmission = FacturXManager.Modules.Transmission.Domain.Transmissions.Transmission;

namespace FacturXManager.Modules.Transmission.IntegrationTests.Transmissions;

public class SubmitTransmissionTests : BaseIntegrationTest
{
    public SubmitTransmissionTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Handle_ShouldReturnSubmittedSummary_WhenChorusProAcceptsDeposit()
    {
        // Arrange
        WireMock
            .Given(Request.Create().WithPath("/cpro/factures/v1/deposer/flux").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"codeRetour":0,"libelle":"Dépôt accepté","numeroFluxDepot":"CHORUS-DEPOT-42"}"""));

        SubmitTransmissionCommand command = new SubmitTransmissionCommandBuilder().Build();

        // Act
        Result<SubmitTransmissionSummary> result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Submitted");
        result.Value.ChannelDepositId.Should().Be("CHORUS-DEPOT-42");
    }

    [Fact]
    public async Task Handle_ShouldFailWithRejected_WhenChorusProReturns400WithLibelle()
    {
        // Arrange : Chorus Pro rejette avec un libellé métier dans le body
        WireMock
            .Given(Request.Create().WithPath("/cpro/factures/v1/deposer/flux").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(400)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"codeRetour":1,"libelle":"SIRET destinataire inconnu de Chorus Pro"}"""));

        SubmitTransmissionCommand command = new SubmitTransmissionCommandBuilder().Build();

        // Act
        Result<SubmitTransmissionSummary> result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Transmission.Rejected");
        result.Error.Description.Should().Contain("SIRET destinataire inconnu de Chorus Pro");
        result.Error.Type.Should().Be(ErrorType.Problem);
    }

    [Fact]
    public async Task Handle_ShouldFailWithExternalService_WhenChorusProReturns503()
    {
        // Arrange : erreur technique côté plateforme
        WireMock
            .Given(Request.Create().WithPath("/cpro/factures/v1/deposer/flux").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(503)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"codeRetour":-1,"libelle":"Service temporairement indisponible"}"""));

        SubmitTransmissionCommand command = new SubmitTransmissionCommandBuilder().Build();

        // Act
        Result<SubmitTransmissionSummary> result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Transmission.ExternalService");
        result.Error.Description.Should().Contain("Service temporairement indisponible");
        result.Error.Type.Should().Be(ErrorType.ExternalService);
    }

    [Fact]
    public async Task Handle_ShouldFailWithRejected_WhenChorusProReturnsNonZeroReturnCode()
    {
        // Arrange : HTTP 200 mais codeRetour != 0 (rejet applicatif Chorus Pro)
        WireMock
            .Given(Request.Create().WithPath("/cpro/factures/v1/deposer/flux").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"codeRetour":12,"libelle":"Données de facturation invalides"}"""));

        SubmitTransmissionCommand command = new SubmitTransmissionCommandBuilder().Build();

        // Act
        Result<SubmitTransmissionSummary> result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Transmission.Rejected");
        result.Error.Description.Should().Contain("Données de facturation invalides");
    }

    [Fact]
    public async Task Handle_ShouldFailWithInvalidChannel_WhenChannelStringIsUnknown()
    {
        // Arrange : rejeté avant tout appel HTTP — aucun stub WireMock nécessaire
        SubmitTransmissionCommand command = new SubmitTransmissionCommandBuilder()
            .WithChannel("MARTIAN")
            .Build();

        // Act
        Result<SubmitTransmissionSummary> result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Transmission.InvalidChannel");
        WireMock.LogEntries.Should().NotContain(e => e.RequestMessage.Path.Contains("deposer"));
    }

    [Fact]
    public async Task Handle_ShouldFailWithValidation_WhenDocumentReferenceIsMissing()
    {
        // Arrange
        SubmitTransmissionCommand command = new SubmitTransmissionCommandBuilder()
            .WithDocumentReference("")
            .Build();

        // Act
        Result<SubmitTransmissionSummary> result = await Sender.Send(command);

        // Assert : intercepté par le ValidationPipelineBehavior
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Handle_ShouldFailWithValidation_WhenRecipientSiretHasWrongLength()
    {
        // Arrange
        SubmitTransmissionCommand command = new SubmitTransmissionCommandBuilder()
            .WithRecipientSiret("12345")
            .Build();

        // Act
        Result<SubmitTransmissionSummary> result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Handle_ShouldPersistTraceability_EvenWhenChorusProRejects()
    {
        // Arrange : rejet métier ? la transmission doit quand même être tracée en base
        WireMock
            .Given(Request.Create().WithPath("/cpro/factures/v1/deposer/flux").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(400)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"codeRetour":1,"libelle":"Rejet test traçabilité"}"""));

        SubmitTransmissionCommand command = new SubmitTransmissionCommandBuilder().Build();

        // Act
        await Sender.Send(command);

        // Assert : une ligne persiste en base malgré le rejet
        DbContext.ChangeTracker.Clear();
        bool persisted = DbContext.Set<DomainTransmission>()
            .Any(t => t.DocumentReference == command.DocumentReference);
        persisted.Should().BeTrue();
    }
}

