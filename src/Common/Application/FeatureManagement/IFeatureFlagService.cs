namespace Rtl.Core.Application.FeatureManagement;

/// <summary>
/// Service for checking if features are enabled.
/// Features are configured in IConfiguration using dot-notation paths.
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Checks if a feature is enabled asynchronously.
    /// </summary>
    /// <param name="featureName">The configuration path to the feature (e.g., "Sales:Features:CatalogV2Pagination").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>True if the feature is enabled, false otherwise.</returns>
    Task<bool> IsEnabledAsync(string featureName, CancellationToken ct = default);

    /// <summary>
    /// Checks if a feature is enabled synchronously.
    /// </summary>
    /// <param name="featureName">The configuration path to the feature (e.g., "Sales:Features:CatalogV2Pagination").</param>
    /// <returns>True if the feature is enabled, false otherwise.</returns>
    bool IsEnabled(string featureName);
}
