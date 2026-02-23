using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using System.Threading.RateLimiting;

namespace Rtl.Core.Api.Shared;

/// <summary>
/// Configuration options for API rate limiting.
/// </summary>
public sealed class RateLimitingOptions : IValidatableObject
{
    /// <summary>
    /// The configuration section name for rate limiting options.
    /// </summary>
    public const string SectionName = "RateLimiting";

    /// <summary>
    /// Gets the maximum number of requests permitted within the time window.
    /// </summary>
    public int PermitLimit { get; init; } = 100;

    /// <summary>
    /// Gets the time window duration in seconds.
    /// </summary>
    public int WindowInSeconds { get; init; } = 60;

    /// <summary>
    /// Gets the maximum number of requests that can be queued when the limit is reached.
    /// </summary>
    public int QueueLimit { get; init; } = 0;

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PermitLimit <= 0)
        {
            yield return new ValidationResult(
                "PermitLimit must be positive.",
                [nameof(PermitLimit)]);
        }

        if (WindowInSeconds <= 0)
        {
            yield return new ValidationResult(
                "WindowInSeconds must be positive.",
                [nameof(WindowInSeconds)]);
        }

        if (QueueLimit < 0)
        {
            yield return new ValidationResult(
                "QueueLimit cannot be negative.",
                [nameof(QueueLimit)]);
        }
    }
}


/// <summary>
/// Extension methods for rate limiting configuration.
/// </summary>
public static class RateLimitingExtensions
{
    public const string FixedWindowPolicy = "fixed";

    public static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RateLimitingOptions>()
            .Bind(configuration.GetSection(RateLimitingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddRateLimiter(options =>
        {
            var rateLimitingOptions = configuration
                .GetSection(RateLimitingOptions.SectionName)
                .Get<RateLimitingOptions>() ?? new RateLimitingOptions();

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddFixedWindowLimiter(FixedWindowPolicy, limiterOptions =>
            {
                limiterOptions.Window = TimeSpan.FromSeconds(rateLimitingOptions.WindowInSeconds);
                limiterOptions.PermitLimit = rateLimitingOptions.PermitLimit;
                limiterOptions.QueueLimit = rateLimitingOptions.QueueLimit;
                limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });
        });

        return services;
    }
}
