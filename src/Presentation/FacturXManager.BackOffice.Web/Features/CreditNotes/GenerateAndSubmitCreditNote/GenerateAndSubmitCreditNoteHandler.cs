using FacturXManager.BackOffice.Web.Common;
using FacturXManager.BackOffice.Web.Features.Compliance;
using FacturXManager.BackOffice.Web.Features.CreditNotes.ViewModels;
using FacturXManager.Client.Workflows;
using MediatR;
using ClientRequest = FacturXManager.Client.Compliance.Models.GenerateAndSubmitCreditNoteRequest;

namespace FacturXManager.BackOffice.Web.Features.CreditNotes.GenerateAndSubmitCreditNote;

/// <summary>
/// Résultat affiché après génération + dépôt d'un avoir.
/// </summary>
public sealed record GenerateAndSubmitCreditNoteViewModel(
    Guid CreditNoteId,
    string CreditNoteNumber,
    string Profile,
    decimal TotalWithVat,
    string FacturXPdfBase64,
    string Status,
    string? ChannelDepositId,
    string? Message);

/// <summary>
/// Use case front : génère un avoir Factur-X et le dépose sur Chorus Pro en une étape.
/// </summary>
public sealed record GenerateAndSubmitCreditNoteRequest(
        CreditNoteFormViewModel Form,
        string RecipientSiret,
        string Channel,
        string? ExecutingService = null)
    : IRequest<Result<GenerateAndSubmitCreditNoteViewModel>>;

internal sealed class GenerateAndSubmitCreditNoteHandler(
    FacturXManagerClient client,
    ILogger<GenerateAndSubmitCreditNoteHandler> logger)
    : IRequestHandler<GenerateAndSubmitCreditNoteRequest, Result<GenerateAndSubmitCreditNoteViewModel>>
{
    public async Task<Result<GenerateAndSubmitCreditNoteViewModel>> Handle(
        GenerateAndSubmitCreditNoteRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            GenerateAndSubmitDto result = await client.Workflows.GenerateAndSubmitCreditNoteAsync(
                new ClientRequest
                {
                    CreditNote = ComplianceRequestMapper.ToRequest(request.Form),
                    RecipientSiret = request.RecipientSiret,
                    Channel = request.Channel,
                    ExecutingService = request.ExecutingService,
                },
                cancellationToken);

            return Result<GenerateAndSubmitCreditNoteViewModel>.Success(
                new GenerateAndSubmitCreditNoteViewModel(
                    result.DocumentId,
                    result.DocumentNumber,
                    result.Profile,
                    result.TotalWithVat,
                    result.FacturXPdfBase64,
                    result.Status,
                    result.ChannelDepositId,
                    result.Message));
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Échec de la génération + dépôt de l'avoir {CreditNoteNumber}", request.Form.Number);
            return Result<GenerateAndSubmitCreditNoteViewModel>.Failure(ApiErrorTranslator.Describe(exception));
        }
    }
}
