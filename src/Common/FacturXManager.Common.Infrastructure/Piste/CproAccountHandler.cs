using Microsoft.Extensions.Options;

namespace FacturXManager.Common.Infrastructure.Piste;

/// <summary>
/// Injecte le header <c>cpro-account</c> (base64(login:password)) requis par Chorus Pro
/// sur chaque appel sortant. La valeur est lue depuis la configuration et n'est jamais loggée.
/// </summary>
public sealed class CproAccountHandler(IOptions<PisteAuthOptions> options) : DelegatingHandler
{
    private readonly PisteAuthOptions _options = options.Value;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(_options.CproLogin))
        {
            request.Headers.TryAddWithoutValidation("cpro-account", _options.CproAccountBase64);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
