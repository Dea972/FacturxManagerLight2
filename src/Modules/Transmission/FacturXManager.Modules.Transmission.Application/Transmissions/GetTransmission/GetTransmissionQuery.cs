using FacturXManager.Common.Application.Messaging;

namespace FacturXManager.Modules.Transmission.Application.Transmissions.GetTransmission;

public sealed record GetTransmissionQuery(Guid TransmissionId) : IQuery<TransmissionResponse>;
