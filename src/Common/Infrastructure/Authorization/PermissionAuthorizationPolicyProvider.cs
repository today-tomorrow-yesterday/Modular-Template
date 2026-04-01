using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace ModularTemplate.Infrastructure.Authorization;

/// <summary>
/// Policy provider that creates permission-based authorization policies on demand.
/// </summary>
internal sealed class PermissionAuthorizationPolicyProvider(
    IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    private readonly AuthorizationOptions _options = options.Value;

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        AuthorizationPolicy? policy = await base.GetPolicyAsync(policyName);

        if (policy is not null)
        {
            return policy;
        }

        AuthorizationPolicy permissionPolicy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(policyName))
            .Build();

        _options.AddPolicy(policyName, permissionPolicy);

        return permissionPolicy;
    }
}
