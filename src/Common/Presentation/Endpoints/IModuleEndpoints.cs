using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Rtl.Core.Presentation.Endpoints;

/// <summary>
/// Interface for module-level endpoint registration with API versioning support.
/// Each module implements this to expose all its resources.
/// </summary>
/// <remarks>
/// When extracted to a microservice, the module's endpoints can be
/// registered with a single call to MapEndpoints().
/// </remarks>
public interface IModuleEndpoints
{
    /// <summary>
    /// The module's URL prefix (e.g., "sample", "orders", "inventory").
    /// Used for Swagger/OpenAPI organization.
    /// </summary>
    string ModulePrefix { get; }

    /// <summary>
    /// The module name for Swagger/OpenAPI documentation.
    /// This creates a separate schema in the Swagger dropdown.
    /// </summary>
    string ModuleName { get; }

    /// <summary>
    /// Maps all module endpoints to the application with versioning.
    /// </summary>
    /// <param name="app">The endpoint route builder.</param>
    /// <param name="versionSet">The API version set for versioned routing.</param>
    /// <returns>The endpoint route builder for chaining.</returns>
    IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder app, ApiVersionSet versionSet);
}

/// <summary>
/// Base class for module endpoint registration with API versioning support.
/// Provides common functionality for registering resources.
/// </summary>
public abstract class ModuleEndpoints : IModuleEndpoints
{
    public abstract string ModulePrefix { get; }

    public abstract string ModuleName { get; }

    public IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        // Create versioned route group: /api/v{version:apiVersion}
        var versionedGroup = app
            .MapGroup("api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet);

        foreach (var (resourcePath, tag, endpoints) in GetResources())
        {
            var resourceGroup = versionedGroup.MapGroup(resourcePath);

            // Apply tags and group name for Swagger organization
            resourceGroup.WithTags(tag);
            resourceGroup.WithGroupName(ModulePrefix); // Links to module's Swagger schema

            endpoints.MapEndpoints(resourceGroup);
        }

        return app;
    }

    /// <summary>
    /// Returns all resources in this module.
    /// Override this to register your module's resources.
    /// </summary>
    /// <returns>Collection of (resourcePath, swaggerTag, endpoints) tuples.</returns>
    protected abstract IEnumerable<(string ResourcePath, string Tag, IResourceEndpoints Endpoints)> GetResources();
}
