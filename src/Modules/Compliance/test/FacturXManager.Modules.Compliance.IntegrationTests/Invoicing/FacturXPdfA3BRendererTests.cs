using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Application.Invoicing.CertifyInvoice;
using FacturXManager.Modules.Compliance.IntegrationTests.Abstractions;
using FacturXManager.Modules.Compliance.IntegrationTests.Abstractions.Builders;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FacturXManager.Modules.Compliance.IntegrationTests.Invoicing;

[Collection(nameof(RealRendererIntegrationTestCollection))]
public class FacturXPdfA3BRendererTests : IDisposable
{
    private readonly IServiceScope _scope;
    private readonly ISender _sender;

    public FacturXPdfA3BRendererTests(RealRendererIntegrationTestWebAppFactory factory)
    {
        _ = factory.CreateClient();
        _scope = factory.Services.CreateScope();
        _sender = _scope.ServiceProvider.GetRequiredService<ISender>();
    }

    public void Dispose() => _scope.Dispose();

    [Fact]
    public async Task Handle_ShouldProduceNonEmptyPdf_WhenInvoiceIsValid()
    {
        // Arrange
        InvoiceModel model = new InvoiceModelBuilder().Build();

        // Act
        Result<CertifyInvoiceSummary> result = await _sender.Send(new CertifyInvoiceCommand(model));

        // Assert
        result.IsSuccess.Should().BeTrue(because: result.IsFailure ? result.Error.Description : "facture valide");
        result.Value.FacturXPdfBase64.Should().NotBeNullOrEmpty();
        byte[] pdfBytes = Convert.FromBase64String(result.Value.FacturXPdfBase64);
        pdfBytes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldProducePdfStartingWithPdfHeader_WhenInvoiceIsValid()
    {
        // Arrange
        InvoiceModel model = new InvoiceModelBuilder().Build();

        // Act
        Result<CertifyInvoiceSummary> result = await _sender.Send(new CertifyInvoiceCommand(model));

        // Assert
        result.IsSuccess.Should().BeTrue(because: result.IsFailure ? result.Error.Description : "facture valide");
        byte[] pdfBytes = Convert.FromBase64String(result.Value.FacturXPdfBase64);
        string header = System.Text.Encoding.ASCII.GetString(pdfBytes, 0, Math.Min(5, pdfBytes.Length));
        header.Should().Be("%PDF-", because: "tout fichier PDF commence par la signature %PDF-");
    }

    [Fact]
    public async Task Handle_ShouldEmbedXmlInPdf_WhenInvoiceIsValid()
    {
        // Arrange
        InvoiceModel model = new InvoiceModelBuilder().Build();

        // Act
        Result<CertifyInvoiceSummary> result = await _sender.Send(new CertifyInvoiceCommand(model));

        // Assert
        result.IsSuccess.Should().BeTrue(because: result.IsFailure ? result.Error.Description : "facture valide");
        byte[] pdfBytes = Convert.FromBase64String(result.Value.FacturXPdfBase64);
        string pdfContent = System.Text.Encoding.Latin1.GetString(pdfBytes);
        pdfContent.Should().Contain("factur-x.xml", because: "le nom du fichier XML embarqué doit apparaître dans le PDF");
        pdfContent.Should().Contain("CrossIndustryInvoice", because: "le XML CII doit être embarqué dans le PDF");
    }

    [Fact]
    public async Task Handle_ShouldIncludeFacturXXmpMetadata_WhenInvoiceIsValid()
    {
        // Arrange
        InvoiceModel model = new InvoiceModelBuilder().Build();

        // Act
        Result<CertifyInvoiceSummary> result = await _sender.Send(new CertifyInvoiceCommand(model));

        // Assert
        result.IsSuccess.Should().BeTrue(because: result.IsFailure ? result.Error.Description : "facture valide");
        byte[] pdfBytes = Convert.FromBase64String(result.Value.FacturXPdfBase64);
        string pdfContent = System.Text.Encoding.Latin1.GetString(pdfBytes);
        pdfContent.Should().Contain("factur-x:pdfa:CrossIndustryDocument", because: "l'espace de noms XMP Factur-X doit être présent");
        pdfContent.Should().Contain("INVOICE", because: "fx:DocumentType=INVOICE est obligatoire");
        pdfContent.Should().Contain("EN 16931", because: "fx:ConformanceLevel doit correspondre au profil EN16931");
    }

    [Fact]
    public async Task Handle_ShouldIncludePdfA3bXmpDeclaration_WhenInvoiceIsValid()
    {
        // Arrange
        InvoiceModel model = new InvoiceModelBuilder().Build();

        // Act
        Result<CertifyInvoiceSummary> result = await _sender.Send(new CertifyInvoiceCommand(model));

        // Assert
        result.IsSuccess.Should().BeTrue(because: result.IsFailure ? result.Error.Description : "facture valide");
        byte[] pdfBytes = Convert.FromBase64String(result.Value.FacturXPdfBase64);
        string pdfContent = System.Text.Encoding.Latin1.GetString(pdfBytes);
        pdfContent.Should().Contain("pdfaid:part>3<", because: "pdfaid:part=3 est requis pour PDF/A-3");
        pdfContent.Should().Contain("pdfaid:conformance>B<", because: "pdfaid:conformance=B est requis pour PDF/A-3b");
    }
}
