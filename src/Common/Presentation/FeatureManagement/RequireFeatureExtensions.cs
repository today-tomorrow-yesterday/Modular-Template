using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ModularTemplate.Presentation.FeatureManagement;

/// <summary>
/// Extension methods for requiring feature flags on endpoints.
/// </summary>
public static class RequireFeatureExtensions
{
    /// <summary>
    /// Requires that a feature flag is enabled for this endpoint.
    /// If the feature is disabled, the endpoint returns 404 Not Found.
    /// </summary>
    public static RouteHandlerBuilder RequireFeature(this RouteHandlerBuilder builder, string featureName)
    {
        builder.AddEndpointFilter(new FeatureFlagEndpointFilter(featureName));
        return builder;
    }
}
