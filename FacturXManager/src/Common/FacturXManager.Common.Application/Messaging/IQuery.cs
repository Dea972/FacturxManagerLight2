using MediatR;
using FacturXManager.Common.Domain;

namespace FacturXManager.Common.Application.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
