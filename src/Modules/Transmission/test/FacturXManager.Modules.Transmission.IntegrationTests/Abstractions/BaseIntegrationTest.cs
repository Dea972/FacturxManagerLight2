using Bogus;
using FacturXManager.Common.Application.Transmission;
using FacturXManager.Modules.Transmission.Application.Abstractions.Data;
using FacturXManager.Modules.Transmission.Domain.Transmissions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ClearExtensions;
using DomainTransmission = FacturXManager.Modules.Transmission.Domain.Transmissions.Transmission;

namespace FacturXManager.Modules.Transmission.IntegrationTests.Abstractions;

[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest : IDisposable
{
    protected static readonly Faker Faker = new();

    protected readonly IServiceScope Scope;

    protected readonly ISender Sender;

    protected readonly IServiceProvider ServiceProvider;

    protected readonly IInvoiceTransmitter Transmitter;

    protected readonly ITransmissionRepository TransmissionRepository;

    protected readonly IUnitOfWork UnitOfWork;

    protected readonly Dictionary<Guid, DomainTransmission> Transmissions;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _ = factory.CreateClient();
        Scope = factory.Services.CreateScope();
        ServiceProvider = Scope.ServiceProvider;
        Sender = ServiceProvider.GetRequiredService<ISender>();
        Transmitter = factory.Transmitter;
        TransmissionRepository = factory.TransmissionRepository;
        UnitOfWork = factory.UnitOfWork;
        Transmissions = factory.Transmissions;

        // Reset state between tests
        Transmitter.ClearReceivedCalls();
        Transmitter.ClearSubstitute();
        UnitOfWork.ClearReceivedCalls();
        TransmissionRepository.ClearReceivedCalls();
        Transmissions.Clear();
    }

    public void Dispose()
    {
        Scope.Dispose();
        GC.SuppressFinalize(this);
    }
}
