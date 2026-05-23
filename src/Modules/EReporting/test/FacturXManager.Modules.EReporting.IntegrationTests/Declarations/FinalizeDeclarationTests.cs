using FacturXManager.Common.Domain;
using FacturXManager.Modules.EReporting.Application.Declarations.FinalizeDeclaration;
using FacturXManager.Modules.EReporting.Domain.Declarations;
using FacturXManager.Modules.EReporting.IntegrationTests.Abstractions;
using FluentAssertions;

namespace FacturXManager.Modules.EReporting.IntegrationTests.Declarations;

public class FinalizeDeclarationTests : BaseIntegrationTest
{
    public FinalizeDeclarationTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Handle_ShouldFailWithBusinessError_WhenDeclarationIsEmpty()
    {
        // Arrange : règle métier — pas de finalisation sur déclaration vide
        Guid id = await Sender.CreateDeclarationAsync();

        // Act
        Result result = await Sender.Send(new FinalizeDeclarationCommand(id));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EReporting.CannotFinalizeEmpty");
    }

    [Fact]
    public async Task Handle_ShouldTransitionToFinalized_WhenDeclarationHasEntries()
    {
        // Arrange
        Guid id = await Sender.CreateDeclarationAsync();
        await Sender.AddEntryAsync(id);

        // Act
        Result result = await Sender.Send(new FinalizeDeclarationCommand(id));

        // Assert
        result.IsSuccess.Should().BeTrue();
        Declarations[id].Status.Should().Be(DeclarationStatus.Finalized);
    }

    [Fact]
    public async Task Handle_ShouldFailWithNotFound_WhenDeclarationDoesNotExist()
    {
        // Act
        Result result = await Sender.Send(new FinalizeDeclarationCommand(Guid.NewGuid()));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenDeclarationAlreadyFinalized()
    {
        // Arrange
        Guid id = await Sender.CreateDeclarationAsync();
        await Sender.AddEntryAsync(id);
        await Sender.FinalizeAsync(id);

        // Act
        Result result = await Sender.Send(new FinalizeDeclarationCommand(id));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EReporting.NotEditable");
    }
}
