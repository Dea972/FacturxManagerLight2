using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Application.Invoicing.CertifyInvoice;
using FacturXManager.Modules.Compliance.Domain.Invoicing;
using FacturXManager.Modules.Compliance.IntegrationTests.Abstractions;
using FacturXManager.Modules.Compliance.IntegrationTests.Abstractions.Builders;
using FluentAssertions;
using NSubstitute;

namespace FacturXManager.Modules.Compliance.IntegrationTests.Invoicing;

public class CertifyInvoiceTests : BaseIntegrationTest
{
    public CertifyInvoiceTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    private static readonly byte[] FakePdfBytes = [0x25, 0x50, 0x44, 0x46, 0x2D, 0x41]; // "%PDF-A"

    private void StubPdfRendererSuccess()
    {
        PdfRenderer.RenderAsync(Arg.Any<FacturXRenderRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(FakePdfBytes));
    }

    [Fact]
    public async Task Handle_ShouldReturnCertifiedDocument_WhenInvoiceIsValidEn16931Comfort()
    {
        // Arrange
        StubPdfRendererSuccess();
        InvoiceModel model = new InvoiceModelBuilder().Build();

        // Act
        Result<CertifyInvoiceSummary> result = await Sender.Send(new CertifyInvoiceCommand(model));

        // Assert
        result.IsSuccess.Should().BeTrue(because: result.IsFailure ? result.Error.Description : "facture valide");
        result.Value.InvoiceNumber.Should().Be("F-2026-0001");
        result.Value.Profile.Should().Be(nameof(FacturXProfile.En16931));
        result.Value.FacturXPdfBase64.Should().Be(Convert.ToBase64String(FakePdfBytes));
        result.Value.CrossIndustryInvoiceXml.Should().Contain("CrossIndustryInvoice");
    }

    [Fact]
    public async Task Handle_ShouldEmbedXmlNamed_FacturXXml_StrictName()
    {
        // Arrange : la réforme impose un nom de fichier embarqué strict
        StubPdfRendererSuccess();
        InvoiceModel model = new InvoiceModelBuilder().Build();

        // Act
        Result<CertifyInvoiceSummary> result = await Sender.Send(new CertifyInvoiceCommand(model));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.EmbeddedFileName.Should().Be("factur-x.xml");
        await PdfRenderer.Received(1).RenderAsync(
            Arg.Is<FacturXRenderRequest>(r => r.EmbeddedFileName == "factur-x.xml"),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ShouldFail_WithBuyerSirenRequired_WhenBuyerHasNoSirenAfterSept2026()
    {
        // Arrange
        StubPdfRendererSuccess();
        InvoiceModel model = new InvoiceModelBuilder()
            .WithIssueDate(new DateOnly(2026, 9, 15))
            .WithBuyer(b => b.WithoutSiren())
            .Build();

        // Act
        Result<CertifyInvoiceSummary> result = await Sender.Send(new CertifyInvoiceCommand(model));

        // Assert
        result.IsFailure.Should().BeTrue();
        ExtractErrorCodes(result.Error).Should().Contain("Compliance.Invoice.BuyerSirenRequired");
        await PdfRenderer.DidNotReceiveWithAnyArgs().RenderAsync(default!, default);
    }

    [Fact]
    public async Task Handle_ShouldFail_WithOperationNatureRequired_WhenNatureUnspecifiedAfterSept2026()
    {
        // Arrange
        StubPdfRendererSuccess();
        InvoiceModel model = new InvoiceModelBuilder()
            .WithIssueDate(new DateOnly(2026, 9, 15))
            .WithOperationNature("Unspecified")
            .Build();

        // Act
        Result<CertifyInvoiceSummary> result = await Sender.Send(new CertifyInvoiceCommand(model));

        // Assert
        result.IsFailure.Should().BeTrue();
        ExtractErrorCodes(result.Error).Should().Contain("Compliance.Invoice.OperationNatureRequired");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenInvoiceNumberIsEmpty()
    {
        // Arrange
        InvoiceModel model = new InvoiceModelBuilder().WithNumber("").Build();

        // Act
        Result<CertifyInvoiceSummary> result = await Sender.Send(new CertifyInvoiceCommand(model));

        // Assert : le ValidationPipelineBehavior agrège l'échec (FluentValidation)
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenNoLines()
    {
        // Arrange
        InvoiceModel model = new InvoiceModelBuilder().WithoutLines().Build();

        // Act
        Result<CertifyInvoiceSummary> result = await Sender.Send(new CertifyInvoiceCommand(model));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenProfileIsInvalid()
    {
        // Arrange
        InvoiceModel model = new InvoiceModelBuilder().WithProfile("UNKNOWN").Build();

        // Act
        Result<CertifyInvoiceSummary> result = await Sender.Send(new CertifyInvoiceCommand(model));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Compliance.Invoice.InvalidProfile");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenCurrencyIsEmpty()
    {
        // Arrange
        InvoiceModel model = new InvoiceModelBuilder().WithCurrency("").Build();

        // Act
        Result<CertifyInvoiceSummary> result = await Sender.Send(new CertifyInvoiceCommand(model));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenSellerHasNoSiren()
    {
        // Arrange
        StubPdfRendererSuccess();
        InvoiceModel model = new InvoiceModelBuilder()
            .WithSeller(s => s.WithoutSiren())
            .Build();

        // Act
        Result<CertifyInvoiceSummary> result = await Sender.Send(new CertifyInvoiceCommand(model));

        // Assert
        result.IsFailure.Should().BeTrue();
        ExtractErrorCodes(result.Error).Should().Contain("Compliance.Invoice.SellerSirenRequired");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenSellerVatIdentifierMissing_AndProfileIsEn16931()
    {
        // Arrange : profil Comfort exige le numéro TVA du vendeur
        StubPdfRendererSuccess();
        InvoiceModel model = new InvoiceModelBuilder()
            .WithSeller(s => s.WithVatIdentifier(null))
            .Build();

        // Act
        Result<CertifyInvoiceSummary> result = await Sender.Send(new CertifyInvoiceCommand(model));

        // Assert
        result.IsFailure.Should().BeTrue();
        ExtractErrorCodes(result.Error).Should().Contain("Compliance.Invoice.SellerVatIdentifierRequired");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenLineHasInconsistentRateForExemptCategory()
    {
        // Arrange : taux > 0 sur une catégorie exonérée → incohérence TVA
        StubPdfRendererSuccess();
        InvoiceModel model = new InvoiceModelBuilder()
            .ReplaceLines(new InvoiceLineModelBuilder()
                .WithVatCategory("Exempt")
                .WithVatRate(20m)
                .Build())
            .Build();

        // Act
        Result<CertifyInvoiceSummary> result = await Sender.Send(new CertifyInvoiceCommand(model));

        // Assert
        result.IsFailure.Should().BeTrue();
        ExtractErrorCodes(result.Error).Should().Contain("Compliance.Invoice.PositiveRateForExemptCategory");
    }

    [Fact]
    public async Task Handle_ShouldUseExtendedGuidelineId_WhenProfileIsExtended()
    {
        // Arrange
        StubPdfRendererSuccess();
        InvoiceModel model = new InvoiceModelBuilder()
            .WithProfile("Extended")
            .Build();

        // Act
        Result<CertifyInvoiceSummary> result = await Sender.Send(new CertifyInvoiceCommand(model));

        // Assert : guideline ID Extended présent dans le XML
        result.IsSuccess.Should().BeTrue();
        result.Value.Profile.Should().Be(nameof(FacturXProfile.Extended));
        result.Value.CrossIndustryInvoiceXml.Should().Contain(FacturXConstants.ExtendedGuidelineId);
    }

    [Fact]
    public async Task Handle_ShouldGenerateCiiWithComputedTotals()
    {
        // Arrange : 2 × 100 HT @ 20 % → 200 HT, 40 TVA, 240 TTC
        StubPdfRendererSuccess();
        InvoiceModel model = new InvoiceModelBuilder()
            .ReplaceLines(new InvoiceLineModelBuilder()
                .WithQuantity(2m).WithUnitPrice(100m).WithVatRate(20m).Build())
            .Build();

        // Act
        Result<CertifyInvoiceSummary> result = await Sender.Send(new CertifyInvoiceCommand(model));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalWithVat.Should().Be(240m);
        result.Value.CrossIndustryInvoiceXml.Should().Contain("<ram:GrandTotalAmount>240.00</ram:GrandTotalAmount>");
        result.Value.CrossIndustryInvoiceXml.Should().Contain("<ram:LineTotalAmount>200.00</ram:LineTotalAmount>");
    }

    [Fact]
    public async Task Handle_ShouldPropagatePdfRendererFailure_WhenRendererUnavailable()
    {
        // Arrange : la facture est conforme mais le renderer est en échec
        PdfRenderer.RenderAsync(Arg.Any<FacturXRenderRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<byte[]>(InvoiceErrors.PdfRendererNotConfigured));
        InvoiceModel model = new InvoiceModelBuilder().Build();

        // Act
        Result<CertifyInvoiceSummary> result = await Sender.Send(new CertifyInvoiceCommand(model));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Compliance.PdfRendererNotConfigured");
    }

    [Fact]
    public async Task Handle_ShouldAcceptBuyerWithoutSiren_BeforeSept2026()
    {
        // Arrange : avant la date butoir, le SIREN acheteur n'est pas obligatoire
        StubPdfRendererSuccess();
        InvoiceModel model = new InvoiceModelBuilder()
            .WithIssueDate(new DateOnly(2026, 8, 31))
            .WithBuyer(b => b.WithoutSiren())
            .Build();

        // Act
        Result<CertifyInvoiceSummary> result = await Sender.Send(new CertifyInvoiceCommand(model));

        // Assert
        result.IsSuccess.Should().BeTrue(because: result.IsFailure ? result.Error.Description : "obligation pas encore en vigueur");
    }

    private static IEnumerable<string> ExtractErrorCodes(Error error) =>
        error is ValidationError validation
            ? validation.Errors.Select(e => e.Code)
            : [error.Code];
}
