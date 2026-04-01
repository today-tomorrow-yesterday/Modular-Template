using Microsoft.Extensions.Configuration;

namespace ModularTemplate.Api.Shared;

/// <summary>
/// Extension methods for configuring module-specific settings.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Adds module-specific configuration from per-module host projects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Loads configuration from each module's dedicated API host project:
    /// - ModularTemplate.Api.{Module}/appsettings.json (base configuration)
    /// - ModularTemplate.Api.{Module}/appsettings.{environment}.json (environment overrides)
    /// </para>
    /// <para>
    /// This keeps module configuration in a single source of truth (the per-module host projects)
    /// while allowing the main API to run all modules locally with consistent settings.
    /// </para>
    /// </remarks>
    public static void AddModuleConfiguration(
        this IConfigurationBuilder configurationBuilder,
        string[] modules,
        string environment)
    {
        // Get the directory where the main API project is located
        var basePath = Directory.GetCurrentDirectory();
        var apiDirectory = Directory.GetParent(basePath)?.FullName ?? basePath;

        foreach (var module in modules)
        {
            // Convert module name to PascalCase for project folder name
            var modulePascal = char.ToUpperInvariant(module[0]) + module[1..];
            var moduleHostPath = Path.Combine(apiDirectory, $"ModularTemplate.Api.{modulePascal}");

            // Base module config (optional - don't fail if module host doesn't exist yet)
            var baseConfigPath = Path.Combine(moduleHostPath, "appsettings.json");
            if (File.Exists(baseConfigPath))
            {
                configurationBuilder.AddJsonFile(baseConfigPath, optional: true, reloadOnChange: true);
            }

            // Environment-specific override (optional)
            var envConfigPath = Path.Combine(moduleHostPath, $"appsettings.{environment}.json");
            if (File.Exists(envConfigPath))
            {
                configurationBuilder.AddJsonFile(envConfigPath, optional: true, reloadOnChange: true);
            }
        }
    }
}
