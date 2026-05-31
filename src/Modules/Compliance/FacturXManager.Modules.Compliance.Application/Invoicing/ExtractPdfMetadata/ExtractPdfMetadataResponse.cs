namespace FacturXManager.Modules.Compliance.Application.Invoicing.ExtractPdfMetadata;

public sealed record ExtractPdfMetadataResponse(
    string Content,
    PdfMetadataType MetadataType);
