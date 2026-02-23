namespace Rtl.Core.Infrastructure.FeatureManagement;

/// <summary>
/// Configuration paths for infrastructure-level feature flags.
/// These flags are defined in the global appsettings.json under Features:Infrastructure.
/// </summary>
public static class InfrastructureFeatures
{
    /// <summary>
    /// Configuration path for the Outbox feature flag.
    /// Controls whether outbox message processing is enabled.
    /// </summary>
    public const string Outbox = "Features:Infrastructure:Outbox";

    /// <summary>
    /// Configuration path for the Inbox feature flag.
    /// Controls whether inbox message processing is enabled.
    /// </summary>
    public const string Inbox = "Features:Infrastructure:Inbox";

    /// <summary>
    /// Configuration path for the Emails feature flag.
    /// Controls whether email sending is enabled.
    /// </summary>
    public const string Emails = "Features:Infrastructure:Emails";

    /// <summary>
    /// Configuration path for the CDC Events feature flag.
    /// Controls whether Change Data Capture event processing is enabled.
    /// </summary>
    public const string CdcEvents = "Features:Infrastructure:CdcEvents";

    /// <summary>
    /// Configuration path for the Background Jobs feature flag.
    /// Controls whether background job processing is enabled.
    /// </summary>
    public const string BackgroundJobs = "Features:Infrastructure:BackgroundJobs";
}
