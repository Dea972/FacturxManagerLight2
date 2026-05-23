namespace FacturXManager.Modules.Transmission.Domain.Transmissions;

public interface ITransmissionRepository
{
    Task<Transmission?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    void Insert(Transmission transmission);
}
