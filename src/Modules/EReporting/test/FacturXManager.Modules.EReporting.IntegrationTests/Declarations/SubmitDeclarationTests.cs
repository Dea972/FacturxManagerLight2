using FacturXManager.Common.Domain;
using FacturXManager.Modules.EReporting.Application.Abstractions;
using FacturXManager.Modules.EReporting.Application.Declarations.SubmitDeclaration;
using FacturXManager.Modules.EReporting.Domain.Declarations;
using FacturXManager.Modules.EReporting.IntegrationTests.Abstractions;
using FluentAssertions;
using NSubstitute;

namespace FacturXManager.Modules.EReporting.IntegrationTests.Declarations;

public class SubmitDeclarationTests : BaseIntegrationTest
{
    public SubmitDeclarationTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Handle_ShouldSubmitDeclarationAndReturnReference_WhenDgfipAccepts()
    {
        // Arrange
        Guid id = await Sender.CreateDeclarationAsync();
        await Sender.AddEntryAsync(id, taxableAmount: 1000m, vatAmount: 200m);
        await Sender.FinalizeAsync(id);

        DgfipClient.SubmitAsync(Arg.Any<DgfipSubmissionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new DgfipSubmissionReceipt("DGFIP-2026-09-001", "ack")));

        // Act
        Result<SubmitDeclarationSummary> result = await Sender.Send(new SubmitDeclarationCommand(id));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Submitted");
        result.Value.SubmissionReference.Should().Be("DGFIP-2026-09-001");
        Declarations[id].Status.Should().Be(DeclarationStatus.Submitted);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenDeclarationNotFinalized()
    {
        // Arrange : déclaration en Draft
        Guid id = await Sender.CreateDeclarationAsync();
        await Sender.AddEntryAsync(id);

        // Act
        Result<SubmitDeclarationSummary> result = await Sender.Send(new SubmitDeclarationCommand(id));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EReporting.NotFinalized");
        await DgfipClient.DidNotReceiveWithAnyArgs().SubmitAsync(default!, default);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenDeclarationDoesNotExist()
    {
        // Act
        Result<SubmitDeclarationSummary> result = await Sender.Send(new SubmitDeclarationCommand(Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenDgfipReturnsBusinessRejection_4xx()
    {
        // Arrange : finalisée mais DGFiP rejette pour motif métier
        Guid id = await Sender.CreateDeclarationAsync();
        await Sender.AddEntryAsync(id);
        await Sender.FinalizeAsync(id);

        DgfipClient.SubmitAsync(Arg.Any<DgfipSubmissionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<DgfipSubmissionReceipt>(
                Error.Problem("EReporting.Dgfip.Rejected", "Période déjà déclarée pour cet émetteur")));

        // Act
        Result<SubmitDeclarationSummary> result = await Sender.Send(new SubmitDeclarationCommand(id));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EReporting.Dgfip.Rejected");
        result.Error.Type.Should().Be(ErrorType.Problem);
        Declarations[id].Status.Should().Be(DeclarationStatus.Finalized); // pas de transition
    }

    [Fact]
    public async Task Handle_ShouldFail_WithExternalServiceError_WhenDgfipReturns5xx()
    {
        // Arrange
        Guid id = await Sender.CreateDeclarationAsync();
        await Sender.AddEntryAsync(id);
        await Sender.FinalizeAsync(id);

        DgfipClient.SubmitAsync(Arg.Any<DgfipSubmissionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<DgfipSubmissionReceipt>(
                Error.ExternalService("EReporting.Dgfip.Unavailable", "HTTP 503 - Service indisponible")));

        // Act
        Result<SubmitDeclarationSummary> result = await Sender.Send(new SubmitDeclarationCommand(id));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.ExternalService);
    }

    [Fact]
    public async Task Handle_ShouldFail_WithDefaultClient_WhenDgfipClientNotConfigured()
    {
        // Arrange : on simule l'implémentation par défaut (non câblée)
        Guid id = await Sender.CreateDeclarationAsync();
        await Sender.AddEntryAsync(id);
        await Sender.FinalizeAsync(id);

        DgfipClient.SubmitAsync(Arg.Any<DgfipSubmissionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<DgfipSubmissionReceipt>(DeclarationErrors.DgfipClientNotConfigured));

        // Act
        Result<SubmitDeclarationSummary> result = await Sender.Send(new SubmitDeclarationCommand(id));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EReporting.DgfipClientNotConfigured");
    }

    [Fact]
    public async Task Handle_ShouldForwardCorrectTotals_ToDgfip()
    {
        // Arrange
        Guid id = await Sender.CreateDeclarationAsync();
        await Sender.AddEntryAsync(id, taxableAmount: 100m, vatAmount: 20m);
        await Sender.AddEntryAsync(id, taxableAmount: 50m, vatAmount: 10m, countryCode: "DE");
        await Sender.FinalizeAsync(id);

        DgfipClient.SubmitAsync(Arg.Any<DgfipSubmissionRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(new DgfipSubmissionReceipt("DGFIP-REF", null)));

        // Act
        await Sender.Send(new SubmitDeclarationCommand(id));

        // Assert
        await DgfipClient.Received(1).SubmitAsync(
            Arg.Is<DgfipSubmissionRequest>(r =>
                r.TotalTaxableAmount == 150m
                && r.TotalVatAmount == 30m
                && r.EntryCount == 2
                && r.PeriodLabel == "2026-09"),
            Arg.Any<CancellationToken>());
    }
}
