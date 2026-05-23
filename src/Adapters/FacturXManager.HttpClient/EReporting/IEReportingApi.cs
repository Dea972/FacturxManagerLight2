using System;
using System.Threading;
using System.Threading.Tasks;
using FacturXManager.Client.EReporting.Models;
using Refit;

namespace FacturXManager.Client.EReporting;

/// <summary>
/// Client HTTP pour le module EReporting : déclarations périodiques B2C et internationales.
/// </summary>
public interface IEReportingApi
{
    /// <summary>
    /// Crée une nouvelle déclaration au statut Brouillon.
    /// </summary>
    [Post("/ereporting/declarations")]
    Task<Guid> CreateAsync(
        [Body] CreateDeclarationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retourne le détail complet et le statut courant d'une déclaration.
    /// </summary>
    [Get("/ereporting/declarations/{declarationId}")]
    Task<DeclarationDto> GetAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Ajoute une transaction à une déclaration au statut Brouillon.
    /// </summary>
    [Post("/ereporting/declarations/{declarationId}/entries")]
    Task AddEntryAsync(
        Guid declarationId,
        [Body] AddDeclarationEntryRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finalise une déclaration Brouillon pour la rendre prête à la soumission.
    /// </summary>
    [Post("/ereporting/declarations/{declarationId}/finalize")]
    Task FinalizeAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Soumet une déclaration finalisée à la DGFiP.
    /// </summary>
    [Post("/ereporting/declarations/{declarationId}/submit")]
    Task<SubmitDeclarationDto> SubmitAsync(
        Guid declarationId,
        CancellationToken cancellationToken = default);
}
