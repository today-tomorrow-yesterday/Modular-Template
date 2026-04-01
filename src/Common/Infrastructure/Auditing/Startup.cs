using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ModularTemplate.Application.Auditing;
using ModularTemplate.Application.Caching;
using ModularTemplate.Application.Identity;
using ModularTemplate.Infrastructure.Auditing.Interceptors;
using ModularTemplate.Infrastructure.Caching;
using ModularTemplate.Infrastructure.Identity;
using ModularTemplate.Infrastructure.Outbox.Persistence;
using ModularTemplate.Infrastructure.Security;

namespace ModularTemplate.Infrastructure.Auditing;

/// <summary>
/// Extension methods for configuring auditing services.
/// </summary>
internal static class Startup
{
    /// <summary>
    /// Adds auditing services to the service collection.
    /// </summary>
    internal static IServiceCollection AddAuditingServices(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.TryAddScoped<ICurrentUserService, CurrentUserService>();
        services.TryAddScoped<AuditableEntitiesInterceptor>();
        services.TryAddScoped<SoftDeleteInterceptor>();
        services.TryAddSingleton<InsertOutboxMessagesInterceptor>();
        services.TryAddScoped<ICacheWriteScope, CacheWriteScope>();
        services.TryAddScoped<CacheWriteGuardInterceptor>();
        services.TryAddSingleton<IEncryptionService, AesEncryptionService>();

        // Audit trail services
        services.TryAddScoped<AuditTrailInterceptor>();
        services.TryAddScoped<IAuditContext, AuditContext>();

        return services;
    }
}
