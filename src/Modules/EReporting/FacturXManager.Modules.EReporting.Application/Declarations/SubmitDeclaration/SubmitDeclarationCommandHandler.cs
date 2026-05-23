using FacturXManager.Common.Application.Clock;
using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.EReporting.Application.Abstractions;
using FacturXManager.Modules.EReporting.Application.Abstractions.Data;
using FacturXManager.Modules.EReporting.Domain.Declarations;

namespace FacturXManager.Modules.EReporting.Application.Declarations.SubmitDeclaration;

internal sealed class SubmitDeclarationCommandHandler(
    IDeclarationRepository repository,
    IUnitOfWork unitOfWork,
    IDgfipReportingClient dgfipClient,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<SubmitDeclarationCommand, SubmitDeclarationSummary>
{
    public async Task<Result<SubmitDeclarationSummary>> Handle(
        SubmitDeclarationCommand request,
        CancellationToken cancellationToken)
    {
        Declaration? declaration = await repository.GetAsync(request.DeclarationId, cancellationToken);
        if (declaration is null)
        {
            return Result.Failure<SubmitDeclarationSummary>(DeclarationErrors.NotFound(request.DeclarationId));
        }

        if (declaration.Status != DeclarationStatus.Finalized)
        {
            return Result.Failure<SubmitDeclarationSummary>(DeclarationErrors.NotFinalized);
        }

        var submissionRequest = new DgfipSubmissionRequest(
            declaration.Period.Label,
            declaration.TransactionType.ToString(),
            declaration.TotalTaxableAmount,
            declaration.TotalVatAmount,
            declaration.Entries.Count);

        Result<DgfipSubmissionReceipt> submission = await dgfipClient.SubmitAsync(submissionRequest, cancellationToken);
        if (submission.IsFailure)
        {
            return Result.Failure<SubmitDeclarationSummary>(submission.Error);
        }

        Result markResult = declaration.MarkSubmitted(submission.Value.SubmissionReference, dateTimeProvider.UtcNow);
        if (markResult.IsFailure)
        {
            return Result.Failure<SubmitDeclarationSummary>(markResult.Error);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new SubmitDeclarationSummary(
            declaration.Id,
            declaration.Status.ToString(),
            submission.Value.SubmissionReference);
    }
}
