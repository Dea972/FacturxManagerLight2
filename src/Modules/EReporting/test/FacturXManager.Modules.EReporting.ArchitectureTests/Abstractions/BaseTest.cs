using System.Reflection;
using FacturXManager.Modules.EReporting.Infrastructure;

namespace FacturXManager.Modules.EReporting.ArchitectureTests.Abstractions;

public abstract class BaseTest
{
    protected static readonly Assembly ApplicationAssembly = typeof(EReporting.Application.AssemblyReference).Assembly;

    protected static readonly Assembly InfrastructureAssembly = typeof(EReportingModule).Assembly;

    protected static readonly Assembly PresentationAssembly = typeof(EReporting.Presentation.AssemblyReference).Assembly;
}
