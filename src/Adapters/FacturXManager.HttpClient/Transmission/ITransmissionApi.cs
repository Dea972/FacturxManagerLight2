using System;
using System.Threading;
using System.Threading.Tasks;
using FacturXManager.Client.Transmission.Models;
using Refit;

namespace FacturXManager.Client.Transmission;

/// <summary>
/// Client HTTP pour le module Transmission : dépôt et suivi des documents Factur-X.
/// </summary>
public interface ITransmissionApi
{
    /// <summary>
    /// Dépose un document certifié sur la plateforme du canal demandé et trace la tentative.
    /// </summary>
    [Post("/transmissions")]
    Task<SubmitTransmissionDto> SubmitAsync(
        [Body] SubmitTransmissionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retourne le statut courant et l'historique des tentatives d'une transmission.
    /// </summary>
    [Get("/transmissions/{transmissionId}")]
    Task<TransmissionDto> GetAsync(
        Guid transmissionId,
        CancellationToken cancellationToken = default);
}
