using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Domain.Common;
using FacturXManager.Modules.Compliance.Domain.Invoices;
using FacturXManager.Modules.Compliance.UnitTests.Abstractions;
using FluentAssertions;

namespace FacturXManager.Modules.Compliance.UnitTests.Invoicing;

public class InvoiceTests : BaseTest
{
    [Fact]
    public void Create_ShouldFail_WhenNumberIsEmpty()
    {
        // Arrange
        InvoiceLine[] lines = new[] { InvoiceTestData.StandardLine(1m, 10m) };

        // Act
        Result<Invoice> result = Invoice.Create(
            number: "",
            issueDate: new DateOnly(2026, 9, 1),
            currencyCode: "EUR",
            profile: FacturXProfile.En16931,
            operationNature: OperationNature.Services,
            seller: InvoiceTestData.SellerWithSiren(),
            buyer: InvoiceTestData.BuyerWithSiren(),
            payment: PaymentDetails.Create("30"),
            lines: lines);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(InvoiceErrors.NumberRequired);
    }

    [Fact]
    public void Create_ShouldFail_WhenNoLines()
    {
        // Act
        Result<Invoice> result = InvoiceTestData.BuildInvoice(lines: []);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(InvoiceErrors.NoLines);
    }

    [Fact]
    public void Create_ShouldComputeTotals_WhenSingleStandardLine()
    {
        // Arrange
        InvoiceLine line = InvoiceTestData.StandardLine(quantity: 3m, unitNetPrice: 100m, vatRate: 20m);

        // Act
        Result<Invoice> result = InvoiceTestData.BuildInvoice(lines: [line]);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Invoice invoice = result.Value;
        invoice.TotalWithoutVat.Should().Be(300m);
        invoice.TotalVat.Should().Be(60m);
        invoice.TotalWithVat.Should().Be(360m);
        invoice.AmountDue.Should().Be(360m);
    }

    [Fact]
    public void Create_ShouldGroupVatBreakdowns_WhenLinesShareCategoryAndRate()
    {
        // Arrange : deux lignes 20 % + une ligne 5,5 %
        InvoiceLine line1 = InvoiceTestData.StandardLine(quantity: 2m, unitNetPrice: 50m, vatRate: 20m);
        InvoiceLine line2 = InvoiceTestData.StandardLine(quantity: 1m, unitNetPrice: 100m, vatRate: 20m);
        var line3 = InvoiceLine.Create( "Livre", 1m, 10m, VatCategory.Standard, 5.5m);

        // Act
        Invoice invoice = InvoiceTestData.BuildInvoice(lines: [line1, line2, line3]).Value;

        // Assert
        invoice.VatBreakdowns.Should().HaveCount(2);
        invoice.VatBreakdowns.Single(b => b.Rate == 20m).BasisAmount.Should().Be(200m);
        invoice.VatBreakdowns.Single(b => b.Rate == 20m).TaxAmount.Should().Be(40m);
        invoice.VatBreakdowns.Single(b => b.Rate == 5.5m).BasisAmount.Should().Be(10m);
        invoice.VatBreakdowns.Single(b => b.Rate == 5.5m).TaxAmount.Should().Be(0.55m);
    }

    [Theory]
    [InlineData(2026, 8, 31, false)]
    [InlineData(2026, 9, 1, true)]
    [InlineData(2027, 1, 1, true)]
    public void IsSubjectToB2BObligation_ShouldReflectThresholdDate(int year, int month, int day, bool expected)
    {
        // Arrange
        Invoice invoice = InvoiceTestData.BuildInvoice(issueDate: new DateOnly(year, month, day)).Value;

        // Act
        bool actual = invoice.IsSubjectToB2BObligation();

        // Assert
        actual.Should().Be(expected);
    }
}
