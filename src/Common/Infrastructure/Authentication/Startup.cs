using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularTemplate.Infrastructure.Application;

namespace ModularTemplate.Infrastructure.Authentication;

/// <summary>
/// Extension methods for configuring authentication services.
/// </summary>
internal static class Startup
{
    /// <summary>
    /// Adds authentication services to the service collection.
    /// </summary>
    internal static IServiceCollection AddAuthenticationServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register AuthenticationOptions from config section with validation
        services.AddOptions<AuthenticationOptions>()
            .Bind(configuration.GetSection(AuthenticationOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Register post-configure to derive values from ApplicationOptions
        services.ConfigureOptions<ConfigureAuthenticationOptions>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        // Configure JwtBearerOptions from our AuthenticationOptions
        services.ConfigureOptions<JwtBearerConfigureOptions>();

        return services;
    }
}
