namespace FacturXManager.Modules.EReporting.Domain.Declarations;

public interface IDeclarationRepository
{
    Task<Declaration?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    void Insert(Declaration declaration);
}
