using NetArchTest.Rules;
using Xunit;

namespace Rtl.Core.ArchitectureTests;

/// <summary>
/// Architecture tests for the Application layer.
/// Application depends on Domain but should not depend on Infrastructure or Presentation.
/// </summary>
public sealed class ApplicationLayerTests : BaseTest
{
    #region Common.Application Tests

    [Fact]
    public void CommonApplication_ShouldNotDependOn_Infrastructure()
    {
        if (CommonApplicationAssembly is null)
        {
            Assert.Fail("Common.Application assembly not found. Ensure naming convention is followed.");
            return;
        }

        var result = Types.InAssembly(CommonApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(GetCommonNamespace("Infrastructure"))
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Common.Application should not depend on any Infrastructure layer");
    }

    [Fact]
    public void CommonApplication_ShouldNotDependOn_Presentation()
    {
        if (CommonApplicationAssembly is null)
        {
            Assert.Fail("Common.Application assembly not found. Ensure naming convention is followed.");
            return;
        }

        var result = Types.InAssembly(CommonApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOn(GetCommonNamespace("Presentation"))
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Common.Application should not depend on any Presentation layer");
    }

    [Fact]
    public void CommonApplication_ShouldNotDependOn_AnyModule()
    {
        if (CommonApplicationAssembly is null)
        {
            Assert.Fail("Common.Application assembly not found. Ensure naming convention is followed.");
            return;
        }

        var moduleNamespaces = GetAllModuleNamespaces();
        if (moduleNamespaces.Length == 0)
        {
            return; // No modules to check against
        }

        var result = Types.InAssembly(CommonApplicationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(moduleNamespaces)
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Common.Application should not depend on any module-specific code");
    }

    #endregion

    #region Module Application Tests (Auto-discovered)

    [Fact]
    public void AllModuleApplications_ShouldNotDependOn_Infrastructure()
    {
        var applications = GetModuleAssemblies("Application");

        foreach (var (moduleName, assembly) in applications)
        {
            var forbiddenNamespaces = new[]
            {
                GetCommonNamespace("Infrastructure"),
                GetModuleNamespace(moduleName, "Infrastructure")
            };

            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(forbiddenNamespaces)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{moduleName}.Application should not depend on any Infrastructure layer. " +
                $"Found dependencies in: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }
    }

    [Fact]
    public void AllModuleApplications_ShouldNotDependOn_Presentation()
    {
        var applications = GetModuleAssemblies("Application");

        foreach (var (moduleName, assembly) in applications)
        {
            var forbiddenNamespaces = new[]
            {
                GetCommonNamespace("Presentation"),
                GetModuleNamespace(moduleName, "Presentation")
            };

            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(forbiddenNamespaces)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{moduleName}.Application should not depend on any Presentation layer. " +
                $"Found dependencies in: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }
    }

    [Fact]
    public void AllModuleApplications_ShouldNotDependOn_OtherModules()
    {
        var applications = GetModuleAssemblies("Application");

        foreach (var (moduleName, assembly) in applications)
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
                $"{moduleName}.Application should not depend on other modules. " +
                $"Found dependencies in: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }
    }

    /// <summary>
    /// Application CAN depend on Common.Domain, Common.Application, and its own Domain.
    /// This test documents this allowed dependency.
    /// </summary>
    [Fact]
    public void ApplicationLayer_AllowedDependencies_Documentation()
    {
        // Application layer is allowed to depend on:
        // - Common.Domain (shared kernel)
        // - Common.Application (shared application services, abstractions)
        // - Module.Domain (same module only)
        //
        // Application layer should NOT depend on:
        // - Infrastructure layer
        // - Presentation layer
        // - Other modules

        Assert.True(true, "Application layer dependency rules documented above");
    }

    #endregion
}
