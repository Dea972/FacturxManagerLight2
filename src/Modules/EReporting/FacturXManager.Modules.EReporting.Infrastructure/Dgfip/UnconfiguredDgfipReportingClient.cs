using FacturXManager.Common.Domain;
using FacturXManager.Modules.EReporting.Application.Abstractions;
using FacturXManager.Modules.EReporting.Domain.Declarations;

namespace FacturXManager.Modules.EReporting.Infrastructure.Dgfip;

/// <summary>
/// Implémentation par défaut (non opérationnelle) du client de soumission DGFiP.
/// </summary>
/// <remarks>
/// L'API d'e-reporting DGFiP n'étant pas encore stabilisée, ce point d'extension doit être
/// remplacé par une implémentation concrète (client Refit + authentification, traçage des
/// statuts de soumission) dès que les spécifications seront disponibles.
/// </remarks>
internal sealed class UnconfiguredDgfipReportingClient : IDgfipReportingClient
{
    public Task<Result<DgfipSubmissionReceipt>> SubmitAsync(
        DgfipSubmissionRequest request,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Result.Failure<DgfipSubmissionReceipt>(DeclarationErrors.DgfipClientNotConfigured));
}
