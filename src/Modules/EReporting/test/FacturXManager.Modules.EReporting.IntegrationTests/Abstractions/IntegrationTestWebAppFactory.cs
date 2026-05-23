using FacturXManager.Modules.EReporting.Application.Abstractions;
using FacturXManager.Modules.EReporting.Application.Abstractions.Data;
using FacturXManager.Modules.EReporting.Domain.Declarations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace FacturXManager.Modules.EReporting.IntegrationTests.Abstractions;

/// <summary>
/// Compose l'API in-memory pour les tests d'intégration du module EReporting.
/// L'état externe (API DGFiP) est mocké via <see cref="IDgfipReportingClient"/>.
/// Le repository et l'unit-of-work sont également mockés (in-memory), aucune base de données.
/// </summary>
public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public IDgfipReportingClient DgfipClient { get; } = Substitute.For<IDgfipReportingClient>();

    public IDeclarationRepository DeclarationRepository { get; } = Substitute.For<IDeclarationRepository>();

    public IUnitOfWork UnitOfWork { get; } = Substitute.For<IUnitOfWork>();

    /// <summary>Mémoire partagée simulant le repository de déclarations.</summary>
    public Dictionary<Guid, Declaration> Declarations { get; } = [];

    public IntegrationTestWebAppFactory()
    {
        DeclarationRepository
            .GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(call => Declarations.GetValueOrDefault((Guid)call[0]!));

        DeclarationRepository
            .When(x => x.Insert(Arg.Any<Declaration>()))
            .Do(call =>
            {
                var d = (Declaration)call[0]!;
                Declarations[d.Id] = d;
            });

        UnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");

        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Database"] = "Server=test;Database=facturxmanager_tests;Integrated Security=True;",
        }));

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IDgfipReportingClient>();
            services.AddSingleton(DgfipClient);

            services.RemoveAll<IDeclarationRepository>();
            services.AddSingleton(DeclarationRepository);

            services.RemoveAll<IUnitOfWork>();
            services.AddSingleton(UnitOfWork);
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public new Task DisposeAsync() => Task.CompletedTask;
}
