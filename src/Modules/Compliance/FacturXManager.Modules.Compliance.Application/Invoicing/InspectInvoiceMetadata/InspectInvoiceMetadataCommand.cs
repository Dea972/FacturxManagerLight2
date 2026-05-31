using FacturXManager.Common.Application.Messaging;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Application.Invoicing.ExtractPdfMetadata;

namespace FacturXManager.Modules.Compliance.Application.Invoicing.InspectInvoiceMetadata;

public sealed record InspectInvoiceMetadataCommand(
    InvoiceModel Invoice,
    PdfMetadataType MetadataType) : ICommand<byte[]>;
