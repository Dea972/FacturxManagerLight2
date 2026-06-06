using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Application.Abstractions.CreditNotes;
using FacturXManager.Modules.Compliance.Application.CreditNotes.DownloadCreditNotePdf;
using FacturXManager.Modules.Compliance.Application.CreditNotes.GenerateCreditNote;
using FacturXManager.Modules.Compliance.Domain.Common;
using FacturXManager.Modules.Compliance.IntegrationTests.Abstractions;
using FacturXManager.Modules.Compliance.IntegrationTests.Abstractions.Builders;
using FluentAssertions;
using NSubstitute;

namespace FacturXManager.Modules.Compliance.IntegrationTests.Invoicing;

public class GenerateCreditNoteTests : BaseIntegrationTest
{
    public GenerateCreditNoteTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
    }

    private static readonly byte[] FakePdfBytes = [0x25, 0x50, 0x44, 0x46, 0x2D, 0x41]; // "%PDF-A"

    private void StubPdfRendererSuccess()
    {
        PdfRenderer.RenderAsync(Arg.Any<FacturXRenderRequest>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(FakePdfBytes));
    }

    [Fact]
    public async Task Handle_ShouldReturnGeneratedCreditNote_WhenModelIsValid()
    {
        // Arrange
        StubPdfRendererSuccess();
        CreditNoteModel model = new CreditNoteModelBuilder().Build();

        // Act
        Result<GenerateCreditNoteSummary> result = await Sender.Send(new GenerateCreditNoteCommand(model));

        // Assert
        result.IsSuccess.Should().BeTrue(because: result.IsFailure ? result.Error.Description : "avoir valide");
        result.Value.CreditNoteNumber.Should().Be("AV-2026-0001");
        result.Value.Profile.Should().Be(nameof(FacturXProfile.En16931));
        result.Value.FacturXPdfBase64.Should().Be(Convert.ToBase64String(FakePdfBytes));
        result.Value.CrossIndustryInvoiceXml.Should().Contain("CrossIndustryInvoice");
    }

    [Fact]
    public async Task Handle_ShouldGenerateCiiWithTypeCode381()
    {
        // Arrange
        StubPdfRendererSuccess();
        CreditNoteModel model = new CreditNoteModelBuilder().Build();

        // Act
        Result<GenerateCreditNoteSummary> result = await Sender.Send(new GenerateCreditNoteCommand(model));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CrossIndustryInvoiceXml.Should().Contain("<ram:TypeCode>381</ram:TypeCode>");
    }

    [Fact]
    public async Task Handle_ShouldIncludeInvoiceReferencedDocument()
    {
        // Arrange
        StubPdfRendererSuccess();
        CreditNoteModel model = new CreditNoteModelBuilder()
            .WithOriginalInvoice("F-2026-0042", new DateOnly(2026, 9, 10))
            .Build();

        // Act
        Result<GenerateCreditNoteSummary> result = await Sender.Send(new GenerateCreditNoteCommand(model));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.CrossIndustryInvoiceXml.Should().Contain("<ram:IssuerAssignedID>F-2026-0042</ram:IssuerAssignedID>");
        result.Value.CrossIndustryInvoiceXml.Should().Contain("20260910");
    }

    [Fact]
    public async Task Handle_ShouldGenerateCiiWithComputedTotals()
    {
        // Arrange : 3 × 200 HT @ 20 % → 600 HT, 120 TVA, 720 TTC
        StubPdfRendererSuccess();
        CreditNoteModel model = new CreditNoteModelBuilder()
            .WithoutLines()
            .WithLine(l => l.WithQuantity(3m).WithUnitPrice(200m).WithVatRate(20m))
            .Build();

        // Act
        Result<GenerateCreditNoteSummary> result = await Sender.Send(new GenerateCreditNoteCommand(model));

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.TotalWithVat.Should().Be(720m);
        result.Value.CrossIndustryInvoiceXml.Should().Contain("<ram:GrandTotalAmount>720.00</ram:GrandTotalAmount>");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenOriginalInvoiceNumberIsEmpty()
    {
        // Arrange
        StubPdfRendererSuccess();
        CreditNoteModel model = new CreditNoteModelBuilder()
            .WithOriginalInvoice("")
            .Build();

        // Act
        Result<GenerateCreditNoteSummary> result = await Sender.Send(new GenerateCreditNoteCommand(model));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Compliance.CreditNote.OriginalInvoiceNumberRequired");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenNumberIsEmpty()
    {
        // Arrange
        StubPdfRendererSuccess();
        CreditNoteModel model = new CreditNoteModelBuilder().WithNumber("").Build();

        // Act
        Result<GenerateCreditNoteSummary> result = await Sender.Send(new GenerateCreditNoteCommand(model));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Compliance.CreditNote.NumberRequired");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenNoLines()
    {
        // Arrange
        StubPdfRendererSuccess();
        CreditNoteModel model = new CreditNoteModelBuilder().WithoutLines().Build();

        // Act
        Result<GenerateCreditNoteSummary> result = await Sender.Send(new GenerateCreditNoteCommand(model));

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Compliance.CreditNote.NoLines");
    }

    [Fact]
    public async Task DownloadHandle_ShouldReturnPdfBytes_WhenModelIsValid()
    {
        // Arrange
        StubPdfRendererSuccess();
        CreditNoteModel model = new CreditNoteModelBuilder().Build();

        // Act
        Result<DownloadCreditNotePdfSummary> result =
            await Sender.Send(new DownloadCreditNotePdfCommand(model));

        // Assert
        result.IsSuccess.Should().BeTrue(because: result.IsFailure ? result.Error.Description : "avoir valide");
        result.Value.PdfBytes.Should().BeEquivalentTo(FakePdfBytes);
        result.Value.FileName.Should().Contain("AV-2026-0001");
        result.Value.CrossIndustryInvoiceXml.Should().Contain("<ram:TypeCode>381</ram:TypeCode>");
    }
}
