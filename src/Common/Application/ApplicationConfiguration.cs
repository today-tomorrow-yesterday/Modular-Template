using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Rtl.Core.Application.Behaviors;
using System.Reflection;

namespace Rtl.Core.Application;

/// <summary>
/// Extension methods for configuring the application layer.
/// </summary>
public static class ApplicationConfiguration
{
    /// <summary>
    /// Adds application layer services including MediatR and FluentValidation.
    /// </summary>
    public static IServiceCollection AddCommonApplication(
        this IServiceCollection services,
        Assembly[] moduleAssemblies)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssemblies(moduleAssemblies);

            // Pipeline order matters! Behaviors wrap in registration order:
            // 1. ExceptionHandling - outermost, catches all unhandled exceptions
            // 2. RequestLogging - logs request start/end and success/failure
            // 3. Validation - validates commands before handler runs, returns Result.Failure for validation errors
            // This ensures validation failures are properly logged as errors (not exceptions)
            config.AddOpenBehavior(typeof(ExceptionHandlingPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
            config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
        });

        services.AddValidatorsFromAssemblies(moduleAssemblies, includeInternalTypes: true);

        return services;
    }
}
