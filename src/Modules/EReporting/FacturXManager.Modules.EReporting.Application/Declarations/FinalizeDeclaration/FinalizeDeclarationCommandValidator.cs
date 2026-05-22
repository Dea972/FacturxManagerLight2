using FluentValidation;

namespace FacturXManager.Modules.EReporting.Application.Declarations.FinalizeDeclaration;

internal sealed class FinalizeDeclarationCommandValidator : AbstractValidator<FinalizeDeclarationCommand>
{
    public FinalizeDeclarationCommandValidator()
    {
        RuleFor(c => c.DeclarationId).NotEmpty();
    }
}
