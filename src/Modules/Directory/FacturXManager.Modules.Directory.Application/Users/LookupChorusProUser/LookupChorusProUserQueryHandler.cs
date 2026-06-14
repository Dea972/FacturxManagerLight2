using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Directory.Application.Abstractions;
using FacturXManager.Modules.Directory.Domain.Companies;

namespace FacturXManager.Modules.Directory.Application.Users.LookupChorusProUser;

internal sealed class LookupChorusProUserQueryHandler(IChorusProUserService userService)
    : IQueryHandler<LookupChorusProUserQuery, ChorusProUserLookupResponse>
{
    public async Task<Result<ChorusProUserLookupResponse>> Handle(
        LookupChorusProUserQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Login))
        {
            return Result.Failure<ChorusProUserLookupResponse>(CompanyErrors.IdentifierRequired);
        }

        return await userService.ConsultUserAsync(request.Login, cancellationToken);
    }
}
