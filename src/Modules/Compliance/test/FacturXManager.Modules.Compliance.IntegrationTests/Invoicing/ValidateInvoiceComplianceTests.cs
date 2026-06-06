using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions.Invoices;
using FacturXManager.Modules.Compliance.Application.Invoices.ValidateInvoiceCompliance;
using FacturXManager.Modules.Compliance.IntegrationTests.Abstractions;
using FacturXManager.Modules.Compliance.IntegrationTests.Abstractions.Builders;
using FluentAssertions;

namespace FacturXManager.Modules.Compliance.IntegrationTests.Invoicing;

public class ValidateInvoiceComplianceTests : BaseIntegrationTest
{
    public ValidateInvoiceComplianceTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Handle_ShouldReportCompliant_WhenAllRulesSatisfied()
    {
        // Arrange
        InvoiceModel model = new InvoiceModelBuilder().Build();

        // Act
        Result<ComplianceReportResponse> result = await Sender.Send(new ValidateInvoiceComplianceQuery(model));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsCompliant.Should().BeTrue(because: "facture B2B post-2026 valide");
        result.Value.Violations.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReportBuyerSirenViolation_WhenMissingAfterSept2026()
    {
        // Arrange
        InvoiceModel model = new InvoiceModelBuilder()
            .WithIssueDate(new DateOnly(2026, 9, 15))
            .WithBuyer(b => b.WithoutSiren())
            .Build();

        // Act
        Result<ComplianceReportResponse> result = await Sender.Send(new ValidateInvoiceComplianceQuery(model));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.IsCompliant.Should().BeFalse();
        result.Value.Violations.Should().Contain(v => v.Code == "Compliance.Invoice.BuyerSirenRequired");
    }

    [Fact]
    public async Task Handle_ShouldAggregateMultipleViolations()
    {
        // Arrange : facture B2B post-2026 sans SIREN acheteur ni nature d'opération
        InvoiceModel model = new InvoiceModelBuilder()
            .WithIssueDate(new DateOnly(2026, 10, 1))
            .WithBuyer(b => b.WithoutSiren())
            .WithOperationNature("Unspecified")
            .Build();

        // Act
        Result<ComplianceReportResponse> result = await Sender.Send(new ValidateInvoiceComplianceQuery(model));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Violations.Should().HaveCountGreaterThanOrEqualTo(2);
        result.Value.Violations.Should().Contain(v => v.Code == "Compliance.Invoice.BuyerSirenRequired");
        result.Value.Violations.Should().Contain(v => v.Code == "Compliance.Invoice.OperationNatureRequired");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenProfileInvalid()
    {
        // Arrange
        InvoiceModel model = new InvoiceModelBuilder().WithProfile("UNKNOWN").Build();

        // Act
        Result<ComplianceReportResponse> result = await Sender.Send(new ValidateInvoiceComplianceQuery(model));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Compliance.Invoice.InvalidProfile");
    }

    [Fact]
    public async Task Handle_ShouldReturnProfile_InReport()
    {
        // Arrange
        InvoiceModel model = new InvoiceModelBuilder().WithProfile("Extended").Build();

        // Act
        Result<ComplianceReportResponse> result = await Sender.Send(new ValidateInvoiceComplianceQuery(model));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Profile.Should().Be("Extended");
    }
}
