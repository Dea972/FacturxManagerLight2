using FluentValidation;

namespace FacturXManager.Modules.EReporting.Application.Declarations.SubmitDeclaration;

internal sealed class SubmitDeclarationCommandValidator : AbstractValidator<SubmitDeclarationCommand>
{
    public SubmitDeclarationCommandValidator()
    {
        RuleFor(c => c.DeclarationId).NotEmpty();
    }
}
