using FacturXManager.Common.Application.Messaging;
using FacturXManager.Common.Domain;
using FacturXManager.Modules.Directory.Application.Abstractions;
using FacturXManager.Modules.Directory.Domain.Companies;

namespace FacturXManager.Modules.Directory.Application.Structures.ConsultChorusProStructure;

internal sealed class ConsultChorusProStructureQueryHandler(IChorusProStructureService structureService)
    : IQueryHandler<ConsultChorusProStructureQuery, ChorusProStructureDetailResponse>
{
    public async Task<Result<ChorusProStructureDetailResponse>> Handle(
        ConsultChorusProStructureQuery request,
        CancellationToken cancellationToken)
    {
        if (request.IdStructureCpp <= 0)
        {
            return Result.Failure<ChorusProStructureDetailResponse>(CompanyErrors.IdentifierRequired);
        }

        return await structureService.ConsultByIdAsync(request.IdStructureCpp, cancellationToken);
    }
}
