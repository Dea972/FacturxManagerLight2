using FacturXManager.Modules.Compliance.Application.Abstractions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace FacturXManager.Modules.Compliance.IntegrationTests.Abstractions;

/// <summary>
/// Compose l'API complète en mémoire. Le seul état externe (rendu PDF/A-3b) est mocké via NSubstitute.
/// Aucune base de données : Compliance est un module processeur sans persistance.
/// </summary>
public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public IFacturXPdfA3BRenderer PdfRenderer { get; } = Substitute.For<IFacturXPdfA3BRenderer>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Database"] = "Server=test;Database=facturxmanager_tests;Integrated Security=True;",
        }));

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IFacturXPdfA3BRenderer>();
            services.AddSingleton(PdfRenderer);
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public new Task DisposeAsync() => Task.CompletedTask;
}
