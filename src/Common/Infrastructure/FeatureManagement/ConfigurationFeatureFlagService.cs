using Microsoft.Extensions.Configuration;
using ModularTemplate.Application.FeatureManagement;

namespace ModularTemplate.Infrastructure.FeatureManagement;

/// <summary>
/// Feature flag service that reads feature flags from IConfiguration.
/// Supports hierarchical configuration paths using colon notation (e.g., "Sales:Features:CatalogV2Pagination").
/// </summary>
internal sealed class ConfigurationFeatureFlagService(IConfiguration configuration) : IFeatureFlagService
{
    public Task<bool> IsEnabledAsync(string featureName, CancellationToken ct = default)
        => Task.FromResult(IsEnabled(featureName));

    public bool IsEnabled(string featureName)
    {
        // Get the value from configuration
        var value = configuration[featureName];

        // If the value is not set, default to false (feature disabled)
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        // Parse as boolean, default to false if parsing fails
        return bool.TryParse(value, out var isEnabled) && isEnabled;
    }
}
