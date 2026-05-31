using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Application.Invoicing.ValidateInvoiceCompliance;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.Compliance.Presentation.Invoicing;

internal sealed class ValidateInvoiceCompliance : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("compliance/invoices/validate", async (Request request, ISender sender) =>
        {
            Result<ComplianceReportResponse> result =
                await sender.Send(new ValidateInvoiceComplianceQuery(request.Invoice));

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Invoice)
        .WithSummary("Valider la conformité EN16931")
        .WithDescription(
            "Vérifie la conformité EN16931 d'une facture sans produire de document. " +
            "Profils supportés : EN16931 (Comfort), Extended, Basic, BasicWL, Minimum. " +
            "Le fichier factur-x.xml n'est pas généré par cet endpoint.")
        .Produces<ComplianceReportResponse>()
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }

    internal sealed record Request(InvoiceModel Invoice);
}
