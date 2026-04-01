using System.Security.Claims;

namespace ModularTemplate.Infrastructure.Authentication;

/// <summary>
/// Extension methods for ClaimsPrincipal.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the user's identity ID from claims.
    /// </summary>
    public static string GetIdentityId(this ClaimsPrincipal principal) =>
        principal.FindFirstValue(CustomClaims.Sub) ??
        throw new InvalidOperationException("User identity ID not found");

    /// <summary>
    /// Gets the user's permissions from claims.
    /// </summary>
    public static HashSet<string> GetPermissions(this ClaimsPrincipal principal)
    {
        IEnumerable<Claim> permissionClaims = principal.FindAll(CustomClaims.Permission);
        return [.. permissionClaims.Select(c => c.Value)];
    }
}
