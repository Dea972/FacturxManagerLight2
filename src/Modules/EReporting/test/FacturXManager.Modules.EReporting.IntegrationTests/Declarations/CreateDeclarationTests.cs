using FacturXManager.Common.Domain;
using FacturXManager.Modules.EReporting.Application.Declarations.CreateDeclaration;
using FacturXManager.Modules.EReporting.Domain.Declarations;
using FacturXManager.Modules.EReporting.IntegrationTests.Abstractions;
using FluentAssertions;

namespace FacturXManager.Modules.EReporting.IntegrationTests.Declarations;

public class CreateDeclarationTests : BaseIntegrationTest
{
    public CreateDeclarationTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Handle_ShouldCreateDeclaration_InDraftStatus()
    {
        // Act
        Result<Guid> result = await Sender.Send(new CreateDeclarationCommand(2026, 9, "B2C"));

        // Assert
        result.IsSuccess.Should().BeTrue();
        Declarations.Should().ContainKey(result.Value);
        Declarations[result.Value].Status.Should().Be(DeclarationStatus.Draft);
        Declarations[result.Value].TransactionType.Should().Be(TransactionType.B2C);
    }

    [Theory]
    [InlineData("B2C", TransactionType.B2C)]
    [InlineData("International", TransactionType.International)]
    public async Task Handle_ShouldAcceptTransactionType_ValidValues(string type, TransactionType expected)
    {
        // Act
        Result<Guid> result = await Sender.Send(new CreateDeclarationCommand(2026, 9, type));

        // Assert
        result.IsSuccess.Should().BeTrue();
        Declarations[result.Value].TransactionType.Should().Be(expected);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTransactionTypeUnknown()
    {
        // Act
        Result<Guid> result = await Sender.Send(new CreateDeclarationCommand(2026, 9, "UNKNOWN"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("EReporting.InvalidTransactionType");
    }

    [Theory]
    [InlineData(2023, 9)]
    [InlineData(2101, 9)]
    public async Task Handle_ShouldFail_WhenYearOutOfRange(int year, int month)
    {
        // Act
        Result<Guid> result = await Sender.Send(new CreateDeclarationCommand(year, month, "B2C"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(13)]
    public async Task Handle_ShouldFail_WhenMonthOutOfRange(int month)
    {
        // Act
        Result<Guid> result = await Sender.Send(new CreateDeclarationCommand(2026, month, "B2C"));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenTransactionTypeEmpty()
    {
        // Act
        Result<Guid> result = await Sender.Send(new CreateDeclarationCommand(2026, 9, ""));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }
}
