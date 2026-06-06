using Bogus;
using FacturXManager.Modules.Transmission.Infrastructure.Database;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace FacturXManager.Modules.Transmission.IntegrationTests.Abstractions;

[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest : IDisposable
{
    protected static readonly Faker Faker = new();

    protected readonly IServiceScope Scope;

    protected readonly ISender Sender;

    protected readonly IServiceProvider ServiceProvider;

    protected readonly TransmissionDbContext DbContext;

    /// <summary>Serveur WireMock partagé. Stubs réinitialisés entre chaque test.</summary>
    protected readonly WireMockServer WireMock;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _ = factory.CreateClient();
        Scope = factory.Services.CreateScope();
        ServiceProvider = Scope.ServiceProvider;
        Sender = ServiceProvider.GetRequiredService<ISender>();
        DbContext = ServiceProvider.GetRequiredService<TransmissionDbContext>();
        WireMock = factory.WireMock;

        WireMock.Reset();
        StubOAuth2(WireMock);
    }

    public void Dispose()
    {
        Scope.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Réenregistre le stub OAuth2 minimal après un <c>Reset()</c>.
    /// </summary>
    protected static void StubOAuth2(WireMockServer server)
    {
        server
            .Given(Request.Create().WithPath("/oauth/token").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""{"access_token":"test-token","expires_in":3600,"token_type":"Bearer"}"""));
    }
}

