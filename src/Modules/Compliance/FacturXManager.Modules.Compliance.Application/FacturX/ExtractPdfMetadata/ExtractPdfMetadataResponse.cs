namespace FacturXManager.Modules.Compliance.Application.FacturX.ExtractPdfMetadata;

public sealed record ExtractPdfMetadataResponse(
    string Content,
    PdfMetadataType MetadataType);
