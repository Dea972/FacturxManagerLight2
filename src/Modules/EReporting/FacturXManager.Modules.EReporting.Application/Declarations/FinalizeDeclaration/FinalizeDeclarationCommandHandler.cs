using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.EReporting.Application.Abstractions.Data;
using FacturXManager.Modules.EReporting.Domain.Declarations;

namespace FacturXManager.Modules.EReporting.Application.Declarations.FinalizeDeclaration;

internal sealed class FinalizeDeclarationCommandHandler(
    IDeclarationRepository repository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<FinalizeDeclarationCommand>
{
    public async Task<Result> Handle(FinalizeDeclarationCommand request, CancellationToken cancellationToken)
    {
        Declaration? declaration = await repository.GetAsync(request.DeclarationId, cancellationToken);
        if (declaration is null)
        {
            return Result.Failure(DeclarationErrors.NotFound(request.DeclarationId));
        }

        Result finalizeResult = declaration.Finalize();
        if (finalizeResult.IsFailure)
        {
            return finalizeResult;
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
