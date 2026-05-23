using FacturXManager.Common.Application.Messaging;

namespace FacturXManager.Modules.EReporting.Application.Declarations.GetDeclaration;

public sealed record GetDeclarationQuery(Guid DeclarationId) : IQuery<DeclarationResponse>;
