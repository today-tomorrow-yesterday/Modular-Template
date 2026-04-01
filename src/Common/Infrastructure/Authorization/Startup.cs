using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ModularTemplate.Application.Authorization;

namespace ModularTemplate.Infrastructure.Authorization;

/// <summary>
/// Extension methods for configuring authorization services.
/// </summary>
internal static class Startup
{
    /// <summary>
    /// Adds authorization services to the service collection.
    /// </summary>
    internal static IServiceCollection AddAuthorizationServices(this IServiceCollection services)
    {
        // Add ASP.NET Core authorization services
        services.AddAuthorization();

        // Register default permission service (can be overridden by modules)
        services.TryAddScoped<IPermissionService, DefaultPermissionService>();

        services.AddTransient<IClaimsTransformation, CustomClaimsTransformation>();
        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return services;
    }
}
