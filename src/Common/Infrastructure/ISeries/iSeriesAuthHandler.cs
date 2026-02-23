using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Rtl.Core.Application.Secrets;

namespace Rtl.Core.Infrastructure.ISeries;

#pragma warning disable IDE1006 // Naming Styles
internal sealed class iSeriesAuthHandler(
    IOptions<iSeriesOptions> options,
    ISecretProvider secretProvider) : DelegatingHandler
#pragma warning restore IDE1006 // Naming Styles
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var opts = options.Value;

        if (!string.IsNullOrEmpty(opts.SigningKeySecretName))
        {
            var signingKey = await secretProvider.GetSecretStringAsync(
                opts.SigningKeySecretName, cancellationToken);

            if (!string.IsNullOrEmpty(signingKey))
            {
                var key = Convert.FromBase64String(signingKey);
                request.Headers.Authorization = new AuthenticationHeaderValue(
                    "Bearer", CreateToken(key, opts));
            }
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private static string CreateToken(byte[] key, iSeriesOptions opts)
    {
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = opts.ValidIssuer,
            Audience = opts.ValidAudience,
            Expires = DateTime.UtcNow.AddMinutes(opts.TokenLifetimeMinutes),
            SigningCredentials = credentials,
            Subject = new ClaimsIdentity([new Claim("sub", opts.ValidIssuer)])
        };

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(handler.CreateToken(descriptor));
    }
}
