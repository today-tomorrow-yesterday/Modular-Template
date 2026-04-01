using NetArchTest.Rules;
using Xunit;

namespace ModularTemplate.ArchitectureTests;

/// <summary>
/// Architecture tests for the Infrastructure layer.
/// Infrastructure is the outermost layer and can depend on all other layers within its module.
/// However, it should NOT depend on other modules.
/// </summary>
public sealed class InfrastructureLayerTests : BaseTest
{
    #region Common.Infrastructure Tests

    [Fact]
    public void CommonInfrastructure_ShouldNotDependOn_AnyModule()
    {
        if (CommonInfrastructureAssembly is null)
        {
            Assert.Fail("Common.Infrastructure assembly not found. Ensure naming convention is followed.");
            return;
        }

        var moduleNamespaces = GetAllModuleNamespaces();
        if (moduleNamespaces.Length == 0)
        {
            return; // No modules to check against
        }

        var result = Types.InAssembly(CommonInfrastructureAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(moduleNamespaces)
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Common.Infrastructure should not depend on any module-specific code");
    }

    #endregion

    #region Module Infrastructure Tests

    [Fact]
    public void AllModuleInfrastructures_ShouldNotDependOn_OtherModules()
    {
        var infrastructures = GetModuleAssemblies("Infrastructure");

        foreach (var (moduleName, assembly) in infrastructures)
        {
            var otherModuleNamespaces = GetOtherModuleNamespaces(moduleName);
            if (otherModuleNamespaces.Length == 0)
            {
                continue; // Only one module exists
            }

            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(otherModuleNamespaces)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{moduleName}.Infrastructure should not depend on other modules. " +
                $"Found dependencies in: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }
    }

    /// <summary>
    /// Infrastructure CAN depend on its own module's Domain, Application, and Presentation layers.
    /// This test documents this allowed dependency.
    /// </summary>
    [Fact]
    public void InfrastructureLayer_AllowedDependencies_Documentation()
    {
        // Infrastructure layer is allowed to depend on:
        // - Common.Domain
        // - Common.Application
        // - Common.Infrastructure
        // - Common.Presentation (for assembly scanning in some cases)
        // - Module.Domain (same module)
        // - Module.Application (same module)
        // - Module.Presentation (same module, for handler discovery)
        //
        // Infrastructure layer should NOT depend on:
        // - Other modules (cross-module dependencies)

        Assert.True(true, "Infrastructure layer dependency rules documented above");
    }

    #endregion
}
