using System.Reflection;
using System.Text.RegularExpressions;

namespace ModularTemplate.ArchitectureTests;

/// <summary>
/// Base class providing auto-discovery of assemblies for architecture tests.
///
/// NAMING CONVENTION CONTRACT:
/// This test framework auto-discovers modules based on strict naming conventions.
///
/// Common layers must follow: ModularTemplate.{Layer}
///   - e.g., ModularTemplate.Domain
///   - e.g., ModularTemplate.Application
///
/// Module layers must follow: Modules.{ModuleName}.{Layer}
///   - e.g., Modules.SampleOrders.Domain
///   - e.g., Modules.SampleSales.Application
///
/// If you follow these conventions, all architecture tests will automatically
/// validate your modules without any manual configuration.
/// </summary>
public abstract class BaseTest
{
    // Pattern to extract module info from assembly names
    // Matches: Modules.{ModuleName}.{Layer}
    private static readonly Regex ModuleAssemblyPattern = new(
        @"^Modules\.([^.]+)\.(Domain|Application|Infrastructure|Presentation|IntegrationEvents)$",
        RegexOptions.Compiled);

    // Pattern for Common assemblies
    // Matches: ModularTemplate.{Layer}
    private static readonly Regex CommonAssemblyPattern = new(
        @"^(ModularTemplate)\.(Domain|Application|Infrastructure|Presentation)$",
        RegexOptions.Compiled);

    #region Lazy-loaded Assembly Discovery

    private static readonly Lazy<AssemblyInfo> _assemblyInfo = new(DiscoverAssemblies);

    protected static AssemblyInfo Assemblies => _assemblyInfo.Value;

    #endregion

    #region Common Layer Assemblies (convenience properties)

    protected static Assembly? CommonDomainAssembly => Assemblies.CommonDomain;
    protected static Assembly? CommonApplicationAssembly => Assemblies.CommonApplication;
    protected static Assembly? CommonInfrastructureAssembly => Assemblies.CommonInfrastructure;
    protected static Assembly? CommonPresentationAssembly => Assemblies.CommonPresentation;

    #endregion

    #region Module Discovery

    /// <summary>
    /// Gets all discovered module names.
    /// </summary>
    protected static IReadOnlyList<string> ModuleNames => Assemblies.ModuleNames;

    /// <summary>
    /// Gets all module assemblies for a specific layer.
    /// </summary>
    protected static IReadOnlyList<(string ModuleName, Assembly Assembly)> GetModuleAssemblies(string layer)
    {
        return layer switch
        {
            "Domain" => Assemblies.ModuleDomains,
            "Application" => Assemblies.ModuleApplications,
            "Infrastructure" => Assemblies.ModuleInfrastructures,
            "Presentation" => Assemblies.ModulePresentations,
            "IntegrationEvents" => Assemblies.ModuleIntegrationEvents,
            _ => throw new ArgumentException($"Unknown layer: {layer}")
        };
    }

    #endregion

    #region Namespace Helpers

    /// <summary>
    /// Gets the namespace prefix discovered from assemblies (e.g., "ModularTemplate").
    /// </summary>
    protected static string NamespacePrefix => Assemblies.NamespacePrefix;

    /// <summary>
    /// Gets all module root namespaces (e.g., ["Modules.SampleOrders", "Modules.SampleSales"]).
    /// </summary>
    protected static string[] GetAllModuleNamespaces()
    {
        return [.. Assemblies.ModuleNames.Select(m => $"Modules.{m}")];
    }

    /// <summary>
    /// Gets other module namespaces excluding the specified module.
    /// </summary>
    protected static string[] GetOtherModuleNamespaces(string excludeModule)
    {
        return [.. Assemblies.ModuleNames
            .Where(m => m != excludeModule)
            .Select(m => $"Modules.{m}")];
    }

    /// <summary>
    /// Gets other modules' namespaces excluding IntegrationEvents (which ARE allowed as cross-module dependencies).
    /// Returns Domain, Application, Infrastructure, and Presentation namespaces for other modules.
    /// </summary>
    protected static string[] GetOtherModuleNonEventNamespaces(string excludeModule)
    {
        var nonEventLayers = new[] { "Domain", "Application", "Infrastructure", "Presentation" };
        return [.. Assemblies.ModuleNames
            .Where(m => m != excludeModule)
            .SelectMany(m => nonEventLayers.Select(layer => $"Modules.{m}.{layer}"))];
    }

    /// <summary>
    /// Gets the Common namespace for a layer.
    /// </summary>
    protected static string GetCommonNamespace(string layer)
        => $"{NamespacePrefix}.{layer}";

    /// <summary>
    /// Gets the module namespace for a specific module and layer.
    /// </summary>
    protected static string GetModuleNamespace(string moduleName, string layer)
        => $"Modules.{moduleName}.{layer}";

    #endregion

    #region Assembly Discovery Implementation

    private static AssemblyInfo DiscoverAssemblies()
    {
        // First, load assemblies from the bin directory that match our naming patterns
        LoadAssembliesFromBinDirectory();

        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies()
            .Where(a => !a.IsDynamic && !string.IsNullOrEmpty(a.FullName))
            .ToList();

        // Also try to find referenced assemblies that haven't been loaded yet
        var referencedAssemblies = allAssemblies
            .SelectMany(a =>
            {
                try { return a.GetReferencedAssemblies(); }
                catch { return Array.Empty<AssemblyName>(); }
            })
            .Where(an => an.Name != null &&
                (an.Name.StartsWith("ModularTemplate.") || an.Name.StartsWith("Modules.")))
            .Distinct()
            .ToList();

        foreach (var assemblyName in referencedAssemblies)
        {
            try
            {
                var loaded = Assembly.Load(assemblyName);
                if (!allAssemblies.Contains(loaded))
                {
                    allAssemblies.Add(loaded);
                }
            }
            catch
            {
                // Assembly couldn't be loaded - skip it
            }
        }

        string? namespacePrefix = null;

        // Discover Common assemblies
        Assembly? commonDomain = null;
        Assembly? commonApplication = null;
        Assembly? commonInfrastructure = null;
        Assembly? commonPresentation = null;

        foreach (var assembly in allAssemblies)
        {
            var name = assembly.GetName().Name ?? "";
            var match = CommonAssemblyPattern.Match(name);
            if (match.Success)
            {
                namespacePrefix ??= match.Groups[1].Value;
                var layer = match.Groups[2].Value;

                switch (layer)
                {
                    case "Domain": commonDomain = assembly; break;
                    case "Application": commonApplication = assembly; break;
                    case "Infrastructure": commonInfrastructure = assembly; break;
                    case "Presentation": commonPresentation = assembly; break;
                }
            }
        }

        // Discover Module assemblies
        var moduleDomains = new List<(string, Assembly)>();
        var moduleApplications = new List<(string, Assembly)>();
        var moduleInfrastructures = new List<(string, Assembly)>();
        var modulePresentations = new List<(string, Assembly)>();
        var moduleIntegrationEvents = new List<(string, Assembly)>();
        var moduleNames = new HashSet<string>();

        foreach (var assembly in allAssemblies)
        {
            var name = assembly.GetName().Name ?? "";
            var match = ModuleAssemblyPattern.Match(name);
            if (match.Success)
            {
                var moduleName = match.Groups[1].Value;
                var layer = match.Groups[2].Value;

                moduleNames.Add(moduleName);

                switch (layer)
                {
                    case "Domain": moduleDomains.Add((moduleName, assembly)); break;
                    case "Application": moduleApplications.Add((moduleName, assembly)); break;
                    case "Infrastructure": moduleInfrastructures.Add((moduleName, assembly)); break;
                    case "Presentation": modulePresentations.Add((moduleName, assembly)); break;
                    case "IntegrationEvents": moduleIntegrationEvents.Add((moduleName, assembly)); break;
                }
            }
        }

        return new AssemblyInfo
        {
            NamespacePrefix = namespacePrefix ?? "Unknown",
            CommonDomain = commonDomain,
            CommonApplication = commonApplication,
            CommonInfrastructure = commonInfrastructure,
            CommonPresentation = commonPresentation,
            ModuleNames = moduleNames.OrderBy(n => n).ToList(),
            ModuleDomains = moduleDomains,
            ModuleApplications = moduleApplications,
            ModuleInfrastructures = moduleInfrastructures,
            ModulePresentations = modulePresentations,
            ModuleIntegrationEvents = moduleIntegrationEvents
        };
    }

    private static void LoadAssembliesFromBinDirectory()
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var dllFiles = Directory.GetFiles(baseDirectory, "*.dll");

        foreach (var dllFile in dllFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(dllFile);

            // Only load assemblies matching our patterns
            if (!CommonAssemblyPattern.IsMatch(fileName) &&
                !ModuleAssemblyPattern.IsMatch(fileName))
            {
                continue;
            }

            try
            {
                // Check if already loaded
                var assemblyName = AssemblyName.GetAssemblyName(dllFile);
                var alreadyLoaded = AppDomain.CurrentDomain.GetAssemblies()
                    .Any(a => !a.IsDynamic && a.GetName().Name == assemblyName.Name);

                if (!alreadyLoaded)
                {
                    Assembly.LoadFrom(dllFile);
                }
            }
            catch
            {
                // Skip assemblies that can't be loaded
            }
        }
    }

    #endregion

    #region Assembly Info Record

    protected sealed class AssemblyInfo
    {
        public required string NamespacePrefix { get; init; }

        public Assembly? CommonDomain { get; init; }
        public Assembly? CommonApplication { get; init; }
        public Assembly? CommonInfrastructure { get; init; }
        public Assembly? CommonPresentation { get; init; }

        public required IReadOnlyList<string> ModuleNames { get; init; }
        public required IReadOnlyList<(string ModuleName, Assembly Assembly)> ModuleDomains { get; init; }
        public required IReadOnlyList<(string ModuleName, Assembly Assembly)> ModuleApplications { get; init; }
        public required IReadOnlyList<(string ModuleName, Assembly Assembly)> ModuleInfrastructures { get; init; }
        public required IReadOnlyList<(string ModuleName, Assembly Assembly)> ModulePresentations { get; init; }
        public required IReadOnlyList<(string ModuleName, Assembly Assembly)> ModuleIntegrationEvents { get; init; }
    }

    #endregion
}
