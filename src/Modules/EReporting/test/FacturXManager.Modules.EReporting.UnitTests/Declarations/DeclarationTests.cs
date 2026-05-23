using FacturXManager.Common.Domain;
using FacturXManager.Modules.EReporting.Domain.Declarations;
using FacturXManager.Modules.EReporting.UnitTests.Abstractions;
using FluentAssertions;

namespace FacturXManager.Modules.EReporting.UnitTests.Declarations;

public class DeclarationTests : BaseTest
{
    private static readonly DateTime Now = new(2026, 10, 5, 9, 0, 0, DateTimeKind.Utc);

    private static Declaration NewDraft(TransactionType type = TransactionType.B2C) =>
        Declaration.Create(ReportingPeriod.Create(2026, 9).Value, type, Now);

    [Fact]
    public void Create_ShouldReturnDraftStatus_AndEmptyEntries()
    {
        // Act
        Declaration declaration = NewDraft();

        // Assert
        declaration.Status.Should().Be(DeclarationStatus.Draft);
        declaration.Entries.Should().BeEmpty();
        declaration.TotalTaxableAmount.Should().Be(0m);
        declaration.TotalVatAmount.Should().Be(0m);
    }

    [Fact]
    public void AddEntry_ShouldAccumulate_TaxableAndVatAmounts()
    {
        // Arrange
        Declaration declaration = NewDraft();

        // Act
        declaration.AddEntry(100m, 20m, "FR", OperationNature.Goods);
        declaration.AddEntry(50m, 10m, "FR", OperationNature.Services);

        // Assert
        declaration.Entries.Should().HaveCount(2);
        declaration.TotalTaxableAmount.Should().Be(150m);
        declaration.TotalVatAmount.Should().Be(30m);
    }

    [Fact]
    public void AddEntry_ShouldFail_WhenAmountIsZeroOrNegative()
    {
        // Arrange
        Declaration declaration = NewDraft();

        // Act
        Result result = declaration.AddEntry(0m, 0m, "FR", OperationNature.Goods);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DeclarationErrors.EntryAmountInvalid);
    }

    [Fact]
    public void AddEntry_ShouldFail_WhenCountryIsEmpty()
    {
        // Arrange
        Declaration declaration = NewDraft();

        // Act
        Result result = declaration.AddEntry(100m, 20m, "", OperationNature.Goods);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DeclarationErrors.CountryRequired);
    }

    [Fact]
    public void AddEntry_ShouldFail_WhenDeclarationIsFinalized()
    {
        // Arrange
        Declaration declaration = NewDraft();
        declaration.AddEntry(100m, 20m, "FR", OperationNature.Goods);
        declaration.Finalize();

        // Act
        Result result = declaration.AddEntry(50m, 10m, "FR", OperationNature.Services);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DeclarationErrors.NotEditable);
    }

    [Fact]
    public void Finalize_ShouldFail_WhenNoEntries()
    {
        // Arrange : règle métier — une déclaration vide ne peut pas être finalisée
        Declaration declaration = NewDraft();

        // Act
        Result result = declaration.Finalize();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DeclarationErrors.CannotFinalizeEmpty);
    }

    [Fact]
    public void Finalize_ShouldTransitionToFinalized()
    {
        // Arrange
        Declaration declaration = NewDraft();
        declaration.AddEntry(100m, 20m, "FR", OperationNature.Goods);

        // Act
        Result result = declaration.Finalize();

        // Assert
        result.IsSuccess.Should().BeTrue();
        declaration.Status.Should().Be(DeclarationStatus.Finalized);
    }

    [Fact]
    public void MarkSubmitted_ShouldFail_WhenNotFinalized()
    {
        // Arrange
        Declaration declaration = NewDraft();
        declaration.AddEntry(100m, 20m, "FR", OperationNature.Goods);

        // Act : encore en Draft
        Result result = declaration.MarkSubmitted("DGFIP-REF-1", Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DeclarationErrors.NotFinalized);
    }

    [Fact]
    public void MarkSubmitted_ShouldTransitionAndRecordSubmissionRef()
    {
        // Arrange
        Declaration declaration = NewDraft();
        declaration.AddEntry(100m, 20m, "FR", OperationNature.Goods);
        declaration.Finalize();

        // Act
        Result result = declaration.MarkSubmitted("DGFIP-REF-42", Now);

        // Assert
        result.IsSuccess.Should().BeTrue();
        declaration.Status.Should().Be(DeclarationStatus.Submitted);
        declaration.DgfipSubmissionReference.Should().Be("DGFIP-REF-42");
        declaration.SubmittedOnUtc.Should().Be(Now);
    }

    [Fact]
    public void MarkAccepted_ShouldFail_WhenNotSubmitted()
    {
        // Arrange
        Declaration declaration = NewDraft();
        declaration.AddEntry(100m, 20m, "FR", OperationNature.Goods);
        declaration.Finalize();

        // Act
        Result result = declaration.MarkAccepted(Now);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DeclarationErrors.NotSubmitted);
    }

    [Fact]
    public void MarkAccepted_ShouldTransitionToAccepted()
    {
        // Arrange
        Declaration declaration = NewDraft();
        declaration.AddEntry(100m, 20m, "FR", OperationNature.Goods);
        declaration.Finalize();
        declaration.MarkSubmitted("DGFIP-REF-1", Now);

        // Act
        Result result = declaration.MarkAccepted(Now.AddHours(1));

        // Assert
        result.IsSuccess.Should().BeTrue();
        declaration.Status.Should().Be(DeclarationStatus.Accepted);
        declaration.AcceptedOnUtc.Should().Be(Now.AddHours(1));
    }

    [Fact]
    public void Totals_ShouldRound_TwoDecimals()
    {
        // Arrange
        Declaration declaration = NewDraft();
        declaration.AddEntry(33.333m, 6.666m, "FR", OperationNature.Goods);

        // Assert : valeurs arrondies à la 2e décimale dès l'ajout
        declaration.TotalTaxableAmount.Should().Be(33.33m);
        declaration.TotalVatAmount.Should().Be(6.67m);
    }
}
