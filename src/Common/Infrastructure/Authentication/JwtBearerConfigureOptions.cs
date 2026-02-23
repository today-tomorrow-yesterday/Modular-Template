using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Rtl.Core.Infrastructure.Authentication;

/// <summary>
/// Configures JWT Bearer authentication options from <see cref="AuthenticationOptions"/>.
/// </summary>
internal sealed class JwtBearerConfigureOptions(IOptions<AuthenticationOptions> authOptions)
    : IConfigureNamedOptions<JwtBearerOptions>
{
    private readonly AuthenticationOptions _authOptions = authOptions.Value;

    public void Configure(JwtBearerOptions options) => Configure(string.Empty, options);

    public void Configure(string? name, JwtBearerOptions options)
    {
        options.Audience = _authOptions.Audience;
        options.Authority = _authOptions.GetAuthority();
        options.RequireHttpsMetadata = _authOptions.RequireHttpsMetadata;
        options.TokenValidationParameters ??= new TokenValidationParameters();
        options.TokenValidationParameters.ValidateIssuer = _authOptions.ValidateIssuer;
    }
}
