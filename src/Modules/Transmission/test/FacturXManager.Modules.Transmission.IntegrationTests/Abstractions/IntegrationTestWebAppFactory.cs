using FacturXManager.Common.Application.Transmission;
using FacturXManager.Modules.Transmission.Application.Abstractions.Data;
using FacturXManager.Modules.Transmission.Domain.Transmissions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;
using DomainTransmission = FacturXManager.Modules.Transmission.Domain.Transmissions.Transmission;

namespace FacturXManager.Modules.Transmission.IntegrationTests.Abstractions;

/// <summary>
/// Compose l'API in-memory pour les tests d'intégration du module Transmission.
/// L'état externe (canal d'acheminement Chorus Pro / PDP / PPF) est mocké via
/// <see cref="IInvoiceTransmitter"/>. Le repository et l'unit-of-work sont également
/// mockés (in-memory) afin de ne dépendre d'aucune base de données.
/// </summary>
public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public IInvoiceTransmitter Transmitter { get; } = Substitute.For<IInvoiceTransmitter>();

    public ITransmissionRepository TransmissionRepository { get; } = Substitute.For<ITransmissionRepository>();

    public IUnitOfWork UnitOfWork { get; } = Substitute.For<IUnitOfWork>();

    /// <summary>Mémoire partagée simulant le repository de transmissions.</summary>
    public Dictionary<Guid, DomainTransmission> Transmissions { get; } = [];

    public IntegrationTestWebAppFactory()
    {
        TransmissionRepository
            .GetAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(call => Transmissions.GetValueOrDefault((Guid)call[0]!));

        TransmissionRepository
            .When(x => x.Insert(Arg.Any<DomainTransmission>()))
            .Do(call =>
            {
                var t = (DomainTransmission)call[0]!;
                Transmissions[t.Id] = t;
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
            services.RemoveAll<IInvoiceTransmitter>();
            services.AddSingleton(Transmitter);

            services.RemoveAll<ITransmissionRepository>();
            services.AddSingleton(TransmissionRepository);

            services.RemoveAll<IUnitOfWork>();
            services.AddSingleton(UnitOfWork);
        });
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public new Task DisposeAsync() => Task.CompletedTask;
}
