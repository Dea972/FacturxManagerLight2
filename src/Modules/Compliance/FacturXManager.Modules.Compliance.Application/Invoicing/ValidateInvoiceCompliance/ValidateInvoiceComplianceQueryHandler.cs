using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Compliance.Application.Abstractions;
using FacturXManager.Modules.Compliance.Domain.Invoicing;

namespace FacturXManager.Modules.Compliance.Application.Invoicing.ValidateInvoiceCompliance;

internal sealed class ValidateInvoiceComplianceQueryHandler(
    IInvoiceBuilder invoiceBuilder,
    IEn16931Validator validator)
    : IQueryHandler<ValidateInvoiceComplianceQuery, ComplianceReportResponse>
{
    public Task<Result<ComplianceReportResponse>> Handle(
        ValidateInvoiceComplianceQuery request,
        CancellationToken cancellationToken)
    {
        Result<Invoice> buildResult = invoiceBuilder.Build(request.Invoice);
        if (buildResult.IsFailure)
        {
            return Task.FromResult(Result.Failure<ComplianceReportResponse>(buildResult.Error));
        }

        Invoice invoice = buildResult.Value;

        Result validation = validator.Validate(invoice);

        IReadOnlyList<ComplianceViolation> violations = validation.IsSuccess
            ? []
            : ExtractViolations(validation.Error);

        var report = new ComplianceReportResponse(
            validation.IsSuccess,
            invoice.Profile.ToString(),
            violations);

        return Task.FromResult(Result.Success(report));
    }

    private static List<ComplianceViolation> ExtractViolations(Error error)
    {
        if (error is ValidationError validationError)
        {
            return validationError.Errors
                .Select(e => new ComplianceViolation(e.Code, e.Description))
                .ToList();
        }

        return [new ComplianceViolation(error.Code, error.Description)];
    }
}
