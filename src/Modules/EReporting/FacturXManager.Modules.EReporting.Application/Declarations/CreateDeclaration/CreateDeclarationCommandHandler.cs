using FacturXManager.Common.Application.Clock;
using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.EReporting.Application.Abstractions.Data;
using FacturXManager.Modules.EReporting.Domain.Declarations;

namespace FacturXManager.Modules.EReporting.Application.Declarations.CreateDeclaration;

internal sealed class CreateDeclarationCommandHandler(
    IDeclarationRepository repository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateDeclarationCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateDeclarationCommand request, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse(request.TransactionType?.Trim(), ignoreCase: true, out TransactionType transactionType) ||
            !Enum.IsDefined(transactionType))
        {
            return Result.Failure<Guid>(
                DeclarationErrors.InvalidTransactionType(request.TransactionType ?? string.Empty));
        }

        Result<ReportingPeriod> periodResult = ReportingPeriod.Create(request.Year, request.Month);
        if (periodResult.IsFailure)
        {
            return Result.Failure<Guid>(periodResult.Error);
        }

        Declaration declaration = Declaration.Create(periodResult.Value, transactionType, dateTimeProvider.UtcNow);

        repository.Insert(declaration);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return declaration.Id;
    }
}
