using Rtl.Core.Domain.Results;

namespace Rtl.Core.Application.FeatureManagement;

/// <summary>
/// Error definitions for feature flag related errors.
/// </summary>
public static class FeatureErrors
{
    /// <summary>
    /// Creates an error indicating that a feature is disabled.
    /// </summary>
    /// <returns>An error with code "Feature.Disabled".</returns>
    public static Error FeatureDisabled(string featureName) =>
        Error.Failure("Feature.Disabled", $"The feature '{featureName}' is currently disabled.");
}
