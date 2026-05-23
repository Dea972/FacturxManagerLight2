using Bogus;
using FacturXManager.Modules.EReporting.Application.Abstractions;
using FacturXManager.Modules.EReporting.Application.Abstractions.Data;
using FacturXManager.Modules.EReporting.Domain.Declarations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using NSubstitute.ClearExtensions;

namespace FacturXManager.Modules.EReporting.IntegrationTests.Abstractions;

[Collection(nameof(IntegrationTestCollection))]
public abstract class BaseIntegrationTest : IDisposable
{
    protected static readonly Faker Faker = new();

    protected readonly IServiceScope Scope;

    protected readonly ISender Sender;

    protected readonly IServiceProvider ServiceProvider;

    protected readonly IDgfipReportingClient DgfipClient;

    protected readonly IDeclarationRepository DeclarationRepository;

    protected readonly IUnitOfWork UnitOfWork;

    protected readonly Dictionary<Guid, Declaration> Declarations;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        _ = factory.CreateClient();
        Scope = factory.Services.CreateScope();
        ServiceProvider = Scope.ServiceProvider;
        Sender = ServiceProvider.GetRequiredService<ISender>();
        DgfipClient = factory.DgfipClient;
        DeclarationRepository = factory.DeclarationRepository;
        UnitOfWork = factory.UnitOfWork;
        Declarations = factory.Declarations;

        DgfipClient.ClearReceivedCalls();
        DgfipClient.ClearSubstitute();
        UnitOfWork.ClearReceivedCalls();
        DeclarationRepository.ClearReceivedCalls();
        Declarations.Clear();
    }

    public void Dispose()
    {
        Scope.Dispose();
        GC.SuppressFinalize(this);
    }
}
