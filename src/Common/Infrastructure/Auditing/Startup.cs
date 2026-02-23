using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Rtl.Core.Application.Auditing;
using Rtl.Core.Application.Caching;
using Rtl.Core.Application.Identity;
using Rtl.Core.Infrastructure.Auditing.Interceptors;
using Rtl.Core.Infrastructure.Caching;
using Rtl.Core.Infrastructure.Identity;
using Rtl.Core.Infrastructure.Outbox.Persistence;
using Rtl.Core.Infrastructure.Security;

namespace Rtl.Core.Infrastructure.Auditing;

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
