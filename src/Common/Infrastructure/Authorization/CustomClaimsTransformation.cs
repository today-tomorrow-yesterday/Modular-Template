using Microsoft.AspNetCore.Authentication;
using Rtl.Core.Application.Authorization;
using Rtl.Core.Infrastructure.Authentication;
using System.Security.Claims;

namespace Rtl.Core.Infrastructure.Authorization;

/// <summary>
/// Transforms claims to include user permissions.
/// </summary>
internal sealed class CustomClaimsTransformation(IPermissionService permissionService) : IClaimsTransformation
{
    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.HasClaim(c => c.Type == CustomClaims.Permission))
        {
            return principal;
        }

        string identityId = principal.GetIdentityId();

        var permissionsResult = await permissionService.GetUserPermissionsAsync(identityId);

        if (permissionsResult.IsFailure)
        {
            return principal;
        }

        var claimsIdentity = new ClaimsIdentity();

        foreach (string permission in permissionsResult.Value.Permissions)
        {
            claimsIdentity.AddClaim(new Claim(CustomClaims.Permission, permission));
        }

        principal.AddIdentity(claimsIdentity);

        return principal;
    }
}
