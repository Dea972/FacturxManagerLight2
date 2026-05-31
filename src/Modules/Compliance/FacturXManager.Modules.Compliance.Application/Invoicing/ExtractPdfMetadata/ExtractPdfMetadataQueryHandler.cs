using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Domain.Invoicing;

namespace FacturXManager.Modules.Compliance.Application.Invoicing.ExtractPdfMetadata;

internal sealed class ExtractPdfMetadataQueryHandler(IFacturXMetadataExtractor extractor)
    : IQueryHandler<ExtractPdfMetadataQuery, ExtractPdfMetadataResponse>
{
    public Task<Result<ExtractPdfMetadataResponse>> Handle(
        ExtractPdfMetadataQuery request,
        CancellationToken cancellationToken)
    {
        if (request.PdfBytes is not { Length: > 0 })
        {
            return Task.FromResult(Result.Failure<ExtractPdfMetadataResponse>(InvoiceErrors.InvalidPdf));
        }

        string content = request.MetadataType switch
        {
            PdfMetadataType.Xmp => extractor.ExtractXmp(request.PdfBytes),
            PdfMetadataType.EmbeddedXml => extractor.ExtractEmbeddedXml(request.PdfBytes),
            _ => string.Empty
        };

        return Task.FromResult(Result.Success(new ExtractPdfMetadataResponse(content, request.MetadataType)));
    }
}
