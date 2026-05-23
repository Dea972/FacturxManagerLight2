using FacturXManager.Common.Domain;
using FacturXManager.Modules.EReporting.Application.Declarations.GetDeclaration;
using FacturXManager.Modules.EReporting.IntegrationTests.Abstractions;
using FluentAssertions;

namespace FacturXManager.Modules.EReporting.IntegrationTests.Declarations;

public class GetDeclarationTests : BaseIntegrationTest
{
    public GetDeclarationTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Handle_ShouldReturnNotFound_WhenDeclarationDoesNotExist()
    {
        // Act
        Result<DeclarationResponse> result = await Sender.Send(new GetDeclarationQuery(Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EReporting.NotFound");
    }

    [Fact]
    public async Task Handle_ShouldReturnDetails_WithEntries_WhenDeclarationExists()
    {
        // Arrange
        Guid id = await Sender.CreateDeclarationAsync(2026, 9, "B2C");
        await Sender.AddEntryAsync(id, 200m, 40m, "FR", "Goods");
        await Sender.AddEntryAsync(id, 100m, 20m, "DE", "Services");

        // Act
        Result<DeclarationResponse> result = await Sender.Send(new GetDeclarationQuery(id));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Period.Should().Be("2026-09");
        result.Value.TransactionType.Should().Be("B2C");
        result.Value.Status.Should().Be("Draft");
        result.Value.TotalTaxableAmount.Should().Be(300m);
        result.Value.TotalVatAmount.Should().Be(60m);
        result.Value.Entries.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ShouldReflectSubmittedState_AfterSubmitWorkflow()
    {
        // Arrange
        Guid id = await Sender.CreateDeclarationAsync();
        await Sender.AddEntryAsync(id);
        await Sender.FinalizeAsync(id);

        // Patch direct sur l'agrégat mémoire pour simuler la transition vers Submitted
        // (les tests dédiés SubmitDeclarationTests couvrent le flux DGFiP)
        Declarations[id].MarkSubmitted("DGFIP-REF-1", DateTime.UtcNow);

        // Act
        Result<DeclarationResponse> result = await Sender.Send(new GetDeclarationQuery(id));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Status.Should().Be("Submitted");
        result.Value.DgfipSubmissionReference.Should().Be("DGFIP-REF-1");
    }
}
