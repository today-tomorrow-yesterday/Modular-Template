using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModularTemplate.Application.FeatureManagement;
using ModularTemplate.Domain.Results;
using ModularTemplate.Presentation.Results;

namespace ModularTemplate.Presentation.FeatureManagement;

/// <summary>
/// Endpoint filter that checks if a feature is enabled before allowing the request to proceed.
/// Returns 404 Not Found if the feature is disabled, making the endpoint appear non-existent.
/// </summary>
internal sealed class FeatureFlagEndpointFilter(string featureName) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        if (context.HttpContext.RequestServices
            .GetService(typeof(IFeatureFlagService)) is not IFeatureFlagService featureFlagService)
        {
            var environment = context.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>();

            if (environment.IsDevelopment())
            {
                // Fail open in development only - allows endpoints to work without feature flag setup
                return await next(context);
            }

            // Fail CLOSED in production - protected endpoints must not be accessible
            // if we cannot verify their feature flag status
            throw new InvalidOperationException(
                $"IFeatureFlagService is not registered. Cannot evaluate feature flag '{featureName}'. " +
                "Feature-protected endpoints are inaccessible when feature flag service is unavailable. " +
                "Ensure AddFeatureFlags() is called during service registration.");
        }

        var isEnabled = await featureFlagService.IsEnabledAsync(featureName, context.HttpContext.RequestAborted);

        if (!isEnabled)
        {
            return ApiResponse.Problem(
                Error.NotFound("Feature.Disabled", "The requested resource was not found."));
        }

        return await next(context);
    }
}
