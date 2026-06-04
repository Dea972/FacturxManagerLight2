using FacturXManager.Common.Domain;
using FacturXManager.Common.Presentation.ApiResults;
using FacturXManager.Common.Presentation.Endpoints;
using FacturXManager.Modules.Compliance.Application.Invoices.GetInvoices;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace FacturXManager.Modules.Compliance.Presentation.Invoices;

internal sealed class GetInvoices : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("compliance/invoices", async (ISender sender) =>
        {
            Result<List<InvoiceSummaryResponse>> result =
                await sender.Send(new GetInvoicesQuery());

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Invoice)
        .WithSummary("Lister les factures")
        .WithDescription("Retourne la liste des factures certifiées et persistées, ordonnées par date d'émission décroissante.")
        .Produces<IReadOnlyList<InvoiceSummaryResponse>>()
        .Produces<ProblemDetails>(StatusCodes.Status400BadRequest);
    }
}
