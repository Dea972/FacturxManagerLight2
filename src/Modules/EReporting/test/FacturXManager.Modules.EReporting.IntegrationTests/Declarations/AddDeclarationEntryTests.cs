using FacturXManager.Common.Domain;
using FacturXManager.Modules.EReporting.Application.Declarations.AddDeclarationEntry;
using FacturXManager.Modules.EReporting.IntegrationTests.Abstractions;
using FluentAssertions;

namespace FacturXManager.Modules.EReporting.IntegrationTests.Declarations;

public class AddDeclarationEntryTests : BaseIntegrationTest
{
    public AddDeclarationEntryTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Handle_ShouldAppendEntry_ToDraftDeclaration()
    {
        // Arrange
        Guid id = await Sender.CreateDeclarationAsync();

        // Act
        Result result = await Sender.Send(new AddDeclarationEntryCommand(id, 100m, 20m, "FR", "Goods"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        Declarations[id].Entries.Should().ContainSingle();
        Declarations[id].TotalTaxableAmount.Should().Be(100m);
    }

    [Fact]
    public async Task Handle_ShouldFailWithNotFound_WhenDeclarationDoesNotExist()
    {
        // Act
        Result result = await Sender.Send(new AddDeclarationEntryCommand(Guid.NewGuid(), 100m, 20m, "FR", "Goods"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EReporting.NotFound");
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenDeclarationIsFinalized()
    {
        // Arrange
        Guid id = await Sender.CreateDeclarationAsync();
        await Sender.AddEntryAsync(id);
        await Sender.FinalizeAsync(id);

        // Act
        Result result = await Sender.Send(new AddDeclarationEntryCommand(id, 50m, 10m, "DE", "Services"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EReporting.NotEditable");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenAmountIsZero()
    {
        // Arrange
        Guid id = await Sender.CreateDeclarationAsync();

        // Act : intercepté par le validator FluentValidation
        Result result = await Sender.Send(new AddDeclarationEntryCommand(id, 0m, 0m, "FR", "Goods"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenOperationNatureUnknown()
    {
        // Arrange
        Guid id = await Sender.CreateDeclarationAsync();

        // Act
        Result result = await Sender.Send(new AddDeclarationEntryCommand(id, 100m, 20m, "FR", "Martian"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EReporting.InvalidOperationNature");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenCountryCodeWrongLength()
    {
        // Arrange
        Guid id = await Sender.CreateDeclarationAsync();

        // Act : code pays ISO 3166-1 alpha-2 ⇒ longueur 2
        Result result = await Sender.Send(new AddDeclarationEntryCommand(id, 100m, 20m, "FRA", "Goods"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }
}
