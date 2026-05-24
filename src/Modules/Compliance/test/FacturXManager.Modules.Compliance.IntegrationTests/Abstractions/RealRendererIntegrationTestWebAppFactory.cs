using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace FacturXManager.Modules.Compliance.IntegrationTests.Abstractions;

/// <summary>
/// Factory sans stub du renderer PDF/A-3b — utilise l'implémentation réelle <c>FacturXPdfA3BRenderer</c>.
/// Destinée aux tests vérifiant la production effective d'un PDF valide.
/// </summary>
public sealed class RealRendererIntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Database"] = "Server=test;Database=facturxmanager_tests;Integrated Security=True;",
        }));
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public new Task DisposeAsync() => Task.CompletedTask;
}
