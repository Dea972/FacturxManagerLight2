using System.Diagnostics;
using FacturXManager.Common.Application.Transmission;
using FacturXManager.Modules.Transmission.Infrastructure.ChorusPro;
using Microsoft.Extensions.Logging;
using Refit;

namespace FacturXManager.Modules.Transmission.Infrastructure.Transmitters;

/// <summary>
/// Adaptateur Chorus Pro (PISTE) en mode DEPOT_PDF_API. Dépose le PDF/A-3b encodé en Base64.
/// </summary>
internal sealed class ChorusProTransmitter(
    IChorusProApi api,
    ILogger<ChorusProTransmitter> logger)
    : IChannelTransmitter
{
    public TransmissionChannel Channel => TransmissionChannel.ChorusPro;

    public async Task<InvoiceTransmissionResult> TransmitAsync(
        InvoiceTransmissionRequest request,
        CancellationToken cancellationToken = default)
    {
        var depositRequest = new ChorusProDepositRequest
        {
            WithSignature = false,
            FileContentBase64 = request.FacturXPdfBase64,
            FileName = $"{request.DocumentReference}.pdf",
            FluxSyntax = "IN_DP_E2_CII_FACTURX",
        };

        // On ne logue jamais le contenu Base64 du PDF.
        logger.LogInformation(
            "Dépôt Chorus Pro pour le document {DocumentReference} (destinataire {RecipientSiret})",
            request.DocumentReference,
            request.RecipientSiret);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            ApiResponse<ChorusProDepositResponse> response =
                await api.DepositFluxAsync(depositRequest, cancellationToken);

            stopwatch.Stop();

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Échec dépôt Chorus Pro {DocumentReference} : HTTP {StatusCode} en {ElapsedMs} ms",
                    request.DocumentReference,
                    (int)response.StatusCode,
                    stopwatch.ElapsedMilliseconds);

                return (int)response.StatusCode >= 500
                    ? InvoiceTransmissionResult.TechnicalFailure(
                        $"Erreur serveur Chorus Pro (HTTP {(int)response.StatusCode}).")
                    : InvoiceTransmissionResult.Rejected(
                        $"Requête rejetée par Chorus Pro (HTTP {(int)response.StatusCode}).");
            }

            ChorusProDepositResponse? content = response.Content;

            logger.LogInformation(
                "Réponse Chorus Pro {DocumentReference} : HTTP {StatusCode} en {ElapsedMs} ms",
                request.DocumentReference,
                (int)response.StatusCode,
                stopwatch.ElapsedMilliseconds);

            if (content is null)
            {
                return InvoiceTransmissionResult.TechnicalFailure("Réponse Chorus Pro vide.");
            }

            if (content.ReturnCode == 0)
            {
                return InvoiceTransmissionResult.Submitted(
                    content.DepositFluxNumber ?? string.Empty,
                    content.Label);
            }

            return InvoiceTransmissionResult.Rejected(
                content.Label ?? $"Code retour Chorus Pro {content.ReturnCode}.",
                content.Label);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();

            logger.LogError(
                exception,
                "Timeout lors du dépôt Chorus Pro {DocumentReference} après {ElapsedMs} ms",
                request.DocumentReference,
                stopwatch.ElapsedMilliseconds);

            return InvoiceTransmissionResult.Timeout(exception.Message);
        }
        catch (Exception exception) when (exception is HttpRequestException or ApiException)
        {
            stopwatch.Stop();

            logger.LogError(
                exception,
                "Erreur technique lors du dépôt Chorus Pro {DocumentReference} après {ElapsedMs} ms",
                request.DocumentReference,
                stopwatch.ElapsedMilliseconds);

            return InvoiceTransmissionResult.TechnicalFailure(exception.Message);
        }
    }
}
