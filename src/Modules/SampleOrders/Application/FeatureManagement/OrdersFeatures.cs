namespace Modules.SampleOrders.Application.FeatureManagement;

/// <summary>
/// Configuration paths for Orders module feature flags.
/// These flags are defined in modules.orders.json under Orders:Features.
/// </summary>
public static class OrdersFeatures
{
    /// <summary>
    /// Configuration path for the Split Shipments feature flag.
    /// Controls whether orders can be split into multiple shipments.
    /// </summary>
    public const string SplitShipments = "Orders:Features:SplitShipments";

    /// <summary>
    /// Configuration path for the Guest Checkout feature flag.
    /// Controls whether users can checkout without creating an account.
    /// </summary>
    public const string GuestCheckout = "Orders:Features:GuestCheckout";
}
