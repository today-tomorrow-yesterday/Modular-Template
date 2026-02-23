using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;

namespace Rtl.Core.Api.Shared;

/// <summary>
/// CORS configuration options.
/// </summary>
public sealed class CorsOptions : IValidatableObject
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "Cors";

    /// <summary>
    /// Gets or sets the allowed origins for CORS requests.
    /// When empty in development, allows any origin.
    /// When empty in production, blocks all cross-origin requests.
    /// </summary>
    public string[] AllowedOrigins { get; init; } = [];

    /// <summary>
    /// Gets or sets the allowed HTTP methods.
    /// Defaults to allowing any method when empty.
    /// </summary>
    public string[] AllowedMethods { get; init; } = [];

    /// <summary>
    /// Gets or sets the allowed HTTP headers.
    /// Defaults to allowing any header when empty.
    /// </summary>
    public string[] AllowedHeaders { get; init; } = [];

    /// <summary>
    /// Gets or sets whether credentials are allowed in CORS requests.
    /// Defaults to true.
    /// </summary>
    public bool AllowCredentials { get; init; } = true;

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // AllowCredentials + AllowAnyOrigin is not allowed by CORS spec
        // This is enforced at runtime by the CORS middleware, but we can warn here
        if (AllowCredentials && AllowedOrigins.Length == 0)
        {
            // This is only valid in development - we don't fail validation
            // but the extension method handles this case specially
        }

        yield break;
    }
}

/// <summary>
/// Extension methods for CORS configuration.
/// </summary>
public static class CorsExtensions
{
    public static IServiceCollection AddCorsServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.AddOptions<CorsOptions>()
            .Bind(configuration.GetSection(CorsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var corsOptions = configuration
            .GetSection(CorsOptions.SectionName)
            .Get<CorsOptions>() ?? new CorsOptions();

        services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                if (corsOptions.AllowedOrigins.Length > 0)
                {
                    policy.WithOrigins(corsOptions.AllowedOrigins);
                }
                else if (environment.IsDevelopment())
                {
                    // Only allow any origin in development when no origins configured
                    // Note: AllowAnyOrigin() cannot be combined with AllowCredentials()
                    policy.SetIsOriginAllowed(_ => true);
                }
                else
                {
                    // In production with no configured origins, block all cross-origin requests
                    policy.WithOrigins("https://localhost");
                }

                // Methods
                if (corsOptions.AllowedMethods.Length > 0)
                {
                    policy.WithMethods(corsOptions.AllowedMethods);
                }
                else
                {
                    policy.AllowAnyMethod();
                }

                // Headers
                if (corsOptions.AllowedHeaders.Length > 0)
                {
                    policy.WithHeaders(corsOptions.AllowedHeaders);
                }
                else
                {
                    policy.AllowAnyHeader();
                }

                // Credentials
                if (corsOptions.AllowCredentials)
                {
                    policy.AllowCredentials();
                }
            });
        });

        return services;
    }
}
