using FacturXManager.Common.Domain;
using FacturXManager.Modules.Transmission.Application.Transmissions.GetTransmission;
using FacturXManager.Modules.Transmission.Application.Transmissions.SubmitTransmission;
using FacturXManager.Modules.Transmission.IntegrationTests.Abstractions;
using FacturXManager.Modules.Transmission.IntegrationTests.Abstractions.Builders;
using FluentAssertions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using DomainTransmission = FacturXManager.Modules.Transmission.Domain.Transmissions.Transmission;

namespace FacturXManager.Modules.Transmission.IntegrationTests.Transmissions;

public class GetTransmissionTests : BaseIntegrationTest
{
    public GetTransmissionTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTransmissionDoesNotExist()
    {
        // Arrange : aucune transmission en base
        var unknownId = Guid.NewGuid();

        // Act
        Result<TransmissionResponse> result = await Sender.Send(new GetTransmissionQuery(unknownId));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Transmission.NotFound");
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldReturnTransmission_WithSubmittedStatus_AfterSuccessfulDeposit()
    {
        // Arrange
        WireMock
            .Given(Request.Create().WithPath("/cpro/factures/v1/deposer/flux").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"codeRetour":0,"libelle":"OK","numeroFluxDepot":"DEPOT-XYZ"}"""));

        Result<SubmitTransmissionSummary> submission = await Sender.Send(
            new SubmitTransmissionCommandBuilder().Build());
        submission.IsSuccess.Should().BeTrue();
        Guid transmissionId = submission.Value.TransmissionId;

        // Act
        Result<TransmissionResponse> result = await Sender.Send(new GetTransmissionQuery(transmissionId));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(transmissionId);
        result.Value.Status.Should().Be("Submitted");
        result.Value.ChannelDepositId.Should().Be("DEPOT-XYZ");
        result.Value.Attempts.Should().ContainSingle(a => a.Status == "Submitted");
    }

    [Fact]
    public async Task Handle_ShouldReturnTransmission_WithRejectionReason_AfterPlatformRejected()
    {
        // Arrange : HTTP 200 + codeRetour != 0 = rejet applicatif Chorus Pro
        WireMock
            .Given(Request.Create().WithPath("/cpro/factures/v1/deposer/flux").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"codeRetour":12,"libelle":"Code retour 12 : SIRET inconnu"}"""));

        SubmitTransmissionCommand command = new SubmitTransmissionCommandBuilder().Build();
        Result<SubmitTransmissionSummary> submission = await Sender.Send(command);

        // Le dépôt est en échec mais la transmission est persistée pour traçabilité
        submission.IsFailure.Should().BeTrue();
        DbContext.ChangeTracker.Clear();
        DomainTransmission? persisted = DbContext.Set<DomainTransmission>()
            .FirstOrDefault(t => t.DocumentReference == command.DocumentReference);
        persisted.Should().NotBeNull();

        // Act
        Result<TransmissionResponse> result = await Sender.Send(new GetTransmissionQuery(persisted!.Id));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Rejected");
        result.Value.RejectionReason.Should().Contain("SIRET inconnu");
    }
}
