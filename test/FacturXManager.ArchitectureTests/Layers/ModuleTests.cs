using System.Reflection;
using FacturXManager.ArchitectureTests.Abstractions;
using FacturXManager.Modules.Compliance.Infrastructure;
using FacturXManager.Modules.Directory.Infrastructure;
using FacturXManager.Modules.EReporting.Infrastructure;
using FacturXManager.Modules.Transmission.Infrastructure;
using NetArchTest.Rules;

namespace FacturXManager.ArchitectureTests.Layers;

public class ModuleTests : BaseTest
{
    [Fact]
    public void ComplianceModule_ShouldNotHaveDependencyOn_AnyOtherModule()
    {
        string[] otherModules = [TransmissionNamespace, EReportingNamespace, DirectoryNamespace];

        List<Assembly> complianceAssemblies =
        [
            Modules.Compliance.Application.AssemblyReference.Assembly,
            Modules.Compliance.Presentation.AssemblyReference.Assembly,
            typeof(ComplianceModule).Assembly
        ];

        Types.InAssemblies(complianceAssemblies)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }


    [Fact]
    public void EReportingModule_ShouldNotHaveDependencyOn_AnyOtherModule()
    {
        string[] otherModules = [ComplianceNamespace, TransmissionNamespace, DirectoryNamespace];

        List<Assembly> EReportingAssemblies =
        [
            Modules.EReporting.Application.AssemblyReference.Assembly,
            Modules.EReporting.Presentation.AssemblyReference.Assembly,
            typeof(EReportingModule).Assembly
        ];

        Types.InAssemblies(EReportingAssemblies)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }

    [Fact]
    public void DirectoryModule_ShouldNotHaveDependencyOn_AnyOtherModule()
    {
        string[] otherModules = [ComplianceNamespace, TransmissionNamespace, EReportingNamespace];

        List<Assembly> directoryAssemblies =
        [
            FacturXManager.Modules.Directory.Application.AssemblyReference.Assembly,
            FacturXManager.Modules.Directory.Presentation.AssemblyReference.Assembly,
            typeof(DirectoryModule).Assembly
        ];

        Types.InAssemblies(directoryAssemblies)
            .Should()
            .NotHaveDependencyOnAny(otherModules)
            .GetResult()
            .ShouldBeSuccessful();
    }
}
