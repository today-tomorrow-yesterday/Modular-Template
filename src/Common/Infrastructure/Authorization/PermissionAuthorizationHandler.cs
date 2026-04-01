using Microsoft.AspNetCore.Authorization;
using ModularTemplate.Infrastructure.Authentication;

namespace ModularTemplate.Infrastructure.Authorization;

/// <summary>
/// Authorization handler that checks for required permissions.
/// </summary>
internal sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        HashSet<string> permissions = context.User.GetPermissions();

        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
