using NetArchTest.Rules;
using Xunit;

namespace ModularTemplate.ArchitectureTests;

/// <summary>
/// Architecture tests for the Presentation layer.
/// Presentation depends on Application and Domain but should not depend on Infrastructure.
/// </summary>
public sealed class PresentationLayerTests : BaseTest
{
    #region Common.Presentation Tests

    [Fact]
    public void CommonPresentation_ShouldNotDependOn_Infrastructure()
    {
        if (CommonPresentationAssembly is null)
        {
            Assert.Fail("Common.Presentation assembly not found. Ensure naming convention is followed.");
            return;
        }

        var result = Types.InAssembly(CommonPresentationAssembly)
            .ShouldNot()
            .HaveDependencyOn(GetCommonNamespace("Infrastructure"))
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Common.Presentation should not depend on any Infrastructure layer");
    }

    [Fact]
    public void CommonPresentation_ShouldNotDependOn_AnyModule()
    {
        if (CommonPresentationAssembly is null)
        {
            Assert.Fail("Common.Presentation assembly not found. Ensure naming convention is followed.");
            return;
        }

        var moduleNamespaces = GetAllModuleNamespaces();
        if (moduleNamespaces.Length == 0)
        {
            return; // No modules to check against
        }

        var result = Types.InAssembly(CommonPresentationAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(moduleNamespaces)
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Common.Presentation should not depend on any module-specific code");
    }

    #endregion

    #region Module Presentation Tests (Auto-discovered)

    [Fact]
    public void AllModulePresentations_ShouldNotDependOn_Infrastructure()
    {
        var presentations = GetModuleAssemblies("Presentation");

        foreach (var (moduleName, assembly) in presentations)
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
                $"{moduleName}.Presentation should not depend on any Infrastructure layer. " +
                $"Found dependencies in: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }
    }

    /// <summary>
    /// Verifies Presentation layers don't depend on other modules' internal layers.
    /// Note: Presentation CAN depend on other modules' IntegrationEvents (to handle incoming events).
    /// </summary>
    [Fact]
    public void AllModulePresentations_ShouldNotDependOn_OtherModules()
    {
        var presentations = GetModuleAssemblies("Presentation");

        foreach (var (moduleName, assembly) in presentations)
        {
            // Get other modules' non-IntegrationEvents namespaces (Domain, Application, Infrastructure, Presentation)
            // Presentation IS allowed to depend on other modules' IntegrationEvents
            var forbiddenNamespaces = GetOtherModuleNonEventNamespaces(moduleName);
            if (forbiddenNamespaces.Length == 0)
            {
                continue; // Only one module exists
            }

            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(forbiddenNamespaces)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{moduleName}.Presentation should not depend on other modules' internal layers. " +
                $"(IntegrationEvents ARE allowed) Found dependencies in: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }
    }

    /// <summary>
    /// Presentation CAN depend on Common.Domain, Common.Application, Common.Presentation,
    /// and its own Domain and Application layers.
    /// This test documents this allowed dependency.
    /// </summary>
    [Fact]
    public void PresentationLayer_AllowedDependencies_Documentation()
    {
        // Presentation layer is allowed to depend on:
        // - Common.Domain (shared kernel)
        // - Common.Application (shared application services)
        // - Common.Presentation (shared presentation utilities)
        // - Module.Domain (same module only)
        // - Module.Application (same module only)
        //
        // Presentation layer should NOT depend on:
        // - Infrastructure layer
        // - Other modules

        Assert.True(true, "Presentation layer dependency rules documented above");
    }

    #endregion
}
