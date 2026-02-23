using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Rtl.Core.Infrastructure.ISeries;

#pragma warning disable IDE1006 // Naming Styles
internal sealed class iSeriesAuthHandler(
    IOptions<iSeriesOptions> options,
    IAmazonSecretsManager? secretsManager = null) : DelegatingHandler
#pragma warning restore IDE1006 // Naming Styles
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);
    private static byte[]? s_cachedKey;
    private static DateTime s_cacheExpiry;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var opts = options.Value;

        if (!string.IsNullOrEmpty(opts.SigningKeySecretName))
        {
            Debug.Assert(secretsManager is not null, "IAmazonSecretsManager must be registered when SigningKeySecretName is configured.");
            var key = await GetSigningKeyAsync(opts.SigningKeySecretName, cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken(key, opts));
        }

        return await base.SendAsync(request, cancellationToken);
    }

    private async Task<byte[]> GetSigningKeyAsync(string secretName, CancellationToken ct)
    {
        if (s_cachedKey is not null && DateTime.UtcNow < s_cacheExpiry)
            return s_cachedKey;

        var response = await secretsManager!.GetSecretValueAsync(
            new GetSecretValueRequest { SecretId = secretName }, ct);

        s_cachedKey = Convert.FromBase64String(response.SecretString);
        s_cacheExpiry = DateTime.UtcNow.Add(CacheDuration);
        return s_cachedKey;
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
