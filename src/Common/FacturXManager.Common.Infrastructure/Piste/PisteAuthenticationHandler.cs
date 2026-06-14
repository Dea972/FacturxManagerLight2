using System.Net.Http.Headers;
using System.Net.Http.Json;
using FacturXManager.Common.Application.Caching;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FacturXManager.Common.Infrastructure.Piste;

/// <summary>
/// Injecte le token OAuth2 PISTE (client_credentials) sur chaque appel sortant.
/// Le token est mis en cache et renouvelé avant expiration ; il n'est jamais loggué.
/// </summary>
public sealed class PisteAuthenticationHandler(
    IHttpClientFactory httpClientFactory,
    ICacheService cacheService,
    IOptions<PisteAuthOptions> options,
    ILogger<PisteAuthenticationHandler> logger)
    : DelegatingHandler
{
    private const string CacheKey = "piste:access-token";

    private readonly PisteAuthOptions _options = options.Value;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        string token = await GetAccessTokenAsync(cancellationToken);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        string? cached = await cacheService.GetAsync<string>(CacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cached))
        {
            return cached;
        }

        logger.LogInformation("Demande d'un nouveau token OAuth2 PISTE auprès de {TokenEndpoint}", _options.TokenEndpoint);

        using HttpClient client = httpClientFactory.CreateClient("piste-auth");

        using var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _options.ClientId,
            ["client_secret"] = _options.ClientSecret,
            ["scope"] = _options.Scope,
        });

        using HttpResponseMessage response = await client.PostAsync(_options.TokenEndpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        PisteTokenResponse? tokenResponse =
            await response.Content.ReadFromJsonAsync<PisteTokenResponse>(cancellationToken);

        if (tokenResponse is null || string.IsNullOrEmpty(tokenResponse.AccessToken))
        {
            throw new InvalidOperationException("La réponse du serveur OAuth2 PISTE ne contient pas de token.");
        }

        int ttl = Math.Max(tokenResponse.ExpiresInSeconds - _options.RenewBeforeExpirySeconds, 30);
        await cacheService.SetAsync(CacheKey, tokenResponse.AccessToken, TimeSpan.FromSeconds(ttl), cancellationToken);

        logger.LogInformation("Token OAuth2 PISTE obtenu, mis en cache pour {Ttl} s", ttl);

        return tokenResponse.AccessToken;
    }
}
