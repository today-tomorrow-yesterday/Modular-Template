using ModularTemplate.Domain.Auditing;
using ModularTemplate.Domain.Entities;
using ModularTemplate.Domain.ValueObjects;

namespace Modules.SampleOrders.Domain.Orders;

/// <summary>
/// Abstract base for order line items (TPH — single table with discriminator).
/// Each concrete type stores its type-specific data in a single JSONB 'details' column
/// using IVersionedDetails for forward-compatible schema evolution.
/// </summary>
public abstract class OrderLine : Entity
{
    protected OrderLine() { }

    public int OrderId { get; private set; }

    public int Quantity { get; private set; }

    [SensitiveData]
    public Money UnitPrice { get; private set; } = null!;

    public Money LineTotal => UnitPrice.Multiply(Quantity);

    public int SortOrder { get; private set; }

    protected void SetBaseProperties(int orderId, int quantity, Money unitPrice, int sortOrder = 0)
    {
        OrderId = orderId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        SortOrder = sortOrder;
    }

    internal void UpdateQuantity(int quantity)
    {
        Quantity = quantity;
    }
}

/// <summary>
/// Order line for a product from the SampleSales module's ProductCache.
/// Details snapshot the product data at time of order.
/// </summary>
public sealed class ProductLine : OrderLine
{
    private ProductLine() { }

    public int? ProductCacheId { get; private set; }

    public ProductLineDetails? Details { get; private set; }

    internal static ProductLine Create(
        int orderId,
        int quantity,
        Money unitPrice,
        int? productCacheId,
        ProductLineDetails? details,
        int sortOrder = 0)
    {
        var line = new ProductLine
        {
            ProductCacheId = productCacheId,
            Details = details
        };
        line.SetBaseProperties(orderId, quantity, unitPrice, sortOrder);
        return line;
    }

    internal void UpdateDetails(ProductLineDetails details)
    {
        Details = details;
    }
}

/// <summary>
/// Order line for ad-hoc charges, adjustments, or custom items.
/// </summary>
public sealed class CustomLine : OrderLine
{
    private CustomLine() { }

    public CustomLineDetails? Details { get; private set; }

    internal static CustomLine Create(
        int orderId,
        int quantity,
        Money unitPrice,
        CustomLineDetails? details,
        int sortOrder = 0)
    {
        var line = new CustomLine
        {
            Details = details
        };
        line.SetBaseProperties(orderId, quantity, unitPrice, sortOrder);
        return line;
    }
}
