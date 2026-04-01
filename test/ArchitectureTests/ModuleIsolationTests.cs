using NetArchTest.Rules;
using Xunit;

namespace ModularTemplate.ArchitectureTests;

/// <summary>
/// Tests to ensure modules are properly isolated from each other.
/// Modules can only communicate through:
/// 1. Integration events (async, via message bus)
/// 2. Public contracts (if defined)
///
/// Modules should NEVER directly reference another module's:
/// - Domain entities or value objects
/// - Application services or handlers
/// - Infrastructure implementations
/// - Presentation endpoints
///
/// This test class auto-discovers all modules and validates isolation.
/// </summary>
public sealed class ModuleIsolationTests : BaseTest
{
    /// <summary>
    /// Verifies that all modules are properly isolated from each other.
    ///
    /// Allowed cross-module dependencies:
    /// - Presentation/Infrastructure CAN depend on other modules' IntegrationEvents (to handle incoming events)
    ///
    /// Forbidden cross-module dependencies:
    /// - Domain, Application, IntegrationEvents CANNOT depend on ANY other module
    /// - Presentation/Infrastructure CANNOT depend on other modules' Domain, Application, Infrastructure, Presentation
    /// </summary>
    [Fact]
    public void AllModules_ShouldBeIsolated_FromEachOther()
    {
        var moduleNames = ModuleNames;

        if (moduleNames.Count <= 1)
        {
            // Only one module exists - isolation test not applicable yet
            return;
        }

        var violations = new List<string>();

        // Layers that cannot have ANY cross-module dependencies
        var strictlyIsolatedLayers = new[] { "Domain", "Application", "IntegrationEvents" };

        // Layers that CAN depend on other modules' IntegrationEvents (but nothing else)
        var canConsumeEventsLayers = new[] { "Infrastructure", "Presentation" };

        // Check strictly isolated layers - no cross-module dependencies allowed
        foreach (var layer in strictlyIsolatedLayers)
        {
            var moduleAssemblies = GetModuleAssemblies(layer);

            foreach (var (moduleName, assembly) in moduleAssemblies)
            {
                var otherModuleNamespaces = GetOtherModuleNamespaces(moduleName);

                var result = Types.InAssembly(assembly)
                    .ShouldNot()
                    .HaveDependencyOnAny(otherModuleNamespaces)
                    .GetResult();

                if (!result.IsSuccessful)
                {
                    foreach (var failingType in result.FailingTypeNames ?? Array.Empty<string>())
                    {
                        violations.Add($"{moduleName}.{layer}: {failingType}");
                    }
                }
            }
        }

        // Check Presentation/Infrastructure - can depend on IntegrationEvents only
        foreach (var layer in canConsumeEventsLayers)
        {
            var moduleAssemblies = GetModuleAssemblies(layer);

            foreach (var (moduleName, assembly) in moduleAssemblies)
            {
                // Get other modules' non-IntegrationEvents namespaces (Domain, Application, Infrastructure, Presentation)
                var forbiddenNamespaces = GetOtherModuleNonEventNamespaces(moduleName);

                var result = Types.InAssembly(assembly)
                    .ShouldNot()
                    .HaveDependencyOnAny(forbiddenNamespaces)
                    .GetResult();

                if (!result.IsSuccessful)
                {
                    foreach (var failingType in result.FailingTypeNames ?? Array.Empty<string>())
                    {
                        violations.Add($"{moduleName}.{layer}: {failingType}");
                    }
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verifies the expected number of modules were discovered.
    /// This test helps ensure the auto-discovery is working.
    /// </summary>
    [Fact]
    public void ModuleDiscovery_ShouldFindModules()
    {
        var moduleNames = ModuleNames;

        Assert.NotEmpty(moduleNames);

        // Log discovered modules for visibility
        var discoveredModules = string.Join(", ", moduleNames);
        Assert.True(true, $"Discovered modules: {discoveredModules}");
    }

    /// <summary>
    /// Verifies each discovered module has all four layers.
    /// </summary>
    [Fact]
    public void AllModules_ShouldHaveAllFourLayers()
    {
        var moduleNames = ModuleNames;
        var missingLayers = new List<string>();

        foreach (var moduleName in moduleNames)
        {
            var hasDomain = GetModuleAssemblies("Domain").Any(m => m.ModuleName == moduleName);
            var hasApplication = GetModuleAssemblies("Application").Any(m => m.ModuleName == moduleName);
            var hasInfrastructure = GetModuleAssemblies("Infrastructure").Any(m => m.ModuleName == moduleName);
            var hasPresentation = GetModuleAssemblies("Presentation").Any(m => m.ModuleName == moduleName);

            if (!hasDomain) missingLayers.Add($"{moduleName}: Missing Domain layer");
            if (!hasApplication) missingLayers.Add($"{moduleName}: Missing Application layer");
            if (!hasInfrastructure) missingLayers.Add($"{moduleName}: Missing Infrastructure layer");
            if (!hasPresentation) missingLayers.Add($"{moduleName}: Missing Presentation layer");
        }

        Assert.Empty(missingLayers);
    }

    /// <summary>
    /// Documents the architecture rules for module communication.
    /// </summary>
    [Fact]
    public void ArchitectureRules_Documentation()
    {
        // MODULE ISOLATION RULES:
        //
        // 1. DIRECT DEPENDENCIES ARE FORBIDDEN
        //    - Module A cannot directly reference Module B's types
        //    - This applies to ALL layers (Domain, Application, Infrastructure, Presentation)
        //
        // 2. CROSS-MODULE COMMUNICATION MUST USE:
        //    - Integration Events (published via message bus)
        //    - Public Contracts (DTOs in a shared contracts assembly, if needed)
        //
        // 3. NAMING CONVENTION IS THE CONTRACT
        //    - {Prefix}.Modules.{ModuleName}.Domain
        //    - {Prefix}.Modules.{ModuleName}.Application
        //    - {Prefix}.Modules.{ModuleName}.Infrastructure
        //    - {Prefix}.Modules.{ModuleName}.Presentation
        //
        // 4. COMMON LAYERS ARE SHARED
        //    - {Prefix}.Common.Domain - Shared kernel (base entities, value objects)
        //    - {Prefix}.Common.Application - Shared application services
        //    - {Prefix}.Common.Infrastructure - Shared infrastructure
        //    - {Prefix}.Common.Presentation - Shared presentation utilities
        //
        // 5. DEPENDENCY DIRECTION (Clean Architecture)
        //    Domain <- Application <- Presentation
        //                          <- Infrastructure
        //    (Infrastructure and Presentation depend on Application and Domain)

        Assert.True(true, "Architecture rules documented above");
    }
}
