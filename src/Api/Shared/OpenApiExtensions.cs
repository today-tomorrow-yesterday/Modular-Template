using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using Rtl.Core.Presentation.Endpoints;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Rtl.Core.Api.Shared;

/// <summary>
/// Extension methods for OpenAPI/Swagger configuration.
/// </summary>
public static class OpenApiExtensions
{
    /// <summary>
    /// Adds OpenAPI/Swagger services for a single module host.
    /// </summary>
    public static IServiceCollection AddOpenApiForModule(
        this IServiceCollection services,
        string moduleName,
        IModuleEndpoints moduleEndpoints)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(moduleEndpoints.ModulePrefix, new OpenApiInfo
            {
                Title = $"{moduleName} API",
                Version = "v1",
                Description = $"API endpoints for the {moduleName} module"
            });

            options.DocInclusionPredicate((docName, apiDesc) =>
            {
                var groupName = apiDesc.ActionDescriptor.EndpointMetadata
                    .OfType<EndpointGroupNameAttribute>()
                    .FirstOrDefault()?.EndpointGroupName;

                return string.Equals(groupName, docName, StringComparison.OrdinalIgnoreCase);
            });

            ApplyCommonOptions(options);
        });

        return services;
    }

    /// <summary>
    /// Adds simple OpenAPI/Swagger services when module endpoints are not available at service registration.
    /// </summary>
    public static IServiceCollection AddOpenApiSimple(
        this IServiceCollection services,
        string moduleName)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = $"{moduleName} API",
                Version = "v1",
                Description = $"API endpoints for the {moduleName} module"
            });

            ApplyCommonOptions(options);
        });

        return services;
    }

    /// <summary>
    /// Adds OpenAPI/Swagger services for the main API host with multiple modules.
    /// Each module gets its own Swagger document accessible via dropdown.
    /// </summary>
    public static IServiceCollection AddOpenApiVersioned(
        this IServiceCollection services,
        string apiTitle,
        params IModuleEndpoints[] modules)
    {
        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            foreach (var module in modules)
            {
                options.SwaggerDoc(module.ModulePrefix, new OpenApiInfo
                {
                    Title = $"{apiTitle} - {module.ModuleName}",
                    Version = "v1",
                    Description = $"API endpoints for the {module.ModuleName} module"
                });
            }

            options.DocInclusionPredicate((docName, apiDesc) =>
            {
                var groupName = apiDesc.ActionDescriptor.EndpointMetadata
                    .OfType<EndpointGroupNameAttribute>()
                    .FirstOrDefault()?.EndpointGroupName;

                return string.Equals(groupName, docName, StringComparison.OrdinalIgnoreCase);
            });

            ApplyCommonOptions(options);
        });

        return services;
    }

    /// <summary>
    /// Uses OpenAPI/Swagger middleware for a single module host.
    /// </summary>
    public static IApplicationBuilder UseOpenApiForModule(
        this WebApplication app,
        IModuleEndpoints moduleEndpoints)
    {
        if (app.Environment.IsDevelopment())
        {
            var versionProvider = app.Services.GetService<IApiVersionDescriptionProvider>();
            var currentVersion = versionProvider?.ApiVersionDescriptions
                .FirstOrDefault(v => !v.IsDeprecated)?.ApiVersion.ToString() ?? "v1";

            app.UseSwagger(options => options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0);
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(
                    $"/swagger/{moduleEndpoints.ModulePrefix}/swagger.json",
                    $"{moduleEndpoints.ModuleName} ({currentVersion})");

                options.DisplayRequestDuration();
                options.EnableDeepLinking();
            });
        }

        return app;
    }

    /// <summary>
    /// Uses simple OpenAPI/Swagger middleware.
    /// </summary>
    public static IApplicationBuilder UseOpenApiSimple(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger(options => options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0);
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                options.DisplayRequestDuration();
                options.EnableDeepLinking();
            });
        }

        return app;
    }

    /// <summary>
    /// Uses OpenAPI/Swagger middleware for the main API host with multiple modules.
    /// Creates a dropdown in Swagger UI to switch between module APIs.
    /// </summary>
    public static IApplicationBuilder UseOpenApiVersioned(
        this WebApplication app,
        params IModuleEndpoints[] modules)
    {
        if (app.Environment.IsDevelopment())
        {
            var versionProvider = app.Services.GetService<IApiVersionDescriptionProvider>();
            var currentVersion = versionProvider?.ApiVersionDescriptions
                .FirstOrDefault(v => !v.IsDeprecated)?.ApiVersion.ToString() ?? "v1";

            app.UseSwagger(options =>
            {
                options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_0;
            });
            app.UseSwaggerUI(options =>
            {
                foreach (var module in modules)
                {
                    options.SwaggerEndpoint(
                        $"/swagger/{module.ModulePrefix}/swagger.json",
                        $"{module.ModuleName} ({currentVersion})");
                }

                options.DisplayRequestDuration();
                options.EnableDeepLinking();
            });
        }

        return app;
    }

    private static void ApplyCommonOptions(SwaggerGenOptions options)
    {
        options.CustomOperationIds(apiDesc =>
            apiDesc.ActionDescriptor.EndpointMetadata
                .OfType<EndpointNameMetadata>()
                .FirstOrDefault()?.EndpointName
            ?? (apiDesc.TryGetMethodInfo(out var methodInfo)
                ? methodInfo.Name
                : null));

        // IOperationFilter docs: https://github.com/domaindrivendev/Swashbuckle.AspNetCore?tab=readme-ov-file#operation-filters
        options.OperationFilter<RequestBodyExampleOperationFilter>();
        options.OperationFilter<SeedParameterExampleFilter>();
    }
}
