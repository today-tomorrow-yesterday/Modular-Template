using System.ComponentModel.DataAnnotations;

namespace Rtl.Core.Infrastructure.Authentication;

/// <summary>
/// Authentication configuration options for OpenID Connect / OAuth 2.0.
/// </summary>
/// <remarks>
/// Supports Azure AD, Auth0, Okta, or any OIDC-compliant provider.
/// <list type="bullet">
///   <item><c>Audience</c>: Defaults to {Application:ShortName}-api</item>
///   <item><c>Authority</c>: The identity provider URL (e.g., https://login.microsoftonline.com/{tenant-id}/v2.0)</item>
/// </list>
/// </remarks>
public sealed class AuthenticationOptions : IValidatableObject
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "Authentication";

    /// <summary>
    /// Gets or sets the JWT audience claim.
    /// For Azure AD, this is typically the Application ID URI or client ID.
    /// If not set, derived from Application:ShortName with "-api" suffix.
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identity provider authority URL.
    /// For Azure AD: https://login.microsoftonline.com/{tenant-id}/v2.0
    /// For Auth0: https://{domain}
    /// </summary>
    public string Authority { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Azure AD tenant ID (optional, for Azure AD convenience).
    /// If set, Authority is derived automatically.
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether HTTPS is required for metadata retrieval.
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to validate the issuer.
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// Gets the effective authority, deriving from TenantId if Authority is not set.
    /// </summary>
    public string GetAuthority() =>
        !string.IsNullOrEmpty(Authority)
            ? Authority
            : !string.IsNullOrEmpty(TenantId)
                ? $"https://login.microsoftonline.com/{TenantId}/v2.0"
                : string.Empty;

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(Authority) && string.IsNullOrWhiteSpace(TenantId))
        {
            yield return new ValidationResult(
                "Either Authority or TenantId must be configured.",
                [nameof(Authority), nameof(TenantId)]);
        }
    }
}
