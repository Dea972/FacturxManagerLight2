using FluentValidation;

namespace FacturXManager.Modules.EReporting.Application.Declarations.CreateDeclaration;

internal sealed class CreateDeclarationCommandValidator : AbstractValidator<CreateDeclarationCommand>
{
    public CreateDeclarationCommandValidator()
    {
        RuleFor(c => c.Year).InclusiveBetween(2024, 2100);
        RuleFor(c => c.Month).InclusiveBetween(1, 12);
        RuleFor(c => c.TransactionType).NotEmpty();
    }
}
