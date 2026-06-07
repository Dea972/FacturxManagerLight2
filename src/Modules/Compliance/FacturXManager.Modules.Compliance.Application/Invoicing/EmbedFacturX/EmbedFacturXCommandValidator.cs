using FluentValidation;

namespace FacturXManager.Modules.Compliance.Application.Invoicing.EmbedFacturX;

internal sealed class EmbedFacturXCommandValidator : AbstractValidator<EmbedFacturXCommand>
{
    public EmbedFacturXCommandValidator()
    {
        RuleFor(c => c.VisualPdfBytes)
            .NotEmpty()
            .WithMessage("Les octets du fichier PDF d'origine ne peuvent pas être vides.");

        RuleFor(c => c.Invoice).NotNull();

        RuleFor(c => c.Invoice.Number).NotEmpty();
        RuleFor(c => c.Invoice.CurrencyCode).NotEmpty().Length(3);
        RuleFor(c => c.Invoice.Profile).NotEmpty();
        RuleFor(c => c.Invoice.Lines).NotEmpty();

        RuleFor(c => c.Invoice.Seller).NotNull();
        RuleFor(c => c.Invoice.Buyer).NotNull();

        RuleForEach(c => c.Invoice.Lines).ChildRules(line =>
        {
            line.RuleFor(l => l.Description).NotEmpty();
            line.RuleFor(l => l.Quantity).GreaterThan(0);
            line.RuleFor(l => l.VatRate).GreaterThanOrEqualTo(0);
        });
    }
}
