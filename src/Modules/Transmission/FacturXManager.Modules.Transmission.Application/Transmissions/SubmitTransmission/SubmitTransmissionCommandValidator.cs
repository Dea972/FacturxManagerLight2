using FluentValidation;

namespace FacturXManager.Modules.Transmission.Application.Transmissions.SubmitTransmission;

internal sealed class SubmitTransmissionCommandValidator : AbstractValidator<SubmitTransmissionCommand>
{
    public SubmitTransmissionCommandValidator()
    {
        RuleFor(c => c.DocumentReference).NotEmpty();
        RuleFor(c => c.RecipientSiret).NotEmpty().Length(14);
        RuleFor(c => c.Channel).NotEmpty();
        RuleFor(c => c.FacturXPdfBase64).NotEmpty();
    }
}
