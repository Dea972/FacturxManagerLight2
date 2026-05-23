using System.Reflection;
using FacturXManager.Modules.Transmission.Infrastructure;

namespace FacturXManager.Modules.Transmission.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = typeof(Transmission.Application.AssemblyReference).Assembly;

    protected static readonly Assembly InfrastructureAssembly = typeof(TransmissionModule).Assembly;

    protected static readonly Assembly PresentationAssembly = typeof(Transmission.Presentation.AssemblyReference).Assembly;
}
