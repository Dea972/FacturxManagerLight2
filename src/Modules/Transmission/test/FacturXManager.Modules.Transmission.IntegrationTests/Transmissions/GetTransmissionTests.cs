using FacturXManager.Common.Application.Transmission;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Transmission.Application.Transmissions.GetTransmission;
using FacturXManager.Modules.Transmission.Application.Transmissions.SubmitTransmission;
using FacturXManager.Modules.Transmission.IntegrationTests.Abstractions;
using FacturXManager.Modules.Transmission.IntegrationTests.Abstractions.Builders;
using FluentAssertions;
using NSubstitute;

namespace FacturXManager.Modules.Transmission.IntegrationTests.Transmissions;

public class GetTransmissionTests : BaseIntegrationTest
{
    public GetTransmissionTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenTransmissionDoesNotExist()
    {
        // Arrange : aucune transmission n'a été enregistrée
        Guid unknownId = Guid.NewGuid();

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
        // Arrange : on dépose d'abord, puis on relit
        Transmitter.TransmitAsync(Arg.Any<InvoiceTransmissionRequest>(), Arg.Any<CancellationToken>())
            .Returns(InvoiceTransmissionResult.Submitted("DEPOT-XYZ", "OK"));

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
        // Arrange : la plateforme rejette
        Transmitter.TransmitAsync(Arg.Any<InvoiceTransmissionRequest>(), Arg.Any<CancellationToken>())
            .Returns(InvoiceTransmissionResult.Rejected("Code retour 12 : SIRET inconnu"));

        Result<SubmitTransmissionSummary> submission = await Sender.Send(
            new SubmitTransmissionCommandBuilder().Build());

        // Le dépôt est en échec mais la transmission est persistée pour traçabilité
        Guid transmissionId = Transmissions.Keys.Single();

        // Act
        Result<TransmissionResponse> result = await Sender.Send(new GetTransmissionQuery(transmissionId));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Rejected");
        result.Value.RejectionReason.Should().Contain("SIRET inconnu");
    }
}
