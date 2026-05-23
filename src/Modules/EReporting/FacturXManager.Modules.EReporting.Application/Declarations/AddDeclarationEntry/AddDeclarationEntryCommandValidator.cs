using FluentValidation;

namespace FacturXManager.Modules.EReporting.Application.Declarations.AddDeclarationEntry;

internal sealed class AddDeclarationEntryCommandValidator : AbstractValidator<AddDeclarationEntryCommand>
{
    public AddDeclarationEntryCommandValidator()
    {
        RuleFor(c => c.DeclarationId).NotEmpty();
        RuleFor(c => c.TaxableAmount).GreaterThan(0);
        RuleFor(c => c.VatAmount).GreaterThanOrEqualTo(0);
        RuleFor(c => c.CountryCode).NotEmpty().Length(2);
        RuleFor(c => c.OperationNature).NotEmpty();
    }
}
