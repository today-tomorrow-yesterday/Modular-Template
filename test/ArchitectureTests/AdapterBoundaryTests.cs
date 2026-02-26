using NetArchTest.Rules;
using System.Reflection;
using Xunit;

namespace Rtl.Core.ArchitectureTests;

/// <summary>
/// Architecture tests enforcing adapter boundary rules.
/// Ensures adapter models use domain-friendly types and wire-format details stay in Infrastructure.
/// </summary>
public sealed class AdapterBoundaryTests : BaseTest
{
    private const string AdapterNamespace = "Rtl.Core.Application.Adapters.ISeries";
    private const string WireModelsNamespace = "Rtl.Core.Infrastructure.ISeries.WireModels";

    /// <summary>
    /// Adapter models in the Application layer must not use char properties.
    /// Char properties indicate iSeries wire-format leaking into the application contract.
    /// Use enums (HomeCondition, ModularClassification, OccupancyType) instead.
    /// </summary>
    [Fact]
    public void AdapterModels_ShouldNotHaveCharProperties()
    {
        if (CommonApplicationAssembly is null)
        {
            Assert.Fail("Common.Application assembly not found.");
            return;
        }

        var adapterTypes = CommonApplicationAssembly.GetTypes()
            .Where(t => t.Namespace is not null && t.Namespace.StartsWith(AdapterNamespace))
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface)
            .ToList();

        var violations = new List<string>();

        foreach (var type in adapterTypes)
        {
            var charProps = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.PropertyType == typeof(char) || p.PropertyType == typeof(char?))
                .ToList();

            foreach (var prop in charProps)
            {
                violations.Add($"{type.Name}.{prop.Name} uses char — replace with a domain enum");
            }
        }

        Assert.True(violations.Count == 0,
            $"Adapter models must not use char properties (iSeries wire-format leak):\n" +
            $"{string.Join("\n", violations)}");
    }

    /// <summary>
    /// Module layers (Domain, Application, Presentation) must not depend on
    /// Infrastructure wire-format models. Only the Infrastructure adapter itself should know about them.
    /// </summary>
    [Fact]
    public void ModuleLayers_ShouldNotDependOn_WireModels()
    {
        var layersToCheck = new[] { "Domain", "Application", "Presentation" };

        foreach (var layer in layersToCheck)
        {
            var assemblies = GetModuleAssemblies(layer);

            foreach (var (moduleName, assembly) in assemblies)
            {
                var result = Types.InAssembly(assembly)
                    .ShouldNot()
                    .HaveDependencyOn(WireModelsNamespace)
                    .GetResult();

                Assert.True(result.IsSuccessful,
                    $"{moduleName}.{layer} should not depend on iSeries wire-format models ({WireModelsNamespace}). " +
                    $"Found dependencies in: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
            }
        }

        // Also check Common Application and Domain
        if (CommonApplicationAssembly is not null)
        {
            var result = Types.InAssembly(CommonApplicationAssembly)
                .ShouldNot()
                .HaveDependencyOn(WireModelsNamespace)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"Common.Application should not depend on iSeries wire-format models.");
        }

        if (CommonDomainAssembly is not null)
        {
            var result = Types.InAssembly(CommonDomainAssembly)
                .ShouldNot()
                .HaveDependencyOn(WireModelsNamespace)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"Common.Domain should not depend on iSeries wire-format models.");
        }
    }
}
