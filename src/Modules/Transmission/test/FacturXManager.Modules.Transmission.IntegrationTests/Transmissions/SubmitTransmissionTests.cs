using FacturXManager.Common.Application.Transmission;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Transmission.Application.Transmissions.SubmitTransmission;
using FacturXManager.Modules.Transmission.IntegrationTests.Abstractions;
using FacturXManager.Modules.Transmission.IntegrationTests.Abstractions.Builders;
using FluentAssertions;
using NSubstitute;

namespace FacturXManager.Modules.Transmission.IntegrationTests.Transmissions;

public class SubmitTransmissionTests : BaseIntegrationTest
{
    public SubmitTransmissionTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Handle_ShouldReturnSubmittedSummary_WhenDepositSucceeds()
    {
        // Arrange
        Transmitter.TransmitAsync(Arg.Any<InvoiceTransmissionRequest>(), Arg.Any<CancellationToken>())
            .Returns(InvoiceTransmissionResult.Submitted("CHORUS-DEPOT-42", "Dépôt accepté"));

        SubmitTransmissionCommand command = new SubmitTransmissionCommandBuilder().Build();

        // Act
        Result<SubmitTransmissionSummary> result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Submitted");
        result.Value.ChannelDepositId.Should().Be("CHORUS-DEPOT-42");
        await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldFailWithRejected_When4xxFromChorusPro()
    {
        // Arrange : la plateforme renvoie un rejet métier (SIRET destinataire invalide par exemple)
        Transmitter.TransmitAsync(Arg.Any<InvoiceTransmissionRequest>(), Arg.Any<CancellationToken>())
            .Returns(InvoiceTransmissionResult.Rejected("SIRET destinataire inconnu"));

        SubmitTransmissionCommand command = new SubmitTransmissionCommandBuilder().Build();

        // Act
        Result<SubmitTransmissionSummary> result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Transmission.Rejected");
        result.Error.Description.Should().Contain("SIRET destinataire inconnu");
        result.Error.Type.Should().Be(ErrorType.Problem);
    }

    [Fact]
    public async Task Handle_ShouldFailWithExternalService_When5xxFromChorusPro()
    {
        // Arrange : erreur technique côté plateforme
        Transmitter.TransmitAsync(Arg.Any<InvoiceTransmissionRequest>(), Arg.Any<CancellationToken>())
            .Returns(InvoiceTransmissionResult.TechnicalFailure("HTTP 503 indisponible"));

        SubmitTransmissionCommand command = new SubmitTransmissionCommandBuilder().Build();

        // Act
        Result<SubmitTransmissionSummary> result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Transmission.ExternalService");
        result.Error.Type.Should().Be(ErrorType.ExternalService);
    }

    [Fact]
    public async Task Handle_ShouldFailWithTimeout_WhenChorusProTimesOut()
    {
        // Arrange
        Transmitter.TransmitAsync(Arg.Any<InvoiceTransmissionRequest>(), Arg.Any<CancellationToken>())
            .Returns(InvoiceTransmissionResult.Timeout("La requête PISTE a dépassé 30s"));

        SubmitTransmissionCommand command = new SubmitTransmissionCommandBuilder().Build();

        // Act
        Result<SubmitTransmissionSummary> result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Transmission.Timeout");
        result.Error.Type.Should().Be(ErrorType.Timeout);
    }

    [Fact]
    public async Task Handle_ShouldFailWithInvalidChannel_WhenChannelStringIsUnknown()
    {
        // Arrange
        SubmitTransmissionCommand command = new SubmitTransmissionCommandBuilder()
            .WithChannel("MARTIAN")
            .Build();

        // Act
        Result<SubmitTransmissionSummary> result = await Sender.Send(command);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Transmission.InvalidChannel");
        await Transmitter.DidNotReceiveWithAnyArgs().TransmitAsync(default!, default);
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
    public async Task Handle_ShouldRouteThroughIInvoiceTransmitter_WithCorrectChannel()
    {
        // Arrange
        Transmitter.TransmitAsync(Arg.Any<InvoiceTransmissionRequest>(), Arg.Any<CancellationToken>())
            .Returns(InvoiceTransmissionResult.Submitted("DEPOT-1"));
        SubmitTransmissionCommand command = new SubmitTransmissionCommandBuilder().WithChannel("ChorusPro").Build();

        // Act
        await Sender.Send(command);

        // Assert
        await Transmitter.Received(1).TransmitAsync(
            Arg.Is<InvoiceTransmissionRequest>(r => r.Channel == TransmissionChannel.ChorusPro),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldPersistTraceability_EvenWhenChorusProRejects()
    {
        // Arrange : on garantit que chaque tentative est tracée (cf. règles métier)
        Transmitter.TransmitAsync(Arg.Any<InvoiceTransmissionRequest>(), Arg.Any<CancellationToken>())
            .Returns(InvoiceTransmissionResult.Rejected("motif"));
        SubmitTransmissionCommand command = new SubmitTransmissionCommandBuilder().Build();

        // Act
        await Sender.Send(command);

        // Assert : insertion + commit ont bien été appelés malgré le rejet métier
        TransmissionRepository.Received(1).Insert(Arg.Any<Domain.Transmissions.Transmission>());
        await UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldAcceptOtherChannel_PdpOrPpf()
    {
        // Arrange : modèle en Y — un autre canal doit être routé sans toucher l'Application
        Transmitter.TransmitAsync(Arg.Any<InvoiceTransmissionRequest>(), Arg.Any<CancellationToken>())
            .Returns(InvoiceTransmissionResult.Submitted("PDP-DEPOT-1"));
        SubmitTransmissionCommand command = new SubmitTransmissionCommandBuilder()
            .WithChannel("Pdp")
            .Build();

        // Act
        Result<SubmitTransmissionSummary> result = await Sender.Send(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await Transmitter.Received(1).TransmitAsync(
            Arg.Is<InvoiceTransmissionRequest>(r => r.Channel == TransmissionChannel.Pdp),
            Arg.Any<CancellationToken>());
    }
}
