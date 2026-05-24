using System.Net.Http.Headers;
using Microsoft.Extensions.Options;

namespace FacturXManager.Modules.Directory.Infrastructure.Sirene;

/// <summary>
/// Injecte le token Bearer SIRENE sur chaque appel sortant. Le token est lu via
/// <see cref="IOptions{TOptions}"/> et n'est jamais journalisé.
/// </summary>
internal sealed class SireneBearerAuthHandler(IOptions<SireneOptions> options) : DelegatingHandler
{
    private readonly SireneOptions _options = options.Value;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_options.BearerToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.BearerToken);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
