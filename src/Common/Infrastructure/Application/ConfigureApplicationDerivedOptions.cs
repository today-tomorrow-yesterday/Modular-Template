using Microsoft.Extensions.Options;
using ModularTemplate.Infrastructure.Authentication;

namespace ModularTemplate.Infrastructure.Application;

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
        // Pattern: {shortname}-api (e.g., "modular-template-api")
        if (string.IsNullOrEmpty(options.Audience))
        {
            options.Audience = $"{_app.ShortName}-api";
        }
    }
}
