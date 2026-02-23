using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Rtl.Core.Api.Shared;

/// <summary>
/// Extension methods for API versioning configuration.
/// </summary>
public static class ApiVersioningExtensions
{
    /// <summary>
    /// Adds API versioning services with URL segment versioning strategy.
    /// </summary>
    public static IServiceCollection AddApiVersioningServices(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            // Set the default API version (used when client doesn't specify)
            options.DefaultApiVersion = new ApiVersion(1, 0);

            // Assume default version when client doesn't specify one
            // Set to false in production to enforce explicit versioning
            options.AssumeDefaultVersionWhenUnspecified = true;

            // Report available versions in response headers
            // api-supported-versions: 1.0, 2.0
            // api-deprecated-versions: 1.0
            options.ReportApiVersions = true;

            // Use URL segment versioning: /api/v1/orders
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            // Format version as 'v'major.minor (e.g., v1.0, v2.0)
            options.GroupNameFormat = "'v'VVV";

            // Substitute {version:apiVersion} in route templates
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    /// <summary>
    /// Creates an API version set for the application.
    /// </summary>
    public static ApiVersionSet CreateApiVersionSet(this WebApplication app)
    {
        return app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .HasApiVersion(new ApiVersion(2, 0))
            .ReportApiVersions()
            .Build();
    }
}
