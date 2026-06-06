using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Application.Abstractions.Invoices;
using FacturXManager.Modules.Compliance.Application.FacturX.AttachFacturX;
using FacturXManager.Modules.Compliance.Domain.Common;
using FacturXManager.Modules.Compliance.IntegrationTests.Abstractions;
using FacturXManager.Modules.Compliance.IntegrationTests.Abstractions.Builders;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace FacturXManager.Modules.Compliance.IntegrationTests.Invoicing;

public class AttachFacturXTests : BaseIntegrationTest
{
    private readonly IFacturXPdfEmbedder _pdfEmbedderMock;

    public AttachFacturXTests(IntegrationTestWebAppFactory factory) : base(factory)
    {
        _pdfEmbedderMock = Substitute.For<IFacturXPdfEmbedder>();
    }

    private static readonly byte[] SamplePdfBytes = { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x31, 0x2E, 0x37 }; // "%PDF-1.7"
    private static readonly byte[] OutputPdfBytes = { 0x25, 0x50, 0x44, 0x46, 0x2D, 0x41, 0x2D, 0x33, 0x62 }; // "%PDF-A-3b"

    private void StubPdfEmbedderSuccess()
    {
        _pdfEmbedderMock.EmbedAsync(Arg.Any<byte[]>(), Arg.Any<FacturXRenderRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(OutputPdfBytes)));
    }

    [Fact]
    public async Task Handle_ShouldReturnAttachedResult_WhenPdfAndInvoiceAreValid()
    {
        // Arrange
        var testFactory = new IntegrationTestWebAppFactory();
        using IServiceScope scope = testFactory.Services.CreateScope();
        IInvoiceBuilder builder = scope.ServiceProvider.GetRequiredService<IInvoiceBuilder>();
        IEn16931Validator validator = scope.ServiceProvider.GetRequiredService<IEn16931Validator>();
        ICrossIndustryInvoiceWriter xmlWriter = scope.ServiceProvider.GetRequiredService<ICrossIndustryInvoiceWriter>();

        IFacturXPdfEmbedder embedderSubstitute = Substitute.For<IFacturXPdfEmbedder>();
        embedderSubstitute.EmbedAsync(Arg.Any<byte[]>(), Arg.Any<FacturXRenderRequest>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(Result.Success(OutputPdfBytes)));

        var handler = new AttachFacturXCommandHandler(builder, validator, xmlWriter, embedderSubstitute);
        InvoiceModel model = new InvoiceModelBuilder()
            .WithSeller(s => s.AsFrenchSeller())
            .Build();
        var command = new AttachFacturXCommand(SamplePdfBytes, model);

        // Act
        Result<AttachFacturXSummary> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        if (result.IsFailure)
        {
            if (result.Error is ValidationError valErr)
            {
                string errors = string.Join(", ", valErr.Errors.Select(e => $"{e.Code}: {e.Description}"));
                errors.Should().BeEmpty();
            }
            else
            {
                result.Error.Code.Should().BeEmpty();
            }
        }
        result.IsSuccess.Should().BeTrue();
        result.Value.InvoiceNumber.Should().Be("F-2026-0001");
        result.Value.Profile.Should().Be(nameof(FacturXProfile.En16931));
        result.Value.FacturXPdfBase64.Should().Be(Convert.ToBase64String(OutputPdfBytes));
        result.Value.CrossIndustryInvoiceXml.Should().Contain("CrossIndustryInvoice");
        result.Value.EmbeddedFileName.Should().Be("factur-x.xml");
    }

    [Fact]
    public async Task Handle_ShouldFail_WhenBuyerSirenMissingAfterSept2026()
    {
        // Arrange
        var testFactory = new IntegrationTestWebAppFactory();
        using IServiceScope scope = testFactory.Services.CreateScope();
        IInvoiceBuilder builder = scope.ServiceProvider.GetRequiredService<IInvoiceBuilder>();
        IEn16931Validator validator = scope.ServiceProvider.GetRequiredService<IEn16931Validator>();
        ICrossIndustryInvoiceWriter xmlWriter = scope.ServiceProvider.GetRequiredService<ICrossIndustryInvoiceWriter>();

        IFacturXPdfEmbedder embedderSubstitute = Substitute.For<IFacturXPdfEmbedder>();

        var handler = new AttachFacturXCommandHandler(builder, validator, xmlWriter, embedderSubstitute);
        InvoiceModel model = new InvoiceModelBuilder()
            .WithIssueDate(new DateOnly(2026, 9, 15))
            .WithBuyer(b => b.WithoutSiren())
            .Build();

        var command = new AttachFacturXCommand(SamplePdfBytes, model);

        // Act
        Result<AttachFacturXSummary> result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<ValidationError>();
        ((ValidationError)result.Error).Errors.Should().Contain(e => e.Code == "Compliance.Invoice.BuyerSirenRequired");
    }
}
