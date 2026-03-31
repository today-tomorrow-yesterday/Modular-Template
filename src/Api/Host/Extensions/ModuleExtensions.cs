using Asp.Versioning.Builder;
using Modules.SampleOrders.Presentation.Endpoints;
using Modules.SampleSales.Presentation.Endpoints;
using Rtl.Core.Presentation.Endpoints;

namespace Rtl.Core.Api.Extensions;

/// <summary>
/// Extension methods for module endpoint registration.
/// </summary>
internal static class ModuleExtensions
{
    /// <summary>
    /// Gets all module endpoint registrations.
    /// Each module has its own Swagger schema and endpoint group.
    /// </summary>
    public static IModuleEndpoints[] GetModuleEndpoints()
    {
        return
        [
            new SampleSalesModuleEndpoints(),
            new SampleOrdersModuleEndpoints(),
        ];
    }

    /// <summary>
    /// Maps all module endpoints with API versioning.
    /// </summary>
    public static IApplicationBuilder MapVersionedModuleEndpoints(
        this WebApplication app,
        ApiVersionSet versionSet,
        params IModuleEndpoints[] modules)
    {
        foreach (var module in modules)
        {
            module.MapEndpoints(app, versionSet);
        }

        return app;
    }
}