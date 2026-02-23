using Rtl.Core.Infrastructure.Authentication;
using System.Security.Claims;
using Xunit;

namespace Rtl.Core.Infrastructure.Tests.Authentication;

public class ClaimsPrincipalExtensionsTests
{
    [Fact]
    public void GetIdentityId_WhenSubClaimExists_ReturnsValue()
    {
        var claims = new[] { new Claim(CustomClaims.Sub, "user-123") };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var result = principal.GetIdentityId();

        Assert.Equal("user-123", result);
    }

    [Fact]
    public void GetIdentityId_WhenSubClaimMissing_ThrowsInvalidOperationException()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity());

        Assert.Throws<InvalidOperationException>(() => principal.GetIdentityId());
    }

    [Fact]
    public void GetPermissions_ReturnsAllPermissionClaims()
    {
        var claims = new[]
        {
            new Claim(CustomClaims.Permission, "read"),
            new Claim(CustomClaims.Permission, "write"),
            new Claim("other-claim", "value")
        };
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims));

        var result = principal.GetPermissions();

        Assert.Equal(2, result.Count);
        Assert.Contains("read", result);
        Assert.Contains("write", result);
    }
}
