using NetArchTest.Rules;
using Rtl.Core.Domain.Entities;
using System.Reflection;
using Xunit;

namespace Rtl.Core.ArchitectureTests;

/// <summary>
/// Architecture tests for the Domain layer.
/// Domain is the innermost layer and should have no dependencies on outer layers.
/// Module domains may ONLY depend on Common.Domain.
/// </summary>
public sealed class DomainLayerTests : BaseTest
{
    #region Common.Domain Tests

    [Fact]
    public void CommonDomain_ShouldNotDependOn_Application()
    {
        if (CommonDomainAssembly is null)
        {
            Assert.Fail("Common.Domain assembly not found. Ensure naming convention is followed.");
            return;
        }

        var result = Types.InAssembly(CommonDomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(GetCommonNamespace("Application"))
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Common.Domain should not depend on any Application layer");
    }

    [Fact]
    public void CommonDomain_ShouldNotDependOn_Infrastructure()
    {
        if (CommonDomainAssembly is null)
        {
            Assert.Fail("Common.Domain assembly not found. Ensure naming convention is followed.");
            return;
        }

        var result = Types.InAssembly(CommonDomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(GetCommonNamespace("Infrastructure"))
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Common.Domain should not depend on any Infrastructure layer");
    }

    [Fact]
    public void CommonDomain_ShouldNotDependOn_Presentation()
    {
        if (CommonDomainAssembly is null)
        {
            Assert.Fail("Common.Domain assembly not found. Ensure naming convention is followed.");
            return;
        }

        var result = Types.InAssembly(CommonDomainAssembly)
            .ShouldNot()
            .HaveDependencyOn(GetCommonNamespace("Presentation"))
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Common.Domain should not depend on any Presentation layer");
    }

    [Fact]
    public void CommonDomain_ShouldNotDependOn_AnyModule()
    {
        if (CommonDomainAssembly is null)
        {
            Assert.Fail("Common.Domain assembly not found. Ensure naming convention is followed.");
            return;
        }

        var moduleNamespaces = GetAllModuleNamespaces();
        if (moduleNamespaces.Length == 0)
        {
            return; // No modules to check against
        }

        var result = Types.InAssembly(CommonDomainAssembly)
            .ShouldNot()
            .HaveDependencyOnAny(moduleNamespaces)
            .GetResult();

        Assert.True(result.IsSuccessful,
            "Common.Domain should not depend on any module-specific code");
    }

    #endregion

    #region Module Domain Tests (Auto-discovered)

    [Fact]
    public void AllModuleDomains_ShouldNotDependOn_Application()
    {
        var domains = GetModuleAssemblies("Domain");

        foreach (var (moduleName, assembly) in domains)
        {
            var forbiddenNamespaces = new[]
            {
                GetCommonNamespace("Application"),
                GetModuleNamespace(moduleName, "Application")
            };

            var result = Types.InAssembly(assembly)
                .ShouldNot()
                .HaveDependencyOnAny(forbiddenNamespaces)
                .GetResult();

            Assert.True(result.IsSuccessful,
                $"{moduleName}.Domain should not depend on any Application layer. " +
                $"Found dependencies in: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }
    }

    [Fact]
    public void AllModuleDomains_ShouldNotDependOn_Infrastructure()
    {
        var domains = GetModuleAssemblies("Domain");

        foreach (var (moduleName, assembly) in domains)
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
                $"{moduleName}.Domain should not depend on any Infrastructure layer. " +
                $"Found dependencies in: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }
    }

    [Fact]
    public void AllModuleDomains_ShouldNotDependOn_Presentation()
    {
        var domains = GetModuleAssemblies("Domain");

        foreach (var (moduleName, assembly) in domains)
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
                $"{moduleName}.Domain should not depend on any Presentation layer. " +
                $"Found dependencies in: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }
    }

    [Fact]
    public void AllModuleDomains_ShouldNotDependOn_OtherModules()
    {
        var domains = GetModuleAssemblies("Domain");

        foreach (var (moduleName, assembly) in domains)
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
                $"{moduleName}.Domain should not depend on other modules. " +
                $"Found dependencies in: {string.Join(", ", result.FailingTypeNames ?? Array.Empty<string>())}");
        }
    }

    /// <summary>
    /// Domain CAN depend on Common.Domain (shared kernel).
    /// This test documents this allowed dependency.
    /// </summary>
    [Fact]
    public void DomainLayer_AllowedDependencies_Documentation()
    {
        // Domain layer is allowed to depend on:
        // - Common.Domain (shared kernel with base entities, value objects, domain events)
        //
        // Domain layer should NOT depend on:
        // - Application layer
        // - Infrastructure layer
        // - Presentation layer
        // - Other modules

        Assert.True(true, "Domain layer dependency rules documented above");
    }

    #endregion

    #region Aggregate Root Enforcement Tests

    /// <summary>
    /// Non-aggregate entities (entities not implementing IAggregateRoot) should only have internal methods.
    /// This enforces that child entities can only be accessed through their aggregate root.
    /// Public properties are allowed (needed for EF Core and read access).
    /// </summary>
    [Fact]
    public void NonAggregateEntities_ShouldOnlyHaveInternalMethods()
    {
        var domains = GetModuleAssemblies("Domain");
        var violations = new List<string>();

        // Methods that are allowed to be public (inherited from Entity/object or special)
        var allowedPublicMethods = new HashSet<string>
        {
            // From Entity base class
            "ClearDomainEvents",
            // From object
            "GetType",
            "ToString",
            "Equals",
            "GetHashCode",
            // Property accessors are handled separately
        };

        foreach (var (moduleName, assembly) in domains)
        {
            var nonAggregateEntities = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(t => IsEntityType(t))
                .Where(t => !typeof(IAggregateRoot).IsAssignableFrom(t))
                .ToList();

            foreach (var entityType in nonAggregateEntities)
            {
                // Get public methods declared on this type (not inherited)
                var publicMethods = entityType.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                    .Where(m => !m.IsSpecialName) // Exclude property getters/setters, operators
                    .Where(m => !allowedPublicMethods.Contains(m.Name))
                    .ToList();

                foreach (var method in publicMethods)
                {
                    violations.Add($"{moduleName}.{entityType.Name}.{method.Name}() - should be internal, not public");
                }
            }
        }

        Assert.True(violations.Count == 0,
            $"Non-aggregate entities should not have public methods (only aggregate roots can expose public methods):\n" +
            $"{string.Join("\n", violations)}");
    }

    /// <summary>
    /// Aggregate roots (entities implementing IAggregateRoot) CAN have public methods.
    /// This test documents the allowed pattern.
    /// </summary>
    [Fact]
    public void AggregateRoots_CanHavePublicMethods()
    {
        // Aggregate roots ARE allowed to have public methods because:
        // - They are the entry points to aggregates
        // - External code interacts with aggregates through the root
        // - Example: Order.Place(), Order.AddLine(), Customer.Update()
        //
        // Non-aggregate entities should use internal methods because:
        // - They should only be modified through their aggregate root
        // - Example: OrderLine.Create() is internal, called by Order.AddLine()

        Assert.True(true, "Aggregate root method visibility rules documented above");
    }

    private static bool IsEntityType(Type type)
    {
        // Check if the type inherits from Entity (directly or through SoftDeletableEntity, AuditableEntity)
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType == typeof(Entity))
            {
                return true;
            }
            baseType = baseType.BaseType;
        }
        return false;
    }

    #endregion
}
