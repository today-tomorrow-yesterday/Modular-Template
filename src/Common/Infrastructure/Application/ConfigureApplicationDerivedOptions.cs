using Microsoft.Extensions.Options;
using Rtl.Core.Infrastructure.Authentication;
using Rtl.Core.Infrastructure.EventBus.Aws;

namespace Rtl.Core.Infrastructure.Application;

/// <summary>
/// Post-configures AWS messaging options to derive values from <see cref="ApplicationOptions"/>.
/// </summary>
/// <remarks>
/// This enables a single source of truth for application naming. Values are derived
/// from <see cref="ApplicationOptions"/> unless explicitly overridden in configuration.
/// </remarks>
public sealed class ConfigureAwsMessagingOptions(IOptions<ApplicationOptions> applicationOptions)
    : IPostConfigureOptions<AwsMessagingOptions>
{
    private readonly ApplicationOptions _app = applicationOptions.Value;

    public void PostConfigure(string? name, AwsMessagingOptions options)
    {
        // Derive EventBusName from ShortName if not explicitly set
        // Pattern: {shortname}-events (e.g., "retail-core-events")
        if (string.IsNullOrEmpty(options.EventBusName))
        {
            options.EventBusName = $"{_app.ShortName}-events";
        }

        // Derive EventSource from Name if not explicitly set
        // Pattern: lowercase with dots (e.g., "Rtl.Core")
        if (string.IsNullOrEmpty(options.EventSource))
        {
            options.EventSource = _app.Name.ToLowerInvariant();
        }
    }
}

/// <summary>
/// Post-configures authentication options to derive values from <see cref="ApplicationOptions"/>.
/// </summary>
public sealed class ConfigureAuthenticationOptions(IOptions<ApplicationOptions> applicationOptions)
    : IPostConfigureOptions<AuthenticationOptions>
{
    private readonly ApplicationOptions _app = applicationOptions.Value;

    public void PostConfigure(string? name, AuthenticationOptions options)
    {
        // Derive Audience from ShortName if not explicitly set
        // Pattern: {shortname}-api (e.g., "retail-core-api")
        if (string.IsNullOrEmpty(options.Audience))
        {
            options.Audience = $"{_app.ShortName}-api";
        }
    }
}
