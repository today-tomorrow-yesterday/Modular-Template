using Microsoft.Extensions.Options;
using Rtl.Core.Infrastructure.ISeries;
using System.IdentityModel.Tokens.Jwt;
using Xunit;

namespace Rtl.Core.Infrastructure.Tests.ISeries;

#pragma warning disable IDE1006 // Naming Styles
public class iSeriesAuthHandlerTests
#pragma warning restore IDE1006
{
    // 32-byte key base64-encoded — valid for HMAC-SHA256
    private const string TestSigningKey = "MTIzNDU2Nzg5MDEyMzQ1Njc4OTAxMjM0NTY3ODkwMTI=";

    private static iSeriesOptions CreateOptions(string? signingKeySecretName = "test-key") => new()
    {
        BaseUrl = "https://test.example.com/api/",
        SigningKeySecretName = signingKeySecretName,
        ValidIssuer = "rtl-core",
        ValidAudience = "iseries-gateway",
        TokenLifetimeMinutes = 5
    };

    private static (HttpClient client, FakeMessageHandler inner, FakeSecretProvider secrets)
        CreateClient(iSeriesOptions? options = null, string? secretToReturn = TestSigningKey)
    {
        var opts = options ?? CreateOptions();
        var secrets = new FakeSecretProvider { SecretToReturn = secretToReturn ?? string.Empty };
        var inner = new FakeMessageHandler();
        var handler = new iSeriesAuthHandler(Options.Create(opts), secrets)
        {
            InnerHandler = inner
        };
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri(opts.BaseUrl)
        };
        return (client, inner, secrets);
    }

    [Fact]
    public async Task SendAsync_Sets_Bearer_Token_When_SigningKey_Available()
    {
        var (client, inner, _) = CreateClient();

        await client.GetAsync("v1/test");

        Assert.NotNull(inner.LastRequest!.Headers.Authorization);
        Assert.Equal("Bearer", inner.LastRequest.Headers.Authorization.Scheme);
        Assert.False(string.IsNullOrEmpty(inner.LastRequest.Headers.Authorization.Parameter));
    }

    [Fact]
    public async Task SendAsync_No_Auth_When_SigningKeySecretName_Is_Null()
    {
        var (client, inner, secrets) = CreateClient(CreateOptions(signingKeySecretName: null));

        await client.GetAsync("v1/test");

        Assert.Null(inner.LastRequest!.Headers.Authorization);
        Assert.Equal(0, secrets.CallCount);
    }

    [Fact]
    public async Task SendAsync_No_Auth_When_SigningKeySecretName_Is_Empty()
    {
        var (client, inner, secrets) = CreateClient(CreateOptions(signingKeySecretName: ""));

        await client.GetAsync("v1/test");

        Assert.Null(inner.LastRequest!.Headers.Authorization);
        Assert.Equal(0, secrets.CallCount);
    }

    [Fact]
    public async Task SendAsync_No_Auth_When_Secret_Returns_Empty()
    {
        var (client, inner, _) = CreateClient(secretToReturn: "");

        await client.GetAsync("v1/test");

        Assert.Null(inner.LastRequest!.Headers.Authorization);
    }

    [Fact]
    public async Task SendAsync_Jwt_Contains_Correct_Issuer_And_Audience()
    {
        var (client, inner, _) = CreateClient();

        await client.GetAsync("v1/test");

        var token = inner.LastRequest!.Headers.Authorization!.Parameter!;
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("rtl-core", jwt.Issuer);
        Assert.Contains("iseries-gateway", jwt.Audiences);
    }

    [Fact]
    public async Task SendAsync_Jwt_Contains_Subject_Claim()
    {
        var (client, inner, _) = CreateClient();

        await client.GetAsync("v1/test");

        var token = inner.LastRequest!.Headers.Authorization!.Parameter!;
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("rtl-core", jwt.Subject);
    }

    [Fact]
    public async Task SendAsync_Jwt_Uses_HmacSha256()
    {
        var (client, inner, _) = CreateClient();

        await client.GetAsync("v1/test");

        var token = inner.LastRequest!.Headers.Authorization!.Parameter!;
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

        Assert.Equal("HS256", jwt.Header.Alg);
    }

    [Fact]
    public async Task SendAsync_Fetches_Correct_Secret_Name()
    {
        var opts = CreateOptions(signingKeySecretName: "my-custom-secret");
        var (client, _, secrets) = CreateClient(opts);

        await client.GetAsync("v1/test");

        Assert.Equal(1, secrets.CallCount);
        Assert.Equal("my-custom-secret", secrets.LastSecretName);
    }

    [Fact]
    public async Task SendAsync_Forwards_Request_To_Inner_Handler()
    {
        var (client, inner, _) = CreateClient();

        await client.GetAsync("v1/test");

        Assert.Equal(1, inner.CallCount);
        Assert.Contains("v1/test", inner.LastRequest!.RequestUri!.ToString());
    }
}
