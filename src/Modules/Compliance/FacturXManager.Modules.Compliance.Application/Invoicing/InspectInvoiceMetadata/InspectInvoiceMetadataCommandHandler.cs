using System.IO.Compression;
using System.Text;
using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Application.Invoicing.ExtractPdfMetadata;
using FacturXManager.Modules.Compliance.Domain.Invoicing;

namespace FacturXManager.Modules.Compliance.Application.Invoicing.InspectInvoiceMetadata;

internal sealed class InspectInvoiceMetadataCommandHandler(
    IInvoiceBuilder invoiceBuilder,
    IEn16931Validator validator,
    ICrossIndustryInvoiceWriter xmlWriter,
    IFacturXPdfA3BRenderer pdfRenderer,
    IFacturXMetadataExtractor metadataExtractor)
    : ICommandHandler<InspectInvoiceMetadataCommand, byte[]>
{
    public async Task<Result<byte[]>> Handle(
        InspectInvoiceMetadataCommand request,
        CancellationToken cancellationToken)
    {
        Result<Invoice> buildResult = invoiceBuilder.Build(request.Invoice);
        if (buildResult.IsFailure)
        {
            return Result.Failure<byte[]>(buildResult.Error);
        }

        Invoice invoice = buildResult.Value;

        Result validation = validator.Validate(invoice);
        if (validation.IsFailure)
        {
            return Result.Failure<byte[]>(validation.Error);
        }

        string ciiXml = xmlWriter.Write(invoice);

        var renderRequest = new FacturXRenderRequest(
            ciiXml,
            FacturXConstants.EmbeddedXmlFileName,
            invoice.Number,
            invoice.Seller.Name,
            invoice.Buyer.Name,
            invoice.IssueDate,
            invoice.CurrencyCode,
            invoice.TotalWithoutVat,
            invoice.TotalVat,
            invoice.TotalWithVat,
            invoice.Profile);

        Result<byte[]> pdfResult = await pdfRenderer.RenderAsync(renderRequest, cancellationToken);
        if (pdfResult.IsFailure)
        {
            return Result.Failure<byte[]>(pdfResult.Error);
        }

        byte[] pdfBytes = pdfResult.Value;

        return request.MetadataType switch
        {
            PdfMetadataType.EmbeddedXml => Encoding.UTF8.GetBytes(metadataExtractor.ExtractEmbeddedXml(pdfBytes)),
            PdfMetadataType.Zip => BuildZip(pdfBytes, ciiXml, metadataExtractor.ExtractXmp(pdfBytes), invoice.Number),
            _ => Encoding.UTF8.GetBytes(metadataExtractor.ExtractXmp(pdfBytes))
        };
    }

    private static byte[] BuildZip(byte[] pdfBytes, string ciiXml, string xmp, string invoiceNumber)
    {
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteEntry(archive, $"{invoiceNumber}.pdf", pdfBytes);
            WriteEntry(archive, FacturXConstants.EmbeddedXmlFileName, Encoding.UTF8.GetBytes(ciiXml));
            WriteEntry(archive, "xmp.xml", Encoding.UTF8.GetBytes(xmp));
        }
        return stream.ToArray();
    }

    private static void WriteEntry(ZipArchive archive, string entryName, byte[] data)
    {
        ZipArchiveEntry entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
        using Stream entryStream = entry.Open();
        entryStream.Write(data);
    }
}
