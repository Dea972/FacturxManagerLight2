using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.EReporting.Application.Abstractions.Data;
using FacturXManager.Modules.EReporting.Domain.Declarations;

namespace FacturXManager.Modules.EReporting.Application.Declarations.AddDeclarationEntry;

internal sealed class AddDeclarationEntryCommandHandler(
    IDeclarationRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<AddDeclarationEntryCommand>
{
    public async Task<Result> Handle(AddDeclarationEntryCommand request, CancellationToken cancellationToken)
    {
        Declaration? declaration = await repository.GetAsync(request.DeclarationId, cancellationToken);
        if (declaration is null)
        {
            return Result.Failure(DeclarationErrors.NotFound(request.DeclarationId));
        }

        if (!Enum.TryParse(request.OperationNature?.Trim(), ignoreCase: true, out OperationNature nature) ||
            !Enum.IsDefined(nature))
        {
            return Result.Failure(DeclarationErrors.InvalidOperationNature(request.OperationNature ?? string.Empty));
        }

        Result addResult = declaration.AddEntry(
            request.TaxableAmount,
            request.VatAmount,
            request.CountryCode,
            nature);

        if (addResult.IsFailure)
        {
            return addResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
